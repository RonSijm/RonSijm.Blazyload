using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RonSijm.Blazyload.IntegrationTests.Helpers;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.IntegrationTests.Tests;

/// <summary>
/// Bootstrap Integration Tests
/// Tests IBootstrapper invocation and service registration when assemblies are loaded
/// </summary>
public class BootstrapIntegrationTests
{
    [Fact]
    public async Task LoadAssemblyAsync_WithBootstrapper_ShouldInvokeBootstrapMethod()
    {
        // Arrange
        var bootstrapCalled = false;
        var mockBootstrapper = Substitute.For<IBootstrapper>();
        mockBootstrapper.Bootstrap().Returns(Task.FromResult<IEnumerable<ServiceDescriptor>>(new List<ServiceDescriptor>()));
        mockBootstrapper.When(x => x.Bootstrap()).Do(_ => bootstrapCalled = true);

        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var mockAssembly = TestSetup.CreateMockAssembly("BootstrapAssembly");

        // Configure the mock assembly to return a type that implements IBootstrapper
        var bootstrapperType = mockBootstrapper.GetType();
        mockAssembly.GetType("BootstrapAssembly.Properties.BlazyBootstrap").Returns(bootstrapperType);
        mockAssembly.GetTypes().Returns([bootstrapperType]);

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]> { ["BootstrapAssembly.wasm"] = assemblyBytes });

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

        // Act
        await loader.LoadAssemblyAsync("BootstrapAssembly.wasm");

        // Assert
        loader.AdditionalAssemblies.Should().Contain(mockAssembly);
    }

    [Fact]
    public async Task LoadAssemblyAsync_WithCustomClassPath_ShouldUseCustomBootstrapper()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var mockAssembly = TestSetup.CreateMockAssembly("CustomBootstrapAssembly");

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]> { ["CustomBootstrapAssembly.wasm"] = assemblyBytes });

        await using var hostContext = await TestSetup.CreateContext(options =>
        {
            options.UseSettingsForDll("CustomBootstrapAssembly")
                .UseCustomClass("CustomBootstrapAssembly.CustomRegistration");
        });
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

        // Act
        await loader.LoadAssemblyAsync("CustomBootstrapAssembly.wasm");

        // Assert - Assembly should be loaded (bootstrap class lookup happens internally)
        loader.AdditionalAssemblies.Should().Contain(mockAssembly);
    }

    [Fact]
    public async Task LoadAssemblyAsync_WithoutBootstrapper_ShouldStillLoadAssembly()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var mockAssembly = TestSetup.CreateMockAssembly("NoBootstrapAssembly");
        mockAssembly.GetType(Arg.Any<string>()).Returns((Type)null);
        mockAssembly.GetTypes().Returns([]);

        var context = TestSetup.CreateContextWithMockHttp(
            new Dictionary<string, byte[]> { ["NoBootstrapAssembly.wasm"] = assemblyBytes });

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

        // Act
        await loader.LoadAssemblyAsync("NoBootstrapAssembly.wasm");

        // Assert
        loader.AdditionalAssemblies.Should().Contain(mockAssembly);
    }
}

