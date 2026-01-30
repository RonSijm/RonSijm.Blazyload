using AwesomeAssertions;
using NSubstitute;
using RonSijm.Blazyload.IntegrationTests.Helpers;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.IntegrationTests.Tests;

/// <summary>
/// Multiple Assembly Loading Tests
/// Tests loading multiple assemblies simultaneously and verifying no DI conflicts
/// </summary>
public class MultipleAssemblyLoadingTests
{
    [Fact]
    public async Task LoadAssembliesAsync_WithMultipleAssemblies_ShouldLoadAll()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var mockAssembly1 = TestSetup.CreateMockAssembly("Assembly1");
        var mockAssembly2 = TestSetup.CreateMockAssembly("Assembly2");
        var mockAssembly3 = TestSetup.CreateMockAssembly("Assembly3");

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]>
            {
                ["Assembly1.wasm"] = assemblyBytes,
                ["Assembly2.wasm"] = assemblyBytes,
                ["Assembly3.wasm"] = assemblyBytes
            });

        await using var hostContext = await TestSetup.CreateContext();
        context.ServiceProvider = hostContext.ServiceProvider;

        var loadCount = 0;
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>())
            .Returns(_ =>
            {
                loadCount++;
                return loadCount switch
                {
                    1 => mockAssembly1,
                    2 => mockAssembly2,
                    _ => mockAssembly3
                };
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
        var loadedAssemblies = await loader.LoadAssembliesAsync(new[]
        {
            "Assembly1.wasm",
            "Assembly2.wasm",
            "Assembly3.wasm"
        });

        // Assert
        loadedAssemblies.Should().HaveCount(3);
        loader.AdditionalAssemblies.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoadAssembliesAsync_WithDuplicates_InSeparateCalls_ShouldLoadOnlyOnce()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var mockAssembly = TestSetup.CreateMockAssembly("DuplicateAssembly");

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]> { ["DuplicateAssembly.wasm"] = assemblyBytes });

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

        // Act - Load same assembly in separate calls
        var firstLoad = await loader.LoadAssemblyAsync("DuplicateAssembly.wasm");
        var secondLoad = await loader.LoadAssemblyAsync("DuplicateAssembly.wasm");
        var thirdLoad = await loader.LoadAssemblyAsync("DuplicateAssembly.wasm");

        // Assert - Should only load once (first call), subsequent calls return empty
        firstLoad.Should().HaveCount(1);
        secondLoad.Should().BeEmpty();
        thirdLoad.Should().BeEmpty();
        assemblyLoadContext.Received(1).LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>());
    }

    [Fact]
    public async Task LoadAssembliesAsync_WithMixedExistingAndNew_ShouldOnlyLoadNew()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var existingAssembly = TestSetup.CreateMockAssembly("ExistingAssembly");
        var newAssembly = TestSetup.CreateMockAssembly("NewAssembly");

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]>
            {
                ["ExistingAssembly.wasm"] = assemblyBytes,
                ["NewAssembly.wasm"] = assemblyBytes
            });

        await using var hostContext = await TestSetup.CreateContext();
        context.ServiceProvider = hostContext.ServiceProvider;

        var loadCount = 0;
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>())
            .Returns(_ =>
            {
                loadCount++;
                return loadCount == 1 ? existingAssembly : newAssembly;
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

        // First load the existing assembly
        await loader.LoadAssemblyAsync("ExistingAssembly.wasm");

        // Act - Load both existing and new
        var loadedAssemblies = await loader.LoadAssembliesAsync(new[]
        {
            "ExistingAssembly.wasm",
            "NewAssembly.wasm"
        });

        // Assert - Only new assembly should be in the result
        loadedAssemblies.Should().HaveCount(1);
        loader.AdditionalAssemblies.Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadAssembliesAsync_WithEmptyList_ShouldReturnEmpty()
    {
        // Arrange
        await using var hostContext = await TestSetup.CreateContext();

        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();

        var loader = new BlazyAssemblyLoader(
            new AssemblyLoaderOptions(),
            hostContext.ServiceProvider,
            "https://example.com/",
            new HttpClient(),
            assemblyLoadContext,
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            new AssemblyLoadConfiguration());

        // Act
        var loadedAssemblies = await loader.LoadAssembliesAsync(Array.Empty<string>());

        // Assert
        loadedAssemblies.Should().BeEmpty();
        assemblyLoadContext.DidNotReceive().LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>());
    }
}

