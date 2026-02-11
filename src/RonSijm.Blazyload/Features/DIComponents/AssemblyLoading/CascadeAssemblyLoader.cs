using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Default implementation of ICascadeAssemblyLoader.
/// Handles loading of referenced assemblies recursively.
/// </summary>
public class CascadeAssemblyLoader : ICascadeAssemblyLoader
{
    private readonly AssemblyLoaderOptions _options;
    private readonly SyringeServiceProvider _serviceProvider;

    public CascadeAssemblyLoader(AssemblyLoaderOptions options, SyringeServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<List<Assembly>> LoadReferencedAssembliesAsync(
        Assembly[] assemblies,
        Func<IEnumerable<string>, Task<List<Assembly>>> loadAssemblyFunc)
    {
        var loadedAssemblies = new List<Assembly>();
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

            var cascadeResults = await loadAssemblyFunc(assemblyNames);
            loadedAssemblies.AddRange(cascadeResults);
        }

        return loadedAssemblies;
    }
}

