using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RonSijm.Blazyload.IntegrationTests.Helpers;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.IntegrationTests.Tests;

/// <summary>
/// Custom Path Loading Tests
/// Tests loading assemblies from custom paths (AbsolutePath, RelativePath)
/// </summary>
public class CustomPathLoadingTests
{
    [Fact]
    public void GetDllLocationFromOptions_WithNullOptions_ShouldReturnDefaultPath()
    {
        // Arrange
        var loader = CreateLoader();

        // Act
        var result = loader.GetDllLocationFromOptions(null);

        // Assert
        result.Should().Be("https://example.com/_framework/");
    }

    [Fact]
    public void GetDllLocationFromOptions_WithAbsolutePath_ShouldReturnAbsolutePath()
    {
        // Arrange
        var loader = CreateLoader();
        var options = new AssemblyOptions { AbsolutePath = "https://cdn.example.com/assemblies/" };

        // Act
        var result = loader.GetDllLocationFromOptions(options);

        // Assert
        result.Should().Be("https://cdn.example.com/assemblies/");
    }

    [Fact]
    public void GetDllLocationFromOptions_WithRelativePath_ShouldReturnBaseUrlPlusRelativePath()
    {
        // Arrange
        var loader = CreateLoader();
        var options = new AssemblyOptions { RelativePath = "custom/path/" };

        // Act
        var result = loader.GetDllLocationFromOptions(options);

        // Assert
        result.Should().Be("https://example.com/custom/path/");
    }

    [Fact]
    public void GetDllLocationFromOptions_WithBothPaths_ShouldPreferAbsolutePath()
    {
        // Arrange
        var loader = CreateLoader();
        var options = new AssemblyOptions
        {
            AbsolutePath = "https://cdn.example.com/",
            RelativePath = "relative/"
        };

        // Act
        var result = loader.GetDllLocationFromOptions(options);

        // Assert
        result.Should().Be("https://cdn.example.com/");
    }

    [Fact]
    public void GetDllLocationFromOptions_WithEmptyOptions_ShouldReturnDefaultPath()
    {
        // Arrange
        var loader = CreateLoader();
        var options = new AssemblyOptions();

        // Act
        var result = loader.GetDllLocationFromOptions(options);

        // Assert
        result.Should().Be("https://example.com/_framework/");
    }

    [Fact]
    public async Task LoadAssemblyAsync_WithCustomRelativePath_ShouldRequestFromCorrectUrl()
    {
        // Arrange
        var assemblyBytes = TestSetup.CreateDummyAssemblyBytes();
        var mockAssembly = TestSetup.CreateMockAssembly("CustomPathAssembly");
        var requestedUrls = new List<string>();

        var httpClient = MockHttpMessageHandler.CreateMockHttpClient((request, ct) =>
        {
            requestedUrls.Add(request.RequestUri?.ToString() ?? string.Empty);

            if (request.RequestUri?.ToString().Contains("blazor.boot.json") == true)
            {
                return Task.FromResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"resources\":{\"assembly\":{}}}")
                });
            }

            return Task.FromResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(assemblyBytes)
            });
        });

        await using var hostContext = await TestSetup.CreateContext(options =>
        {
            options.UseSettingsForDll("CustomPathAssembly").UseCustomRelativePath("custom/libs/");
        });

        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>()).Returns(mockAssembly);

        var loader = new BlazyAssemblyLoader(
            new AssemblyLoaderOptions(),
            hostContext.ServiceProvider,
            "https://example.com/",
            httpClient,
            assemblyLoadContext,
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            new AssemblyLoadConfiguration());

        // Act
        await loader.LoadAssemblyAsync("CustomPathAssembly.wasm");

        // Assert
        requestedUrls.Should().Contain(url => url.Contains("custom/libs/CustomPathAssembly.wasm"));
    }

    private static BlazyAssemblyLoader CreateLoader()
    {
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        return new BlazyAssemblyLoader(
            new AssemblyLoaderOptions(),
            serviceProvider,
            "https://example.com/",
            new HttpClient(),
            Substitute.For<IAssemblyLoadContext>(),
            Substitute.For<IBlazyLogger>(),
            Substitute.For<IDebuggerDetector>(),
            new AssemblyLoadConfiguration());
    }
}

