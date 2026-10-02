using System.Reflection;
using AwesomeAssertions;
using NSubstitute;
using RonSijm.Blazyload.IntegrationTests.Helpers;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.IntegrationTests.Tests;

/// <summary>
/// Cascade Loading Integration Tests
/// Tests loading parent assembly with dependencies, verifying all dependent assemblies load automatically
/// </summary>
public class CascadeLoadingIntegrationTests
{
    [Fact]
    public async Task LoadAssemblyAsync_WithMissingRequiredReference_ShouldNotPublishParent()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var parentAssembly = TestSetup.CreateMockAssembly("ParentAssembly",
            [new AssemblyName("ChildAssembly")]);

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]>
            {
                ["ParentAssembly.wasm"] = assemblyBytes
                // The required child is unavailable, so the parent cannot become ready.
            });

        await using var hostContext = await TestSetup.CreateContext();
        context.ServiceProvider = hostContext.ServiceProvider;

        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>())
            .Returns(parentAssembly);

        var loader = new BlazyAssemblyLoader(
            new AssemblyLoaderOptions { DisableCascadeLoading = false },
            context.ServiceProvider,
            "https://example.com/",
            context.HttpClient!,
            assemblyLoadContext,
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            new AssemblyLoadConfiguration());

        // Act
        var failure = await Assert.ThrowsAsync<BlazyAssemblyLoadException>(() => loader.LoadAssemblyAsync("ParentAssembly.wasm"));

        loader.AdditionalAssemblies.Should().BeEmpty();
        Assert.Contains("ParentAssembly.wasm", failure.Message);
        Assert.IsType<HttpRequestException>(failure.InnerException);
    }

    [Fact]
    public async Task LoadAssemblyAsync_WithCascadeDisabled_ShouldNotLoadReferencedAssemblies()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var parentAssembly = TestSetup.CreateMockAssembly("ParentAssembly",
            [new AssemblyName("ChildAssembly")]);

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]>
            {
                ["ParentAssembly.wasm"] = assemblyBytes,
                ["ChildAssembly.wasm"] = assemblyBytes
            });

        await using var hostContext = await TestSetup.CreateContext();
        context.ServiceProvider = hostContext.ServiceProvider;

        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>())
            .Returns(parentAssembly);

        var loader = new BlazyAssemblyLoader(
            new AssemblyLoaderOptions { DisableCascadeLoading = true },
            context.ServiceProvider,
            "https://example.com/",
            context.HttpClient!,
            assemblyLoadContext,
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            new AssemblyLoadConfiguration());

        // Act
        var loadedAssemblies = await loader.LoadAssemblyAsync("ParentAssembly.wasm");

        // Assert - Only parent should be loaded
        loadedAssemblies.Should().HaveCount(1);
        loader.AdditionalAssemblies.Should().HaveCount(1);
        assemblyLoadContext.Received(1).LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>());
    }

    [Fact]
    public async Task LoadAssemblyAsync_WithMultipleLevelCascade_ShouldAttemptToLoadAllLevels()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var parentAssembly = TestSetup.CreateMockAssembly("ParentAssembly",
            [new AssemblyName("ChildAssembly")]);
        var childAssembly = TestSetup.CreateMockAssembly("ChildAssembly", [new AssemblyName("GrandchildAssembly")]);
        var grandchildAssembly = TestSetup.CreateMockAssembly("GrandchildAssembly");

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]>
            {
                ["ParentAssembly.wasm"] = assemblyBytes,
                ["ChildAssembly.wasm"] = assemblyBytes,
                ["GrandchildAssembly.wasm"] = assemblyBytes
            });

        await using var hostContext = await TestSetup.CreateContext();
        context.ServiceProvider = hostContext.ServiceProvider;

        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>())
            .Returns(parentAssembly, childAssembly, grandchildAssembly);

        var loader = new BlazyAssemblyLoader(
            new AssemblyLoaderOptions { DisableCascadeLoading = false },
            context.ServiceProvider,
            "https://example.com/",
            context.HttpClient!,
            assemblyLoadContext,
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            new AssemblyLoadConfiguration());

        // Act
        await loader.LoadAssemblyAsync("ParentAssembly.wasm");

        loader.AdditionalAssemblies.Should().Contain(parentAssembly);
        loader.AdditionalAssemblies.Should().Contain(childAssembly);
        loader.AdditionalAssemblies.Should().Contain(grandchildAssembly);
        loader.AdditionalAssemblies.Should().HaveCount(3);
        assemblyLoadContext.Received(3).LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>());
    }
}
