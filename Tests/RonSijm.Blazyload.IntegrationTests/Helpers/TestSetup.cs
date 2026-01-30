using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using RonSijm.Blazyload.Loading;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.IntegrationTests.Helpers;

/// <summary>
/// Helper class for setting up integration test contexts.
/// </summary>
public static class TestSetup
{
    /// <summary>
    /// Creates a basic integration test context with default options.
    /// </summary>
    public static async Task<IntegrationTestContext> CreateContext(Action<BlazyloadProviderOptions>? optionsConfig = null)
    {
        var context = new IntegrationTestContext
        {
            HostBuilder = Host.CreateDefaultBuilder()
        };

        context.HostBuilder.UseBlazyload(options =>
        {
            optionsConfig?.Invoke(options);
        });

        context.Host = context.HostBuilder.Build();
        context.ServiceProvider = (context.Host.Services as SyringeServiceProvider)!;

        return context;
    }

    /// <summary>
    /// Creates an integration test context with a mock HTTP client for assembly loading.
    /// </summary>
    public static IntegrationTestContext CreateContextWithMockHttp(
        Dictionary<string, byte[]> assemblyResponses,
        BlazorBootModel? bootModel = null,
        Action<BlazyloadProviderOptions>? optionsConfig = null)
    {
        var context = new IntegrationTestContext();

        bootModel ??= new BlazorBootModel
        {
            Resources = new Resources
            {
                Assembly = new Dictionary<string, string>()
            }
        };

        context.HttpClient = MockHttpMessageHandler.CreateMockHttpClient((request, ct) =>
        {
            var uri = request.RequestUri?.ToString() ?? string.Empty;

            if (uri.Contains("blazor.boot.json"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(bootModel), Encoding.UTF8, "application/json")
                });
            }

            foreach (var (assemblyName, bytes) in assemblyResponses)
            {
                if (uri.Contains(assemblyName))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(bytes)
                    });
                }
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        return context;
    }

    /// <summary>
    /// Creates a mock assembly loader for testing.
    /// </summary>
    public static BlazyAssemblyLoader CreateMockAssemblyLoader(
        IntegrationTestContext context,
        Dictionary<string, Assembly>? assemblyMappings = null,
        string baseUrl = "https://example.com/",
        AssemblyLoadConfiguration? assemblyLoadConfiguration = null)
    {
        var options = new AssemblyLoaderOptions();
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(false);

        if (assemblyMappings != null)
        {
            assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream?>())
                .Returns(callInfo =>
                {
                    // Return the first available mock assembly
                    return assemblyMappings.Values.FirstOrDefault();
                });
        }

        var loader = new BlazyAssemblyLoader(
            options,
            context.ServiceProvider,
            baseUrl,
            context.HttpClient ?? new HttpClient(),
            assemblyLoadContext,
            logger,
            debuggerDetector,
            assemblyLoadConfiguration ?? new AssemblyLoadConfiguration());

        context.AssemblyLoader = loader;
        return loader;
    }

    /// <summary>
    /// Creates a mock assembly with the specified name.
    /// </summary>
    public static Assembly CreateMockAssembly(string name, AssemblyName[]? referencedAssemblies = null)
    {
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetName().Returns(new AssemblyName(name));
        mockAssembly.FullName.Returns($"{name}, Version=1.0.0.0");
        mockAssembly.GetReferencedAssemblies().Returns(referencedAssemblies ?? Array.Empty<AssemblyName>());
        return mockAssembly;
    }

    /// <summary>
    /// Creates dummy assembly bytes (minimal PE header).
    /// </summary>
    public static byte[] CreateDummyAssemblyBytes()
    {
        return [0x4D, 0x5A, 0x90, 0x00];
    }
}

