using System.Text.Json;
using Microsoft.JSInterop;
using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Default implementation of IFingerprintResolver.
/// Uses JavaScript interop to read the lazy assembly fingerprint mapping directly from the Blazor runtime's
/// in-memory configuration. This is more efficient than fetching and parsing dotnet.js via HTTP since the
/// configuration is already loaded in memory by the time Blazor starts.
/// </summary>
public class FingerprintResolver : IFingerprintResolver
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IBlazyLogger _logger;
    private readonly AssemblyLoaderOptions _options;
    private Dictionary<string, string> _fingerprintMapping = new();
    private bool _isInitialized;
    private bool _isFingerprintingEnabled;

    // JavaScript code to read ALL assembly configurations from the Blazor runtime
    // This includes lazyAssembly, assembly, and other resource sections to support cascade loading
    // of dependencies that may not be in the lazyAssembly list
    private const string GetAllAssemblyConfigScript = @"
        (function() {
            try {
                const runtime = getDotnetRuntime(0);
                if (!runtime) return null;

                const config = runtime.getConfig();
                if (!config || !config.resources) return null;

                const result = [];

                // Helper function to extract mappings from an array of assembly assets
                function extractMappings(assets) {
                    if (!assets || !Array.isArray(assets)) return;
                    for (const a of assets) {
                        if (a.virtualPath && a.name) {
                            result.push({ virtualPath: a.virtualPath, name: a.name });
                        }
                    }
                }

                // Read from all relevant resource sections
                // lazyAssembly - explicitly lazy-loaded assemblies
                extractMappings(config.resources.lazyAssembly);

                // assembly - regular assemblies (includes dependencies)
                extractMappings(config.resources.assembly);

                // coreAssembly - core framework assemblies
                extractMappings(config.resources.coreAssembly);

                // satelliteResources - localization assemblies (if any)
                if (config.resources.satelliteResources) {
                    for (const culture in config.resources.satelliteResources) {
                        extractMappings(config.resources.satelliteResources[culture]);
                    }
                }

                return result.length > 0 ? result : null;
            } catch (e) {
                console.warn('Blazyload: Error reading assembly config:', e);
                return null;
            }
        })()
    ";

    public FingerprintResolver(IJSRuntime jsRuntime, IBlazyLogger logger, AssemblyLoaderOptions options)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc />
    public bool IsInitialized => _isInitialized;

    /// <inheritdoc />
    public bool IsFingerprintingEnabled => _isFingerprintingEnabled;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        try
        {
            // Read the lazy assembly config directly from the Blazor runtime's memory via JS interop
            var mapping = await GetLazyAssemblyMappingViaJsInteropAsync();

            if (mapping != null && mapping.Count > 0)
            {
                _fingerprintMapping = mapping;

                // Check if any mappings differ from their virtual paths (indicating fingerprinting is enabled)
                foreach (var kvp in mapping)
                {
                    if (kvp.Key != kvp.Value)
                    {
                        _isFingerprintingEnabled = true;
                        break;
                    }
                }

                if (_options.EnableLogging)
                {
                    _logger.WriteLine($"Blazyload: Loaded {mapping.Count} assembly fingerprint mappings via JS interop. Fingerprinting enabled: {_isFingerprintingEnabled}");
                }
            }
            else if (_options.EnableLogging)
            {
                _logger.WriteLine("Blazyload: No assembly config found. Fingerprinting may not be enabled or no assemblies are configured.");
            }
        }
        catch (Exception e)
        {
            if (_options.EnableLogging)
            {
                _logger.WriteLine("Blazyload: Error initializing fingerprint resolver via JS interop. Falling back to non-fingerprinted names.");
                _logger.WriteLine(e);
            }
        }

        _isInitialized = true;
    }

    /// <summary>
    /// Reads ALL assembly configurations from the Blazor runtime's memory via JavaScript interop.
    /// This includes lazyAssembly, assembly, coreAssembly, and satelliteResources to support
    /// cascade loading of dependencies that may not be in the lazyAssembly list.
    /// </summary>
    private async Task<Dictionary<string, string>> GetLazyAssemblyMappingViaJsInteropAsync()
    {
        var mapping = new Dictionary<string, string>();

        try
        {
            var allAssemblies = await _jsRuntime.InvokeAsync<LazyAssemblyEntry[]>("eval", GetAllAssemblyConfigScript);

            if (allAssemblies != null)
            {
                foreach (var entry in allAssemblies)
                {
                    if (!string.IsNullOrEmpty(entry.VirtualPath) && !string.IsNullOrEmpty(entry.Name))
                    {
                        // Use TryAdd to avoid duplicates (first entry wins)
                        mapping.TryAdd(entry.VirtualPath, entry.Name);
                    }
                }
            }
        }
        catch (JSException ex)
        {
            if (_options.EnableLogging)
            {
                _logger.WriteLine($"Blazyload: JS interop error reading assembly config: {ex.Message}");
            }
        }

        return mapping;
    }

    /// <inheritdoc />
    public string ResolveFingerprintedName(string virtualName)
    {
        if (string.IsNullOrEmpty(virtualName))
        {
            return virtualName;
        }

        // Try to find the fingerprinted name in the mapping
        if (_fingerprintMapping.TryGetValue(virtualName, out var fingerprintedName))
        {
            return fingerprintedName;
        }

        // If not found, return the original name
        return virtualName;
    }

    /// <summary>
    /// Represents an entry in the lazyAssembly array.
    /// </summary>
    private class LazyAssemblyEntry
    {
        public string VirtualPath { get; set; }
        public string Name { get; set; }
    }
}
