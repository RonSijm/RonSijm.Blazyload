using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.Tests;

public class BlazyServiceProviderFactoryTests
{
    [Fact]
    public void Constructor_ShouldAcceptOptions()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();

        // Act
        var factory = new BlazyServiceProviderFactory(options);

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void CreateBuilder_ShouldReturnSyringeServiceProviderBuilder()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();
        var factory = new BlazyServiceProviderFactory(options);
        var services = new ServiceCollection();

        // Act
        var builder = factory.CreateBuilder(services);

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeOfType<SyringeServiceProviderBuilder>();
    }

    [Fact]
    public void CreateBuilder_ShouldRegisterIAssemblyLoader()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();
        var factory = new BlazyServiceProviderFactory(options);
        var services = new ServiceCollection();

        // Act
        var builder = factory.CreateBuilder(services);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(IAssemblyLoader));
    }

    [Fact]
    public void CreateBuilder_ShouldRegisterBlazyAssemblyLoader()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();
        var factory = new BlazyServiceProviderFactory(options);
        var services = new ServiceCollection();

        // Act
        var builder = factory.CreateBuilder(services);

        // Assert - IBlazyAssemblyLoader should be registered with BlazyAssemblyLoader as implementation
        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IBlazyAssemblyLoader));
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(BlazyAssemblyLoader));
    }

    [Fact]
    public void CreateBuilder_ShouldRegisterAssemblyLoadConfiguration()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();
        var factory = new BlazyServiceProviderFactory(options);
        var services = new ServiceCollection();

        // Act
        var builder = factory.CreateBuilder(services);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(AssemblyLoadConfiguration));
    }

    [Fact]
    public void CreateBuilder_ShouldRegisterAssemblyLoaderOptions()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();
        var factory = new BlazyServiceProviderFactory(options);
        var services = new ServiceCollection();

        // Act
        var builder = factory.CreateBuilder(services);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(AssemblyLoaderOptions));
    }

    [Fact]
    public void CreateBuilder_ShouldRegisterServicesAsSingleton()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();
        var factory = new BlazyServiceProviderFactory(options);
        var services = new ServiceCollection();

        // Act
        var builder = factory.CreateBuilder(services);

        // Assert
        var assemblyLoaderDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IAssemblyLoader));
        assemblyLoaderDescriptor.Should().NotBeNull();
        assemblyLoaderDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);

        var configDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(AssemblyLoadConfiguration));
        configDescriptor.Should().NotBeNull();
        configDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);

        var optionsDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(AssemblyLoaderOptions));
        optionsDescriptor.Should().NotBeNull();
        optionsDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void CreateBuilder_ShouldPreserveExistingServices()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();
        var factory = new BlazyServiceProviderFactory(options);
        var services = new ServiceCollection();
        services.AddSingleton<string>("test");

        // Act
        var builder = factory.CreateBuilder(services);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(string));
    }

    [Fact]
    public void CreateBuilder_ShouldRegisterSameAssemblyLoadConfigurationInstance()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();
        var factory = new BlazyServiceProviderFactory(options);
        var services = new ServiceCollection();

        // Act
        var builder = factory.CreateBuilder(services);

        // Assert
        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(AssemblyLoadConfiguration));
        descriptor.Should().NotBeNull();

        // Get the internal AssemblyLoadConfiguration from options
        var configProperty = typeof(BlazyloadProviderOptions).GetProperty("AssemblyLoadConfiguration",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var expectedConfig = configProperty?.GetValue(options);

        descriptor.ImplementationInstance.Should().Be(expectedConfig);
    }

    [Fact]
    public void CreateBuilder_ShouldRegisterSameAssemblyLoaderOptionsInstance()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();
        var factory = new BlazyServiceProviderFactory(options);
        var services = new ServiceCollection();

        // Act
        var builder = factory.CreateBuilder(services);

        // Assert
        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(AssemblyLoaderOptions));
        descriptor.Should().NotBeNull();
        descriptor.ImplementationInstance.Should().Be(options.AssemblyLoaderOptions);
    }

    [Fact]
    public void CreateServiceProvider_ShouldReturnServiceProvider()
    {
        var options = new BlazyloadProviderOptions();
        var factory = new BlazyServiceProviderFactory(options);
        var services = new ServiceCollection();
        var builder = factory.CreateBuilder(services);

        var serviceProvider = factory.CreateServiceProvider(builder);

        serviceProvider.Should().NotBeNull();
        serviceProvider.Should().BeOfType<SyringeServiceProvider>();
    }

    [Fact]
    public void CreateServiceProvider_ShouldReturnSyringeServiceProvider()
    {
        var options = new BlazyloadProviderOptions();
        var factory = new BlazyServiceProviderFactory(options);
        var services = new ServiceCollection();
        var builder = factory.CreateBuilder(services);

        var serviceProvider = factory.CreateServiceProvider(builder);

        serviceProvider.Should().BeOfType<SyringeServiceProvider>();
        var syringeProvider = (SyringeServiceProvider)serviceProvider;
        syringeProvider.Options.Should().Be(options);
    }

}

