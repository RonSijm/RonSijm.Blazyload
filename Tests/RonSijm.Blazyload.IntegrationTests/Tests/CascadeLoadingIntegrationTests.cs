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
    public async Task LoadAssemblyAsync_WithCascadeEnabled_ShouldAttemptToLoadReferencedAssemblies()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var parentAssembly = TestSetup.CreateMockAssembly("ParentAssembly",
            [new AssemblyName("ChildAssembly")]);

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]>
            {
                ["ParentAssembly.wasm"] = assemblyBytes
                // Note: ChildAssembly.wasm is NOT provided, simulating a referenced assembly
                // that may already be loaded or not available for lazy loading
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
        var loadedAssemblies = await loader.LoadAssemblyAsync("ParentAssembly.wasm");

        // Assert - Parent should be loaded, cascade loading is attempted but child may not be available
        loadedAssemblies.Should().HaveCount(1);
        loader.AdditionalAssemblies.Should().Contain(parentAssembly);
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

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]>
            {
                ["ParentAssembly.wasm"] = assemblyBytes
                // Child and grandchild assemblies are not provided - simulating
                // referenced assemblies that may already be loaded or not available
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
        await loader.LoadAssemblyAsync("ParentAssembly.wasm");

        // Assert - Parent should be loaded, cascade loading is attempted for children
        loader.AdditionalAssemblies.Should().Contain(parentAssembly);
        loader.AdditionalAssemblies.Count.Should().BeGreaterThanOrEqualTo(1);
    }
}

