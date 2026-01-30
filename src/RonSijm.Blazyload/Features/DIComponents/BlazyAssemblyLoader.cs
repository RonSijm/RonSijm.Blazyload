// ReSharper disable global EventNeverSubscribedTo.Global - Justification: Used by library consumers
// ReSharper disable global UnusedMember.Global - Justification: Used by library consumers

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using RonSijm.Blazyload.Loading;
using RonSijm.Syringe;
using System.Net.Http.Json;

namespace RonSijm.Blazyload;

public class BlazyAssemblyLoader : IBlazyAssemblyLoader
{
    private HashSet<string> _loadedAssemblyHashes = [];
    private HashSet<string> _preloadedAssemblies;
    private HashSet<string> _runtimeLoadedAssemblies;

    private readonly Syringe.AssemblyLoaderOptions _options;
    private readonly SyringeServiceProvider _serviceProvider;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;
    private readonly IAssemblyLoadContext _assemblyLoadContext;
    private readonly IBlazyLogger _logger;
    private readonly IDebuggerDetector _debuggerDetector;
    private readonly AssemblyLoadConfiguration _assemblyLoadConfiguration;

    public BlazyAssemblyLoader(Syringe.AssemblyLoaderOptions options, SyringeServiceProvider serviceProvider, NavigationManager navigationManager, AssemblyLoadConfiguration assemblyLoadConfiguration)
        : this(options, serviceProvider, navigationManager.BaseUri, new HttpClient(), new DefaultAssemblyLoadContext(), new ConsoleLogger(), new DefaultDebuggerDetector(), assemblyLoadConfiguration)
    {
    }

    internal BlazyAssemblyLoader(Syringe.AssemblyLoaderOptions options, SyringeServiceProvider serviceProvider, string baseUrl)
        : this(options, serviceProvider, baseUrl, new HttpClient(), new DefaultAssemblyLoadContext(), new ConsoleLogger(), new DefaultDebuggerDetector(), new AssemblyLoadConfiguration())
    {
    }

    internal BlazyAssemblyLoader(Syringe.AssemblyLoaderOptions options, SyringeServiceProvider serviceProvider, string baseUrl, HttpClient httpClient, IAssemblyLoadContext assemblyLoadContext, IBlazyLogger logger, IDebuggerDetector debuggerDetector, AssemblyLoadConfiguration assemblyLoadConfiguration)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _baseUrl = baseUrl;
        _httpClient = httpClient;
        _assemblyLoadContext = assemblyLoadContext;
        _logger = logger;
        _debuggerDetector = debuggerDetector;
        _assemblyLoadConfiguration = assemblyLoadConfiguration;

        if (!_baseUrl.EndsWith('/'))
        {
            _baseUrl += '/';
        }

        foreach (var optionsAfterLoadAssembliesExtension in _options.AfterLoadAssembliesExtensions)
        {
            optionsAfterLoadAssembliesExtension.SetReference(serviceProvider);
        }
    }

    public async Task<List<Assembly>> LoadAssemblyAsync(string assemblyToLoad)
    {
        return await LoadAssembliesAsync([assemblyToLoad], false);
    }

    public async Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad)
    {
        return await LoadAssembliesAsync(assembliesToLoad, false);
    }

    /// <summary>
    /// This is a list of 'Loaded Assemblies'
    /// It's called AdditionalAssemblies to match the parameter name of the router - to make it easier to wire.
    /// </summary>
    public List<Assembly> AdditionalAssemblies { get; } = new();

    public async Task OnNavigateAsync(NavigationContext args)
    {
        await HandleNavigationInternal(args.Path);
    }

    internal async Task HandleNavigationInternal(string path)
    {
        try
        {
            var assembly = _assemblyLoadConfiguration.GetAssembly(path);
            if (assembly != null)
            {
                await LoadAssemblyAsync(assembly);
            }
        }
        catch (Exception)
        {
            // Do Nothing
        }
    }

    /// <summary>
    /// Method that actually begins loading the assemblies.
    /// Method is internal for testing purposes. External callers should use the public overloads.
    /// </summary>
    internal async Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad, bool isRecursive, List<ServiceDescriptor> loadedDescriptors = null)
    {
        _preloadedAssemblies ??= await GetPreloadedAssemblies();
        _runtimeLoadedAssemblies ??= GetRuntimeLoadedAssemblies();
        loadedDescriptors ??= new List<ServiceDescriptor>();

        var assembliesToLoadAsList = assembliesToLoad as List<string> ?? assembliesToLoad.ToList();
        var unattemptedAssemblies = assembliesToLoadAsList
            .Where(x => !_loadedAssemblyHashes.Contains(x))
            .Where(x => !_preloadedAssemblies.Contains(x))
            .Where(x => !IsAlreadyLoadedInRuntime(x))
            .ToList();

        var loadedAssemblies = new List<Assembly>();

        try
        {
            var assemblyWithOptions = new List<(string assemblyToLoad, AssemblyOptions options)>();
            var blazyOptions = _serviceProvider.Options.GetOptions<AssemblyLoadOptions>();

            foreach (var assemblyToLoad in unattemptedAssemblies)
            {
                var options = blazyOptions?.GetOptions(assemblyToLoad);
                assemblyWithOptions.Add((assemblyToLoad, options));
            }

            var assemblies = await LoadAssembliesWithByOptions(assemblyWithOptions);

            if (!assemblies.Any())
            {
                return loadedAssemblies;
            }

            loadedAssemblies.AddRange(assemblies);

            var optionsModel = _serviceProvider.Options.GetOptions<AssemblyLoadOptions>();

            // IMPORTANT: Cascade load referenced assemblies BEFORE bootstrapping.
            // This ensures that when a bootstrapper uses types from referenced assemblies,
            // those assemblies are already loaded. Without this order, the bootstrapper
            // would fail with FileNotFoundException when trying to use types from
            // referenced assemblies that haven't been loaded yet.
            foreach (var assembly in assemblies)
            {
                var assemblyName = $"{assembly.GetName().Name}";
                var assemblyOptions = optionsModel?.GetOptions(assemblyName);

                if (_options.DisableCascadeLoading || assemblyOptions is { DisableCascadeLoading: true })
                {
                    continue;
                }

                var referenceAssemblies = assembly.GetReferencedAssemblies();
                var assemblyNames = referenceAssemblies.Select(referenceAssembly => $"{referenceAssembly.Name}.wasm");

                var cascadeResults = await LoadAssembliesAsync(assemblyNames, true, loadedDescriptors);
                loadedAssemblies.AddRange(cascadeResults);
            }

            // Now that all referenced assemblies are loaded, we can safely bootstrap.
            // The bootstrapper may use types from referenced assemblies.
            var serviceDescriptors = LibraryLoader.GetServiceDescriptorsOfAssemblies(optionsModel, assemblies);

            var loadedServiceDescriptors = await _serviceProvider.LoadServiceDescriptors(serviceDescriptors);
            loadedDescriptors.AddRange(loadedServiceDescriptors);
        }
        catch (Exception e)
        {
            if (!isRecursive || _options.EnableLoggingForCascadeErrors)
            {
                _logger.WriteLine(e);
            }
        }

        _loadedAssemblyHashes = _loadedAssemblyHashes.Concat(unattemptedAssemblies).ToHashSet();

        if (isRecursive)
        {
            return loadedAssemblies;
        }

        // Only at the end of the recursive loop we rebuild the service provider, and call consumers
        _serviceProvider.Build();

        _options.AfterLoadAssembliesExtensions.ForEach(x => x.AssembliesLoaded(loadedAssemblies));
        _options.AfterLoadAssembliesExtensions.ForEach(x => x.DescriptorsLoaded(loadedDescriptors));
        
        if (_options.EnableLogging)
        {
            foreach (var loadedAssembly in loadedAssemblies)
            {
                _logger.WriteLine($"Loaded Assembly: {loadedAssembly.FullName}");
            }
        }

        AdditionalAssemblies.AddRange(loadedAssemblies);
        return loadedAssemblies;
    }

    private async Task<HashSet<string>> GetPreloadedAssemblies()
    {
        var preloadedDlls = new HashSet<string>();

        try
        {
            var result = await _httpClient.GetFromJsonAsync<BlazorBootModel>($"{_baseUrl}_framework/blazor.boot.json");

            foreach (var assembly in result.Resources.Assembly)
            {
                preloadedDlls.Add(assembly.Key);
            }

            return preloadedDlls;
        }
        catch (Exception e)
        {
            if (_options.EnableLogging)
            {
                _logger.WriteLine(@"Error while attempting discover preloaded dlls.");
                _logger.WriteLine(e);
            }
        }

        return preloadedDlls;
    }

    /// <summary>
    /// Gets assemblies that are already loaded in the .NET runtime.
    /// This is used to avoid downloading assemblies that are already available.
    /// </summary>
    private HashSet<string> GetRuntimeLoadedAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetName().Name)
            .Where(name => name != null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if an assembly (by .wasm filename) is already loaded in the runtime.
    /// Handles both simple names (e.g., "Microsoft.Extensions.Options.wasm") and
    /// fingerprinted names (e.g., "Microsoft.Extensions.Options.moch3gvzt4.wasm").
    /// </summary>
    private bool IsAlreadyLoadedInRuntime(string assemblyFileName)
    {
        // Remove .wasm extension to get the base name
        var baseName = assemblyFileName;
        if (baseName.EndsWith(".wasm", StringComparison.OrdinalIgnoreCase))
        {
            baseName = baseName[..^5]; // Remove ".wasm"
        }

        // Check if this exact name is loaded
        if (_runtimeLoadedAssemblies.Contains(baseName))
        {
            return true;
        }

        // Handle fingerprinted names like "Microsoft.Extensions.Options.moch3gvzt4"
        // The fingerprint is typically a 10-character alphanumeric string at the end
        var lastDotIndex = baseName.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            var potentialFingerprint = baseName[(lastDotIndex + 1)..];
            // Fingerprints are typically 10 alphanumeric characters
            if (potentialFingerprint.Length >= 8 && potentialFingerprint.Length <= 12 &&
                potentialFingerprint.All(c => char.IsLetterOrDigit(c)))
            {
                var nameWithoutFingerprint = baseName[..lastDotIndex];
                if (_runtimeLoadedAssemblies.Contains(nameWithoutFingerprint))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private async Task<Assembly[]> LoadAssembliesWithByOptions(List<(string assemblyToLoad, AssemblyOptions options)> assembliesToLoad)
    {
        var loadedAssemblies = new List<Assembly>();

        foreach (var (assemblyToLoad, assemblyOptions) in assembliesToLoad)
        {
            var loadedAssemblyBytes = await LoadFileAssembly(assemblyToLoad, assemblyOptions);

            Stream loadedSymbols = null;

            if (_debuggerDetector.IsAttached)
            {
                try
                {
                    // Dotnet6+7 use a DLL, Dotnet8 changed it to .wasm
                    var symbolsToLoad = assemblyToLoad.Replace(".wasm", ".pdb").Replace(".dll", ".pdb");
                    loadedSymbols = await LoadFileAssembly(symbolsToLoad, assemblyOptions);
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

    private async Task<Stream> LoadFileAssembly(string assemblyToLoad, AssemblyOptions options)
    {
        var dllLocation = GetDllLocationFromOptions(options);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{dllLocation}{assemblyToLoad}");

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

    internal string GetDllLocationFromOptions(AssemblyOptions options)
    {
        var dllLocation = options == null ? // If options is null,
            $"{_baseUrl}_framework/" : // use default path
            options.AbsolutePath ?? // If AbsolutePath isn't null, use that.
            (options.RelativePath != null ? $"{_baseUrl}{options.RelativePath}" : // If RelativePath isn't null, use base path + RelativePath
                $"{_baseUrl}_framework/"); // Else just use default path

        return dllLocation;
    }
}