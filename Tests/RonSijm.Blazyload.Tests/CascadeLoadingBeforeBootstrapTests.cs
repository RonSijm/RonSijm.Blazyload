using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RonSijm.Blazyload.Loading;
using RonSijm.Syringe;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace RonSijm.Blazyload.Tests;

/// <summary>
/// Tests to verify that cascade loading happens BEFORE bootstrapping.
/// This is critical because a bootstrapper may depend on types from referenced assemblies.
/// </summary>
public class CascadeLoadingBeforeBootstrapTests
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

        public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            _sendAsync = sendAsync;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _sendAsync(request, cancellationToken);
        }
    }

    private static HttpClient CreateMockHttpClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        var mockHandler = new MockHttpMessageHandler(handler);
        return new HttpClient(mockHandler);
    }

    /// <summary>
    /// This test verifies that when loading an assembly with a referenced assembly,
    /// the referenced assembly is loaded BEFORE the bootstrapper is called.
    ///
    /// The scenario:
    /// - MainAssembly references SharedAssembly
    /// - MainAssembly has a BlazyBootstrap class that uses types from SharedAssembly
    /// - When loading MainAssembly, SharedAssembly should be loaded first (cascade loading)
    /// - Then the bootstrapper should be called
    ///
    /// Without the fix, the bootstrapper would be called before SharedAssembly is loaded,
    /// causing a FileNotFoundException when the bootstrapper tries to use types from SharedAssembly.
    /// </summary>
    [Fact]
    public async Task LoadAssembliesAsync_ShouldCascadeLoadReferencedAssemblies_BeforeBootstrapping()
    {
        // Arrange
        var assemblyBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };
        var loadOrder = new List<string>();

        var options = new AssemblyLoaderOptions { DisableCascadeLoading = false };
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();

        // Create mock for the main assembly (e.g., RonSijm.Vertimart.Web.Dental)
        var mainAssembly = Substitute.For<Assembly>();
        mainAssembly.GetName().Returns(new AssemblyName("MainAssembly"));
        mainAssembly.GetReferencedAssemblies().Returns(new[] { new AssemblyName("SharedAssembly") });

        // Create mock for the shared/referenced assembly (e.g., RonSijm.Vertimart.Shared)
        var sharedAssembly = Substitute.For<Assembly>();
        sharedAssembly.GetName().Returns(new AssemblyName("SharedAssembly"));
        sharedAssembly.GetReferencedAssemblies().Returns(Array.Empty<AssemblyName>());

        // Track the order in which assemblies are loaded
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), null)
            .Returns(callInfo =>
            {
                // First call returns mainAssembly, second call returns sharedAssembly
                if (loadOrder.Count == 0)
                {
                    loadOrder.Add("MainAssembly");
                    return mainAssembly;
                }
                else
                {
                    loadOrder.Add("SharedAssembly");
                    return sharedAssembly;
                }
            });

        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(false);

        var bootJson = new BlazorBootModel { Resources = new Resources { Assembly = new Dictionary<string, string>() } };
        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            if (request.RequestUri!.ToString().Contains("blazor.boot.json"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(bootJson), Encoding.UTF8, "application/json")
                });
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(assemblyBytes)
            });
        });

        var loader = new BlazyAssemblyLoader(options, serviceProvider, "https://example.com/", httpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());

        // Act
        var result = await loader.LoadAssembliesAsync(new[] { "MainAssembly.wasm" }, false);

        // Assert
        // Both assemblies should be loaded
        assemblyLoadContext.Received(2).LoadFromStream(Arg.Any<Stream>(), null);

        // The main assembly should be loaded first, then the shared assembly via cascade loading
        loadOrder.Should().HaveCount(2);
        loadOrder[0].Should().Be("MainAssembly");
        loadOrder[1].Should().Be("SharedAssembly");
    }
}