namespace RonSijm.Blazyload;

public class BlazyAssemblyLoader
{
    private readonly List<string> _attemptedLoadedAssemblies = new();

    private readonly LazyAssemblyLoader _lazyAssemblyLoader;
    private readonly BlazyServiceProvider _blazyServiceProvider;

    public BlazyAssemblyLoader(LazyAssemblyLoader lazyAssemblyLoader, BlazyServiceProvider blazyServiceProvider)
    {
        _blazyServiceProvider = blazyServiceProvider;
        _lazyAssemblyLoader = lazyAssemblyLoader;
    }

    public async Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad, bool isRecursive = false)
    {
        var loadedAssemblies = new List<Assembly>();

        try
        {
            var assembliesToLoadList = assembliesToLoad as List<string> ?? assembliesToLoad.ToList();
            _attemptedLoadedAssemblies.AddRange(assembliesToLoadList);

            var assemblies = (await _lazyAssemblyLoader.LoadAssembliesAsync(assembliesToLoadList)).ToArray();

            if (!assemblies.Any())
            {
                return loadedAssemblies;
            }

            loadedAssemblies.AddRange(assemblies);

            await _blazyServiceProvider.Register(assemblies);
            var references = _blazyServiceProvider.FindReferencedAssemblies(assemblies);

            var newAssembliesToLoad = references.Where(result => !_attemptedLoadedAssemblies.Contains(result)).ToList();

            // "Oh no! recursion! ಠ_ಠ" - Possibly implement trampoline pattern if this causes issues
            var cascadeResults = await LoadAssembliesAsync(newAssembliesToLoad, true);

            loadedAssemblies.AddRange(cascadeResults);
        }
        // TODO: When trying to lazy load libraries that are not marked for lazy-loading, I get an exception thrown at me
        // Should figure out if there's a class that I can call to ask which dlls are registered for lazy loading...
        catch (Exception e)
        {
            if (!isRecursive || BlazyOptions.EnableLoggingForCascadeErrors)
            {
                Console.WriteLine(e);
            }
        }

        // Only at the end of the recursive loop we rebuild the service provider.
        if (!isRecursive)
        {
            _blazyServiceProvider.CreateServiceProvider();
        }

        return loadedAssemblies;
    }
}