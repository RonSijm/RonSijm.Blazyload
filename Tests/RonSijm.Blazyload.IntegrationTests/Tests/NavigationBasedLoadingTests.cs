using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RonSijm.Blazyload.IntegrationTests.Helpers;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.IntegrationTests.Tests;

/// <summary>
/// Navigation-Based Loading Tests
/// Tests LoadOnNavigation configuration and HandleNavigationInternal
/// </summary>
public class NavigationBasedLoadingTests
{
    [Fact]
    public async Task HandleNavigationInternal_WithConfiguredRoute_ShouldLoadAssembly()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var mockAssembly = TestSetup.CreateMockAssembly("NavigationAssembly");

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]> { ["NavigationAssembly.wasm"] = assemblyBytes });

        await using var hostContext = await TestSetup.CreateContext();
        context.ServiceProvider = hostContext.ServiceProvider;

        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>()).Returns(mockAssembly);

        var assemblyLoadConfiguration = new AssemblyLoadConfiguration();
        assemblyLoadConfiguration.Add("/test-page", "NavigationAssembly.wasm");

        var loader = new BlazyAssemblyLoader(
            new AssemblyLoaderOptions { DisableCascadeLoading = true },
            context.ServiceProvider,
            "https://example.com/",
            context.HttpClient!,
            assemblyLoadContext,
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            assemblyLoadConfiguration);

        // Act
        await loader.HandleNavigationInternal("/test-page");

        // Assert
        assemblyLoadContext.Received(1).LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>());
        loader.AdditionalAssemblies.Should().Contain(mockAssembly);
    }

    [Fact]
    public async Task HandleNavigationInternal_WithUnconfiguredRoute_ShouldNotLoadAssembly()
    {
        // Arrange
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());

        var assemblyLoadConfiguration = new AssemblyLoadConfiguration();
        assemblyLoadConfiguration.Add("/configured-page", "SomeAssembly.wasm");

        var loader = new BlazyAssemblyLoader(
            new AssemblyLoaderOptions(),
            serviceProvider,
            "https://example.com/",
            new HttpClient(),
            assemblyLoadContext,
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            assemblyLoadConfiguration);

        // Act
        await loader.HandleNavigationInternal("/unconfigured-page");

        // Assert
        assemblyLoadContext.DidNotReceive().LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>());
    }

    [Fact]
    public async Task HandleNavigationInternal_WithEmptyConfiguration_ShouldNotThrow()
    {
        // Arrange
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());

        var loader = new BlazyAssemblyLoader(
            new AssemblyLoaderOptions(),
            serviceProvider,
            "https://example.com/",
            new HttpClient(),
            assemblyLoadContext,
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            new AssemblyLoadConfiguration());

        // Act & Assert - Should not throw with empty configuration
        await loader.HandleNavigationInternal("/any-page");
        assemblyLoadContext.DidNotReceive().LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>());
    }

    [Fact]
    public void AssemblyLoadConfiguration_Add_ShouldStoreMapping()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act
        config.Add("/page1", "Assembly1.wasm");
        config.Add("/page2", "Assembly2.wasm");

        // Assert
        config.GetAssembly("/page1").Should().Be("Assembly1.wasm");
        config.GetAssembly("/page2").Should().Be("Assembly2.wasm");
    }

    [Fact]
    public void AssemblyLoadConfiguration_GetAssembly_WithUnknownPath_ShouldReturnNull()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();
        config.Add("/known-page", "KnownAssembly.wasm");

        // Act
        var result = config.GetAssembly("/unknown-page");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void AssemblyLoadConfiguration_Remove_ShouldRemoveAllEntriesForAssembly()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();
        config.Add("/page1", "SharedAssembly.wasm");
        config.Add("/page2", "SharedAssembly.wasm");
        config.Add("/page3", "OtherAssembly.wasm");

        // Act
        config.Remove("SharedAssembly.wasm");

        // Assert
        config.GetAssembly("/page1").Should().BeNull();
        config.GetAssembly("/page2").Should().BeNull();
        config.GetAssembly("/page3").Should().Be("OtherAssembly.wasm");
    }
}

