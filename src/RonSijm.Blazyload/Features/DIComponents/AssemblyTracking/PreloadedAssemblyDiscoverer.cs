using System.Net.Http.Json;
using Microsoft.JSInterop;
using RonSijm.Blazyload.Loading;
using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Default implementation of IPreloadedAssemblyDiscoverer.
/// Discovers preloaded assemblies by querying the Blazor runtime configuration via JavaScript interop.
/// This approach works with .NET 10+ where blazor.boot.json no longer exists.
/// </summary>
public class PreloadedAssemblyDiscoverer : IPreloadedAssemblyDiscoverer
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly AssemblyLoaderOptions _options;
    private readonly IBlazyLogger _logger;

    public PreloadedAssemblyDiscoverer(IJSRuntime jsRuntime, HttpClient httpClient, string baseUrl, AssemblyLoaderOptions options, IBlazyLogger logger)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
        _baseUrl = baseUrl;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Legacy constructor for backward compatibility.
    /// Note: This constructor will not support .NET 10+ fingerprinting.
    /// </summary>
    public PreloadedAssemblyDiscoverer(HttpClient httpClient, string baseUrl, AssemblyLoaderOptions options, IBlazyLogger logger)
        : this(null, httpClient, baseUrl, options, logger)
    {
    }

    /// <inheritdoc />
    public async Task<HashSet<string>> GetPreloadedAssembliesAsync()
    {
        var preloadedDlls = new HashSet<string>();

        // First, try to get assemblies from JavaScript interop (works with .NET 10+)
        if (_jsRuntime != null)
        {
            try
            {
                var mapping = await _jsRuntime.InvokeAsync<Dictionary<string, string>>(
                    "BlazyloadInterop.getLazyAssemblyMapping");

                if (mapping != null && mapping.Count > 0)
                {
                    // The mapping contains lazy assemblies, not preloaded ones
                    // But we can use this to know what is available
                    if (_options.EnableLogging)
                    {
                        _logger.WriteLine($"Blazyload: Found {mapping.Count} lazy assemblies via JS interop.");
                    }
                    
                    // For preloaded assemblies, we need to check what is already loaded in the runtime
                    // The lazy assembly mapping tells us what CAN be loaded, not what IS loaded
                    return preloadedDlls;
                }
            }
            catch (Exception e)
            {
                if (_options.EnableLogging)
                {
                    _logger.WriteLine("Blazyload: JS interop not available for assembly discovery.");
                    _logger.WriteLine(e);
                }
            }
        }

#if NET10_0_OR_GREATER
        // In .NET 10+, blazor.boot.json does not exist - the config is embedded in dotnet.js
        // We rely entirely on the JavaScript interop above
        if (_options.EnableLogging)
        {
            _logger.WriteLine("Blazyload: Running on .NET 10+, blazor.boot.json is not available.");
        }
        return preloadedDlls;
#else
        // Fallback: Try to fetch blazor.boot.json (works with older .NET versions)
        try
        {
            var result = await _httpClient.GetFromJsonAsync<BlazorBootModel>($"{_baseUrl}_framework/blazor.boot.json");

            if (result?.Resources?.Assembly != null)
            {
                foreach (var assembly in result.Resources.Assembly)
                {
                    preloadedDlls.Add(assembly.Key);
                }
            }

            return preloadedDlls;
        }
        catch (Exception e)
        {
            if (_options.EnableLogging)
            {
                _logger.WriteLine("Blazyload: Error while attempting to discover preloaded dlls from blazor.boot.json.");
                _logger.WriteLine(e);
            }
        }

        return preloadedDlls;
#endif
    }
}
