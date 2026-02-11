using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Default implementation of IAssemblyFileLoader.
/// Handles downloading assembly files from HTTP and loading them into the runtime.
/// </summary>
public class AssemblyFileLoader : IAssemblyFileLoader
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly AssemblyLoaderOptions _options;
    private readonly IAssemblyLoadContext _assemblyLoadContext;
    private readonly IBlazyLogger _logger;
    private readonly IDebuggerDetector _debuggerDetector;
    private readonly IFingerprintResolver _fingerprintResolver;

    public AssemblyFileLoader(
        HttpClient httpClient,
        string baseUrl,
        AssemblyLoaderOptions options,
        IAssemblyLoadContext assemblyLoadContext,
        IBlazyLogger logger,
        IDebuggerDetector debuggerDetector,
        IFingerprintResolver fingerprintResolver = null)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
        _options = options;
        _assemblyLoadContext = assemblyLoadContext;
        _logger = logger;
        _debuggerDetector = debuggerDetector;
        _fingerprintResolver = fingerprintResolver;
    }

    /// <inheritdoc />
    public async Task<Assembly[]> LoadAssembliesWithOptionsAsync(List<(string assemblyToLoad, AssemblyOptions options)> assembliesToLoad)
    {
        // Ensure fingerprint resolver is initialized before loading assemblies
        if (_fingerprintResolver != null && !_fingerprintResolver.IsInitialized)
        {
            await _fingerprintResolver.InitializeAsync();
        }

        var loadedAssemblies = new List<Assembly>();

        foreach (var (assemblyToLoad, assemblyOptions) in assembliesToLoad)
        {
            var loadedAssemblyBytes = await LoadFileAssemblyAsync(assemblyToLoad, assemblyOptions);

            Stream loadedSymbols = null;

            if (_debuggerDetector.IsAttached)
            {
                try
                {
                    // Dotnet6+7 use a DLL, Dotnet8 changed it to .wasm
                    var symbolsToLoad = assemblyToLoad.Replace(".wasm", ".pdb").Replace(".dll", ".pdb");
                    loadedSymbols = await LoadFileAssemblyAsync(symbolsToLoad, assemblyOptions);
                }
                catch (Exception e)
                {
                    if (_options.EnableLogging)
                    {
                        _logger.WriteLine($@"Error while trying to load pdb for assembly '{assemblyToLoad}'.");
                        _logger.WriteLine(e);
                    }
                }
            }

            if (loadedAssemblyBytes != null)
            {
                var assembly = _assemblyLoadContext.LoadFromStream(loadedAssemblyBytes, loadedSymbols);
                loadedAssemblies.Add(assembly);
            }
        }

        return loadedAssemblies.ToArray();
    }

    /// <inheritdoc />
    public string GetDllLocationFromOptions(AssemblyOptions options)
    {
        var dllLocation = options == null ? // If options is null,
            $"{_baseUrl}_framework/" : // use default path
            options.AbsolutePath ?? // If AbsolutePath isn't null, use that.
            (options.RelativePath != null ? $"{_baseUrl}{options.RelativePath}" : // If RelativePath isn't null, use base path + RelativePath
                $"{_baseUrl}_framework/"); // Else just use default path

        return dllLocation;
    }

    private async Task<Stream> LoadFileAssemblyAsync(string assemblyToLoad, AssemblyOptions options)
    {
        var dllLocation = GetDllLocationFromOptions(options);

        // Resolve the fingerprinted name if fingerprint resolver is available
        var actualFileName = assemblyToLoad;
        if (_fingerprintResolver != null)
        {
            actualFileName = _fingerprintResolver.ResolveFingerprintedName(assemblyToLoad);
            
            if (_options.EnableLogging && actualFileName != assemblyToLoad)
            {
                _logger.WriteLine($"Blazyload: Resolved '{assemblyToLoad}' to fingerprinted name '{actualFileName}'");
            }
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"{dllLocation}{actualFileName}");

        if (options is { HttpHandler: { } })
        {
            var isSuccess = options.HttpHandler.Invoke(assemblyToLoad, request, options);

            if (!isSuccess)
            {
                // HttpHandler indicated we can't process this
                return null;
            }
        }

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // TODO: We could add an option to do enable sha- validation

        var contentBytes = await response.Content.ReadAsStreamAsync();

        return contentBytes;
    }
}
