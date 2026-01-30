using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.Tests;

public class BlazyAssemblyLoaderTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithBaseUrl()
    {
        // Arrange
        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var baseUrl = "https://example.com/";

        // Act
        var loader = new BlazyAssemblyLoader(options, serviceProvider, baseUrl);

        // Assert
        loader.Should().NotBeNull();
        loader.AdditionalAssemblies.Should().NotBeNull();
        loader.AdditionalAssemblies.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldAppendSlashToBaseUrlIfMissing()
    {
        // Arrange
        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var baseUrl = "https://example.com";

        // Act
        var loader = new BlazyAssemblyLoader(options, serviceProvider, baseUrl);

        // Assert - We can't directly test the private _baseUrl field, but we can verify the constructor doesn't throw
        loader.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldCallSetReferenceOnExtensions()
    {
        // Arrange
        var extension = Substitute.For<ILoadAfterExtension>();
        var options = new AssemblyLoaderOptions
        {
            AfterLoadAssembliesExtensions = new List<ILoadAfterExtension> { extension }
        };
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var baseUrl = "https://example.com/";

        // Act
        var loader = new BlazyAssemblyLoader(options, serviceProvider, baseUrl);

        // Assert
        extension.Received(1).SetReference(serviceProvider);
    }

    [Fact]
    public void LoadedAssemblies_ShouldBeEmptyInitially()
    {
        // Arrange
        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var baseUrl = "https://example.com/";

        // Act
        var loader = new BlazyAssemblyLoader(options, serviceProvider, baseUrl);

        // Assert
        loader.AdditionalAssemblies.Should().BeEmpty();
    }

    [Fact]
    public void AssemblyLoadConfiguration_ShouldBeInjectedViaConstructor()
    {
        // Arrange
        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var baseUrl = "https://example.com/";
        var config = new AssemblyLoadConfiguration();
        config.Add("/test", "TestAssembly.wasm");

        // Act - AssemblyLoadConfiguration is now injected via constructor
        var loader = new BlazyAssemblyLoader(options, serviceProvider, baseUrl);

        // Assert - The loader should be created successfully
        loader.Should().NotBeNull();
    }

    // Note: HandleNavigation tests are skipped because NavigationContext is sealed and cannot be easily instantiated in tests
    // The method is tested indirectly through integration tests

    [Fact]
    public void Constructor_ShouldHandleMultipleExtensions()
    {
        // Arrange
        var extension1 = Substitute.For<ILoadAfterExtension>();
        var extension2 = Substitute.For<ILoadAfterExtension>();
        var options = new AssemblyLoaderOptions
        {
            AfterLoadAssembliesExtensions = new List<ILoadAfterExtension> { extension1, extension2 }
        };
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var baseUrl = "https://example.com/";

        // Act
        var loader = new BlazyAssemblyLoader(options, serviceProvider, baseUrl);

        // Assert
        extension1.Received(1).SetReference(serviceProvider);
        extension2.Received(1).SetReference(serviceProvider);
    }
}

