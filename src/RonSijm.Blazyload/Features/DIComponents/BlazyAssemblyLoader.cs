namespace RonSijm.Blazyload.Features.DIComponents;

public class BlazyAssemblyLoader
{
    private HashSet<string> _loadedAssemblies = new();

    private readonly BlazyServiceProvider _blazyServiceProvider;
    private readonly NavigationManager _navigationManager;

    public BlazyAssemblyLoader(BlazyServiceProvider blazyServiceProvider, NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _blazyServiceProvider = blazyServiceProvider;
    }

    public event Action<List<Assembly>> OnAssembliesLoaded;

    public async Task<List<Assembly>> LoadAssemblyAsync(string assemblyToLoad)
    {
        return await LoadAssembliesAsync(new[] { assemblyToLoad }, false);
    }

    public async Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad)
    {
        return await LoadAssembliesAsync(assembliesToLoad, false);
    }

    /// <summary>
    /// Method that actually begins loading the assemblies.
    /// Method is private because I don't want outsiders to call this with isRecursive = true, as it will mess things up.
    /// </summary>
    private async Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad, bool isRecursive)
    {
        var assembliesToLoadAsList = assembliesToLoad as List<string> ?? assembliesToLoad.ToList();
        var unattemptedAssemblies = assembliesToLoadAsList.Where(x => !_loadedAssemblies.Contains(x)).ToList();

        var loadedAssemblies = new List<Assembly>();

        try
        {
            _loadedAssemblies = _loadedAssemblies.Concat(unattemptedAssemblies).ToHashSet();

            var assemblyWithOptions = new List<(string assemblyToLoad, BlazyAssemblyOptions options)>();
            foreach (var assemblyToLoad in unattemptedAssemblies)
            {
                BlazyAssemblyOptions options = _blazyServiceProvider.Options.GetOptions(assemblyToLoad);
                assemblyWithOptions.Add((assemblyToLoad, options));
            }

            var assemblies = await LoadAssembliesWithByOptions(assemblyWithOptions);

            if (!assemblies.Any())
            {
                return loadedAssemblies;
            }

            loadedAssemblies.AddRange(assemblies);

            await _blazyServiceProvider.Register(assemblies);

            foreach (var assembly in assemblies)
            {
                var assemblyName = $"{assembly.GetName().Name}";
                var assemblyOptions = _blazyServiceProvider.Options?.GetOptions(assemblyName);

                if (BlazyOptions.DisableCascadeLoadingGlobally && assemblyOptions is { DisableCascadeLoading: true })
                {
                    continue;
                }

                var referenceAssemblies = assembly.GetReferencedAssemblies();
                var assemblyNames = referenceAssemblies.Select(referenceAssembly => $"{referenceAssembly.Name}.dll");

                // "Oh no! recursion! ಠ_ಠ" - Possibly implement trampoline pattern if this causes issues
                var cascadeResults = await LoadAssembliesAsync(assemblyNames, true);
            }
        }
        catch (Exception e)
        {
            if (!isRecursive || BlazyOptions.EnableLoggingForCascadeErrors)
            {
                Console.WriteLine(e);
            }
        }

        if (isRecursive)
        {
            return loadedAssemblies;
        }

        // Only at the end of the recursive loop we rebuild the service provider, and call consumers
        _blazyServiceProvider.CreateServiceProvider();
        OnAssembliesLoaded?.Invoke(loadedAssemblies);

        return loadedAssemblies;
    }

    private async Task<Assembly[]> LoadAssembliesWithByOptions(List<(string assemblyToLoad, BlazyAssemblyOptions options)> assembliesToLoad)
    {
        var loadedAssemblies = new List<Assembly>();

        using var client = new HttpClient();

        foreach (var assemblyToLoad in assembliesToLoad)
        {
            var dllLocation = GetDllLocationFromOptions(assemblyToLoad);

            var request = new HttpRequestMessage(HttpMethod.Get, $"{dllLocation}{assemblyToLoad.assemblyToLoad}");

            if (assemblyToLoad.options is { HttpHandler: { } })
            {
                var isSuccess = assemblyToLoad.options.HttpHandler.Invoke(assemblyToLoad.assemblyToLoad, request, assemblyToLoad.options);

                if (!isSuccess)
                {
                    // HttpHandler indicated we can't process this
                    continue;
                }
            }

            var response = await client.SendAsync(request);

            var contentBytes = await response.Content.ReadAsStreamAsync();
            var assembly = AssemblyLoadContext.Default.LoadFromStream(contentBytes);

            loadedAssemblies.Add(assembly);
        }

        return loadedAssemblies.ToArray();
    }

    private string GetDllLocationFromOptions((string assemblyToLoad, BlazyAssemblyOptions options) assemblyToLoad)
    {
        var options = assemblyToLoad.options;

        var dllLocation = options == null ? // If options is null, 
            $"{_navigationManager.BaseUri}/_framework/" : // use default path
            options.AbsolutePath ?? // If AbsolutePath isn't null, use that.
            (options.RelativePath != null ? $"{_navigationManager.BaseUri}{assemblyToLoad.options.RelativePath}" : // If RelativePath isn't null, use base path + RelativePath
                $"{_navigationManager.BaseUri}/_framework/"); // Else just use default path

        return dllLocation;
    }
}