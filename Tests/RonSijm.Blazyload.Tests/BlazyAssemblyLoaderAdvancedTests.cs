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

public class BlazyAssemblyLoaderAdvancedTests
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

    private static Uri GetRequestUri(HttpRequestMessage request)
    {
        var uri = request.RequestUri ?? throw new InvalidOperationException("The HTTP test handler requires a request URI.");
        return uri;
    }

    [Fact]
    public async Task LoadAssembliesAsync_ShouldReturnInitializedAssemblies_WhenHttpSucceeds()
    {
        var bootJson = new BlazorBootModel
        {
            Resources = new Resources
            {
                Assembly = new Dictionary<string, string>
                {
                    ["TestAssembly.wasm"] = "hash1",
                    ["AnotherAssembly.wasm"] = "hash2"
                }
            }
        };

        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(bootJson), Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetName().Returns(new AssemblyName("TestAssembly"));
        mockAssembly.GetReferencedAssemblies().Returns(Array.Empty<AssemblyName>());
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), null).Returns(mockAssembly);
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();

        var loader = new BlazyAssemblyLoader(options, serviceProvider, "https://example.com/", httpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());

        var assemblies = await loader.LoadAssembliesAsync(new[] { "TestAssembly.wasm" });

        assemblies.Should().ContainSingle().Which.Should().BeSameAs(mockAssembly);
    }

    [Fact]
    public async Task LoadAssembliesAsync_ShouldPropagateAndLogDownloadFailure()
    {
        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        var options = new AssemblyLoaderOptions { EnableLogging = true };
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();

        var loader = new BlazyAssemblyLoader(options, serviceProvider, "https://example.com/", httpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());

        var failure = await Assert.ThrowsAsync<BlazyAssemblyLoadException>(() => loader.LoadAssembliesAsync(new[] { "TestAssembly.wasm" }));

        Assert.IsType<HttpRequestException>(failure.InnerException);
        logger.Received().WriteLine(failure);
    }

    [Fact]
    public async Task LoadFileAssembly_ShouldLoadAssembly_WhenHttpSucceeds()
    {
        var assemblyBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };

        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(assemblyBytes)
            };
            return Task.FromResult(response);
        });

        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetName().Returns(new AssemblyName("Test"));
        mockAssembly.GetReferencedAssemblies().Returns(Array.Empty<AssemblyName>());
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream>()).Returns(mockAssembly);
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(false);

        var loader = new BlazyAssemblyLoader(options, serviceProvider, "https://example.com/", httpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());

        var bootJson = new BlazorBootModel { Resources = new Resources { Assembly = new Dictionary<string, string>() } };
        var bootHttpClient = CreateMockHttpClient((request, ct) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(bootJson), Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        var loaderWithBoot = new BlazyAssemblyLoader(options, serviceProvider, "https://example.com/", bootHttpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());
        await loaderWithBoot.LoadAssembliesAsync(new[] { "Test.wasm" });

        assemblyLoadContext.Received().LoadFromStream(Arg.Any<Stream>(), null);
    }

    [Fact]
    public async Task LoadAssembliesWithByOptions_ShouldLoadSymbols_WhenDebuggerAttached()
    {
        var assemblyBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };
        var pdbBytes = new byte[] { 0x42, 0x53, 0x4A, 0x42 };

        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            var content = GetRequestUri(request).ToString().Contains(".pdb") ? pdbBytes : assemblyBytes;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            };
            return Task.FromResult(response);
        });

        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetName().Returns(new AssemblyName("Test"));
        mockAssembly.GetReferencedAssemblies().Returns(Array.Empty<AssemblyName>());
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream>()).Returns(mockAssembly);
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(true);

        var bootJson = new BlazorBootModel { Resources = new Resources { Assembly = new Dictionary<string, string>() } };
        var bootHttpClient = CreateMockHttpClient((request, ct) =>
        {
            if (GetRequestUri(request).ToString().Contains("blazor.boot.json"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(bootJson), Encoding.UTF8, "application/json")
                });
            }
            var content = GetRequestUri(request).ToString().Contains(".pdb") ? pdbBytes : assemblyBytes;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            });
        });

        var loader = new BlazyAssemblyLoader(options, serviceProvider, "https://example.com/", bootHttpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());
        await loader.LoadAssembliesAsync(new[] { "Test.wasm" });

        assemblyLoadContext.Received().LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream>());
    }

    [Fact]
    public async Task LoadAssembliesWithByOptions_ShouldHandleSymbolLoadError_WhenPdbFails()
    {
        var assemblyBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };

        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            if (GetRequestUri(request).ToString().Contains(".pdb"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(assemblyBytes)
            });
        });

        var options = new AssemblyLoaderOptions { EnableLogging = true };
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetName().Returns(new AssemblyName("Test"));
        mockAssembly.GetReferencedAssemblies().Returns(Array.Empty<AssemblyName>());
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), null).Returns(mockAssembly);
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(true);

        var bootJson = new BlazorBootModel { Resources = new Resources { Assembly = new Dictionary<string, string>() } };
        var bootHttpClient = CreateMockHttpClient((request, ct) =>
        {
            if (GetRequestUri(request).ToString().Contains("blazor.boot.json"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(bootJson), Encoding.UTF8, "application/json")
                });
            }
            if (GetRequestUri(request).ToString().Contains(".pdb"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(assemblyBytes)
            });
        });

        var loader = new BlazyAssemblyLoader(options, serviceProvider, "https://example.com/", bootHttpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());
        await loader.LoadAssembliesAsync(new[] { "Test.wasm" });

        logger.Received().WriteLine(Arg.Any<string>());
    }


    [Fact]
    public async Task LoadAssembliesAsync_ShouldEnableLogging_WhenEnableLoggingIsTrue()
    {
        var assemblyBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };

        var options = new AssemblyLoaderOptions { EnableLogging = true };
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetName().Returns(new AssemblyName("TestAssembly"));
        mockAssembly.FullName.Returns("TestAssembly, Version=1.0.0.0");
        mockAssembly.GetReferencedAssemblies().Returns(Array.Empty<AssemblyName>());
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), null).Returns(mockAssembly);
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(false);

        var bootJson = new BlazorBootModel { Resources = new Resources { Assembly = new Dictionary<string, string>() } };
        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            if (GetRequestUri(request).ToString().Contains("blazor.boot.json"))
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
        await loader.LoadAssembliesAsync(new[] { "TestAssembly.wasm" });

        logger.Received().WriteLine(Arg.Is<string>(s => s.Contains("Loaded Assembly")));
    }

    [Fact]
    public async Task LoadAssembliesAsync_ShouldCascadeLoad_WhenCascadeLoadingEnabled()
    {
        var assemblyBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };

        var options = new AssemblyLoaderOptions { DisableCascadeLoading = false };
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();

        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetName().Returns(new AssemblyName("TestAssembly"));
        mockAssembly.GetReferencedAssemblies().Returns(new[] { new AssemblyName("ReferencedAssembly") });

        var mockReferencedAssembly = Substitute.For<Assembly>();
        mockReferencedAssembly.GetName().Returns(new AssemblyName("ReferencedAssembly"));
        mockReferencedAssembly.GetReferencedAssemblies().Returns(Array.Empty<AssemblyName>());

        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), null).Returns(mockAssembly, mockReferencedAssembly);

        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(false);

        var bootJson = new BlazorBootModel { Resources = new Resources { Assembly = new Dictionary<string, string>() } };
        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            if (GetRequestUri(request).ToString().Contains("blazor.boot.json"))
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
        var result = await loader.LoadAssembliesAsync(new[] { "TestAssembly.wasm" });

        result.Should().NotBeNull();
        assemblyLoadContext.Received(2).LoadFromStream(Arg.Any<Stream>(), null);
    }

    [Fact]
    public async Task LoadAssembliesAsync_ShouldNotCascadeLoad_WhenCascadeLoadingDisabled()
    {
        var assemblyBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };

        var options = new AssemblyLoaderOptions { DisableCascadeLoading = true };
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();

        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetName().Returns(new AssemblyName("TestAssembly"));
        mockAssembly.GetReferencedAssemblies().Returns(new[] { new AssemblyName("ReferencedAssembly") });

        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), null).Returns(mockAssembly);

        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(false);

        var bootJson = new BlazorBootModel { Resources = new Resources { Assembly = new Dictionary<string, string>() } };
        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            if (GetRequestUri(request).ToString().Contains("blazor.boot.json"))
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
        var result = await loader.LoadAssembliesAsync(new[] { "TestAssembly.wasm" });

        result.Should().NotBeNull();
        assemblyLoadContext.Received(1).LoadFromStream(Arg.Any<Stream>(), null);
    }

    // This test only applies to pre-.NET 10 versions where blazor.boot.json exists.
    // In .NET 10+, blazor.boot.json is no longer used and preloaded assemblies are discovered via JS interop.
#if !NET10_0_OR_GREATER
    [Fact]
    public async Task LoadAssembliesAsync_ShouldSkipPreloadedAssemblies()
    {
        var hostAssembly = typeof(object).Assembly;
        var assemblyFileName = $"{hostAssembly.GetName().Name}.wasm";
        var assemblyBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };

        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(false);

        var bootJson = new BlazorBootModel
        {
            Resources = new Resources
            {
                Assembly = new Dictionary<string, string>
                {
                    [assemblyFileName] = "hash"
                }
            }
        };

        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            if (GetRequestUri(request).ToString().Contains("blazor.boot.json"))
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
        var result = await loader.LoadAssembliesAsync(new[] { assemblyFileName });

        Assert.Empty(result);
        assemblyLoadContext.DidNotReceive().LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream>());
    }
#endif

    [Fact]
    public async Task LoadAssembliesAsync_ShouldSkipRuntimeLoadedAssemblies()
    {
        // This test verifies that assemblies already loaded in the runtime are skipped.
        // We use "System.Private.CoreLib" as an example since it's always loaded in the runtime.
        var assemblyBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };

        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(false);

        var bootJson = new BlazorBootModel
        {
            Resources = new Resources
            {
                Assembly = new Dictionary<string, string>()
            }
        };

        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            if (GetRequestUri(request).ToString().Contains("blazor.boot.json"))
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

        // Try to load "System.Private.CoreLib.wasm" - this should be skipped because it's always loaded in the runtime
        var result = await loader.LoadAssembliesAsync(new[] { "System.Private.CoreLib.wasm" });

        Assert.Empty(result);
        assemblyLoadContext.DidNotReceive().LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream>());
    }

    [Fact]
    public async Task LoadAssembliesAsync_ShouldSkipRuntimeLoadedAssemblies_WithFingerprintedNames()
    {
        var hostAssembly = typeof(object).Assembly;
        var virtualName = $"{hostAssembly.GetName().Name}.wasm";
        var fingerprintedName = $"{hostAssembly.GetName().Name}.abc123xyz.wasm";
        var fingerprintResolver = Substitute.For<IFingerprintResolver>();
        fingerprintResolver.ResolveFingerprintedName(Arg.Any<string>()).Returns(call => call.Arg<string>());
        fingerprintResolver.ResolveFingerprintedName(virtualName).Returns(fingerprintedName);
        var fileLoader = Substitute.For<IAssemblyFileLoader>();
        var components = new BlazyAssemblyLoaderComponents
        {
            Options = new AssemblyLoaderOptions(),
            Logger = Substitute.For<IBlazyLogger>(),
            StateTracker = new AssemblyStateTracker { PreloadedAssemblies = [] },
            ExtensionInitializer = Substitute.For<IExtensionInitializer>(),
            FileLoader = fileLoader,
            OptionsResolver = Substitute.For<IAssemblyOptionsResolver>(),
            FingerprintResolver = fingerprintResolver
        };
        var loader = new BlazyAssemblyLoader(components);
        var result = await loader.LoadAssembliesAsync([fingerprintedName]);

        Assert.Empty(result);
        Assert.Empty(await loader.LoadAssemblyAsync(fingerprintedName));
        await fileLoader.DidNotReceive().LoadAssembliesWithOptionsAsync(Arg.Is<List<(string assemblyToLoad, AssemblyOptions options)>>(requests => requests.Count > 0));
    }

    [Fact]
    public async Task LoadAssembliesAsync_ShouldNotSkipUnknownAssemblies()
    {
        // This test verifies that assemblies NOT loaded in the runtime are still downloaded
        var assemblyBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };

        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetName().Returns(new AssemblyName("MyCustomAssembly"));
        mockAssembly.GetReferencedAssemblies().Returns(Array.Empty<AssemblyName>());
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), null).Returns(mockAssembly);
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(false);

        var bootJson = new BlazorBootModel
        {
            Resources = new Resources
            {
                Assembly = new Dictionary<string, string>()
            }
        };

        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            if (GetRequestUri(request).ToString().Contains("blazor.boot.json"))
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

        // Try to load "MyCustomAssembly.wasm" - this should NOT be skipped because it's not in the runtime
        var result = await loader.LoadAssembliesAsync(new[] { "MyCustomAssembly.wasm" });

        result.Should().NotBeEmpty();
        assemblyLoadContext.Received(1).LoadFromStream(Arg.Any<Stream>(), null);
    }

    [Fact]
    public void GetDllLocationFromOptions_ShouldReturnDefaultPath_WhenOptionsIsNull()
    {
        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var baseUrl = "https://example.com/";
        var httpClient = new HttpClient();
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();

        var loader = new BlazyAssemblyLoader(options, serviceProvider, baseUrl, httpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());

        var result = loader.GetDllLocationFromOptions(null);

        result.Should().Be("https://example.com/_framework/");
    }

    [Fact]
    public void GetDllLocationFromOptions_ShouldReturnAbsolutePath_WhenAbsolutePathIsSet()
    {
        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var baseUrl = "https://example.com/";
        var httpClient = new HttpClient();
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();

        var loader = new BlazyAssemblyLoader(options, serviceProvider, baseUrl, httpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());
        var assemblyOptions = new AssemblyOptions { AbsolutePath = "https://cdn.example.com/libs/" };

        var result = loader.GetDllLocationFromOptions(assemblyOptions);

        result.Should().Be("https://cdn.example.com/libs/");
    }

    [Fact]
    public void GetDllLocationFromOptions_ShouldReturnRelativePath_WhenRelativePathIsSet()
    {
        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var baseUrl = "https://example.com/";
        var httpClient = new HttpClient();
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();

        var loader = new BlazyAssemblyLoader(options, serviceProvider, baseUrl, httpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());
        var assemblyOptions = new AssemblyOptions { RelativePath = "custom/path/" };

        var result = loader.GetDllLocationFromOptions(assemblyOptions);

        result.Should().Be("https://example.com/custom/path/");
    }

    [Fact]
    public void GetDllLocationFromOptions_ShouldReturnDefaultPath_WhenNoPathsAreSet()
    {
        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var baseUrl = "https://example.com/";
        var httpClient = new HttpClient();
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();

        var loader = new BlazyAssemblyLoader(options, serviceProvider, baseUrl, httpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());
        var assemblyOptions = new AssemblyOptions();

        var result = loader.GetDllLocationFromOptions(assemblyOptions);

        result.Should().Be("https://example.com/_framework/");
    }

    [Fact]
    public async Task HandleNavigationInternal_ShouldLoadAssembly_WhenAssemblyConfigured()
    {
        var assemblyBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };

        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetName().Returns(new AssemblyName("TestAssembly"));
        mockAssembly.GetReferencedAssemblies().Returns(Array.Empty<AssemblyName>());
        assemblyLoadContext.LoadFromStream(Arg.Any<Stream>(), null).Returns(mockAssembly);
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();
        debuggerDetector.IsAttached.Returns(false);

        var bootJson = new BlazorBootModel { Resources = new Resources { Assembly = new Dictionary<string, string>() } };
        var httpClient = CreateMockHttpClient((request, ct) =>
        {
            if (GetRequestUri(request).ToString().Contains("blazor.boot.json"))
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

        var assemblyLoadConfig = new AssemblyLoadConfiguration();
        assemblyLoadConfig.Add("/test-page", "TestAssembly.wasm");
        var loader = new BlazyAssemblyLoader(options, serviceProvider, "https://example.com/", httpClient, assemblyLoadContext, logger, debuggerDetector, assemblyLoadConfig);

        await loader.HandleNavigationInternal("/test-page");

        assemblyLoadContext.Received(1).LoadFromStream(Arg.Any<Stream>(), null);
    }

    [Fact]
    public async Task HandleNavigationInternal_ShouldNotThrow_WhenAssemblyNotConfigured()
    {
        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var httpClient = new HttpClient();
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();

        var loader = new BlazyAssemblyLoader(options, serviceProvider, "https://example.com/", httpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());

        await loader.HandleNavigationInternal("/unknown-page");

        assemblyLoadContext.DidNotReceive().LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream>());
    }

    [Fact]
    public async Task HandleNavigationInternal_ShouldNotThrow_WhenConfigurationIsEmpty()
    {
        var options = new AssemblyLoaderOptions();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());
        var httpClient = new HttpClient();
        var assemblyLoadContext = Substitute.For<IAssemblyLoadContext>();
        var logger = Substitute.For<IBlazyLogger>();
        var debuggerDetector = Substitute.For<IDebuggerDetector>();

        var loader = new BlazyAssemblyLoader(options, serviceProvider, "https://example.com/", httpClient, assemblyLoadContext, logger, debuggerDetector, new AssemblyLoadConfiguration());

        await loader.HandleNavigationInternal("/test");

        assemblyLoadContext.DidNotReceive().LoadFromStream(Arg.Any<Stream>(), Arg.Any<Stream>());
    }
}
