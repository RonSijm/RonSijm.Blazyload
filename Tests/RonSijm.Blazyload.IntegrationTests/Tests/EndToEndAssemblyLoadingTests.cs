using AwesomeAssertions;
using NSubstitute;
using RonSijm.Blazyload.IntegrationTests.Helpers;
using RonSijm.Blazyload.Loading;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.IntegrationTests.Tests;

/// <summary>
/// End-to-End Assembly Loading Tests
/// Tests the complete flow: trigger lazy loading → verify assembly loads → verify DI services registered
/// </summary>
public class EndToEndAssemblyLoadingTests
{
    [Fact]
    public async Task LoadAssemblyAsync_ShouldLoadAssembly_AndRegisterWithServiceProvider()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var mockAssembly = TestSetup.CreateMockAssembly("TestAssembly");

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]> { ["TestAssembly.wasm"] = assemblyBytes });

        await using var hostContext = await TestSetup.CreateContext();
        context.ServiceProvider = hostContext.ServiceProvider;

        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>()).Returns(mockAssembly);

        var loader = new BlazyAssemblyLoader(
            new AssemblyLoaderOptions(),
            context.ServiceProvider,
            "https://example.com/",
            context.HttpClient!,
            assemblyLoadContext,
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            new AssemblyLoadConfiguration());

        // Act
        var loadedAssemblies = await loader.LoadAssemblyAsync("TestAssembly.wasm");

        // Assert
        loadedAssemblies.Should().NotBeNull();
        loadedAssemblies.Should().HaveCount(1);
        loader.AdditionalAssemblies.Should().Contain(mockAssembly);
    }

    [Fact]
    public async Task LoadAssembliesAsync_ShouldLoadMultipleAssemblies_InSingleCall()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var mockAssembly1 = TestSetup.CreateMockAssembly("Assembly1");
        var mockAssembly2 = TestSetup.CreateMockAssembly("Assembly2");

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]>
            {
                ["Assembly1.wasm"] = assemblyBytes,
                ["Assembly2.wasm"] = assemblyBytes
            });

        await using var hostContext = await TestSetup.CreateContext();
        context.ServiceProvider = hostContext.ServiceProvider;

        var callCount = 0;
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>())
            .Returns(_ =>
            {
                callCount++;
                return callCount == 1 ? mockAssembly1 : mockAssembly2;
            });

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
        var loadedAssemblies = await loader.LoadAssembliesAsync(new[] { "Assembly1.wasm", "Assembly2.wasm" });

        // Assert
        loadedAssemblies.Should().HaveCount(2);
        loader.AdditionalAssemblies.Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadAssemblyAsync_ShouldSkipAlreadyLoadedAssemblies()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var mockAssembly = TestSetup.CreateMockAssembly("TestAssembly");

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]> { ["TestAssembly.wasm"] = assemblyBytes });

        await using var hostContext = await TestSetup.CreateContext();
        context.ServiceProvider = hostContext.ServiceProvider;

        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>()).Returns(mockAssembly);

        var loader = new BlazyAssemblyLoader(
            new AssemblyLoaderOptions { DisableCascadeLoading = true },
            context.ServiceProvider,
            "https://example.com/",
            context.HttpClient!,
            assemblyLoadContext,
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            new AssemblyLoadConfiguration());

        // Act - Load the same assembly twice
        await loader.LoadAssemblyAsync("TestAssembly.wasm");
        var secondLoad = await loader.LoadAssemblyAsync("TestAssembly.wasm");

        // Assert - Second load should return empty (already loaded)
        secondLoad.Should().BeEmpty();
        assemblyLoadContext.Received(1).LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>());
    }

    [Fact]
    public async Task LoadAssemblyAsync_ShouldSkipPreloadedAssemblies()
    {
        // Arrange
        var bootModel = new BlazorBootModel
        {
            Resources = new Resources
            {
                Assembly = new Dictionary<string, string>
                {
                    ["PreloadedAssembly.wasm"] = "hash123"
                }
            }
        };

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]>(),
            bootModel);

        await using var hostContext = await TestSetup.CreateContext();
        context.ServiceProvider = hostContext.ServiceProvider;

        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();

        var loader = new BlazyAssemblyLoader(
            new AssemblyLoaderOptions(),
            context.ServiceProvider,
            "https://example.com/",
            context.HttpClient!,
            assemblyLoadContext,
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            new AssemblyLoadConfiguration());

        // Act
        var loadedAssemblies = await loader.LoadAssemblyAsync("PreloadedAssembly.wasm");

        // Assert - Should not load preloaded assemblies
        loadedAssemblies.Should().BeEmpty();
        assemblyLoadContext.DidNotReceive().LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>());
    }
}

