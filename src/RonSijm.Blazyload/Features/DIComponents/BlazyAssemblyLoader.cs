// ReSharper disable global EventNeverSubscribedTo.Global - Justification: Used by library consumers
// ReSharper disable global UnusedMember.Global - Justification: Used by library consumers

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using RonSijm.Blazyload.Loading;
using RonSijm.Syringe;
using System.Diagnostics;
using System.Net.Http.Json;

namespace RonSijm.Blazyload;

public class BlazyAssemblyLoader(SyringeServiceProvider serviceProvider, NavigationManager navigationManager) : IAssemblyLoader
{
    private HashSet<string> _loadedAssemblyHashes = [];
    private HashSet<string> _preloadedAssemblies;

    public SyringeServiceProvider ServiceProvider { get; } = serviceProvider;

    public event Action<List<Assembly>> OnAssembliesLoaded;
    public event Action<List<ServiceDescriptor>> OnDescriptorsLoaded;

    public async Task<List<Assembly>> LoadAssemblyAsync(string assemblyToLoad)
    {
        return await LoadAssembliesAsync([assemblyToLoad], false);
    }

    public async Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad)
    {
        return await LoadAssembliesAsync(assembliesToLoad, false);
    }

    public List<Assembly> LoadedAssemblies { get; } = new();

    [Inject]
    public AssemblyLoadConfiguration AssemblyLoadConfiguration { get; set; }

    public async Task HandleNavigation(NavigationContext args)
    {
        try
        {
            var assembly = AssemblyLoadConfiguration.GetAssembly(args.Path);
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
    /// Method is private because I don't want outsiders to call this with isRecursive = true, as it will mess things up.
    /// </summary>
    private async Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad, bool isRecursive, List<ServiceDescriptor> loadedDescriptors = null)
    {
        _preloadedAssemblies ??= await GetPreloadedAssemblies();
        loadedDescriptors ??= new List<ServiceDescriptor>();

        var assembliesToLoadAsList = assembliesToLoad as List<string> ?? assembliesToLoad.ToList();
        var unattemptedAssemblies = assembliesToLoadAsList
            .Where(x => !_loadedAssemblyHashes.Contains(x))
            .Where(x => !_preloadedAssemblies.Contains(x)).ToList();

        var loadedAssemblies = new List<Assembly>();

        try
        {
            var assemblyWithOptions = new List<(string assemblyToLoad, AssemblyOptions options)>();
            var blazyOptions = ServiceProvider.Options.GetOptions<AssemblyLoadOptions>();

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

            var optionsModel = ServiceProvider.Options.GetOptions<AssemblyLoadOptions>();
            var serviceDescriptors = LibraryLoader.GetServiceDescriptorsOfAssemblies(optionsModel, assemblies);

            var loadedServiceDescriptors = await ServiceProvider.LoadServiceDescriptors(serviceDescriptors);
            loadedDescriptors.AddRange(loadedServiceDescriptors);

            foreach (var assembly in assemblies)
            {
                var assemblyName = $"{assembly.GetName().Name}";
                var assemblyOptions = optionsModel?.GetOptions(assemblyName);

                if (BlazyloadProviderOptions.DisableCascadeLoadingGlobally && assemblyOptions is { DisableCascadeLoading: true })
                {
                    continue;
                }

                var referenceAssemblies = assembly.GetReferencedAssemblies();
                var assemblyNames = referenceAssemblies.Select(referenceAssembly => $"{referenceAssembly.Name}.wasm");

                var cascadeResults = await LoadAssembliesAsync(assemblyNames, true);
            }
        }
        catch (Exception e)
        {
            if (!isRecursive || BlazyloadProviderOptions.EnableLoggingForCascadeErrors)
            {

                Console.WriteLine(e);
            }
        }

        _loadedAssemblyHashes = _loadedAssemblyHashes.Concat(unattemptedAssemblies).ToHashSet();

        if (isRecursive)
        {
            return loadedAssemblies;
        }

        // Only at the end of the recursive loop we rebuild the service provider, and call consumers
        ServiceProvider.Build();
        OnAssembliesLoaded?.Invoke(loadedAssemblies);
        OnDescriptorsLoaded?.Invoke(loadedDescriptors);

        LoadedAssemblies.AddRange(loadedAssemblies);
        return loadedAssemblies;
    }

    private async Task<HashSet<string>> GetPreloadedAssemblies()
    {
        var preloadedDlls = new HashSet<string>();

        try
        {
            using var client = new HttpClient();
            var result = await client.GetFromJsonAsync<BlazorBootModel>($"{navigationManager.BaseUri}/_framework/blazor.boot.json");

            foreach (var assembly in result.Resources.Assembly)
            {
                preloadedDlls.Add(assembly.Key);
            }

            return preloadedDlls;
        }
        catch (Exception e)
        {
            Console.WriteLine(@"Error while attempting discover preloaded dlls.");
            Console.WriteLine(e);
        }

        return preloadedDlls;
    }

    private async Task<Assembly[]> LoadAssembliesWithByOptions(List<(string assemblyToLoad, AssemblyOptions options)> assembliesToLoad)
    {
        var loadedAssemblies = new List<Assembly>();

        using var client = new HttpClient();

        foreach (var (assemblyToLoad, options) in assembliesToLoad)
        {
            var loadedAssemblyBytes = await LoadFileAssembly(assemblyToLoad, options, client);

            Stream loadedSymbols = null;

            if (Debugger.IsAttached)
            {
                // Dotnet6+7 use a DLL, Dotnet8 changed it to .wasm
                var symbolsToLoad = assemblyToLoad.Replace(".wasm", ".pdb").Replace(".dll", ".pdb");
                loadedSymbols = await LoadFileAssembly(symbolsToLoad, options, client);
            }

            if (loadedAssemblyBytes != null)
            {
                var assembly = AssemblyLoadContext.Default.LoadFromStream(loadedAssemblyBytes, loadedSymbols);
                loadedAssemblies.Add(assembly);
            }
        }

        return loadedAssemblies.ToArray();
    }

    private async Task<Stream> LoadFileAssembly(string assemblyToLoad, AssemblyOptions options, HttpClient client)
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

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // TODO: We could add an option to do enable sha- validation

        var contentBytes = await response.Content.ReadAsStreamAsync();

        return contentBytes;
    }

    private string GetDllLocationFromOptions(AssemblyOptions options)
    {
        var dllLocation = options == null ? // If options is null,
            $"{navigationManager.BaseUri}/_framework/" : // use default path
            options.AbsolutePath ?? // If AbsolutePath isn't null, use that.
            (options.RelativePath != null ? $"{navigationManager.BaseUri}{options.RelativePath}" : // If RelativePath isn't null, use base path + RelativePath
                $"{navigationManager.BaseUri}/_framework/"); // Else just use default path

        return dllLocation;
    }
}