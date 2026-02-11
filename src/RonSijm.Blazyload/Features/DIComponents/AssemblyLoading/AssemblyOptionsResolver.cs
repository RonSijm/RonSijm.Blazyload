using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Default implementation of IAssemblyOptionsResolver.
/// Resolves assembly options from the SyringeServiceProvider.
/// </summary>
public class AssemblyOptionsResolver : IAssemblyOptionsResolver
{
    private readonly SyringeServiceProvider _serviceProvider;

    public AssemblyOptionsResolver(SyringeServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public List<(string assemblyToLoad, AssemblyOptions options)> ResolveOptions(IEnumerable<string> assemblyNames)
    {
        var result = new List<(string assemblyToLoad, AssemblyOptions options)>();
        var blazyOptions = _serviceProvider.Options.GetOptions<AssemblyLoadOptions>();

        foreach (var assemblyName in assemblyNames)
        {
            var options = blazyOptions?.GetOptions(assemblyName);
            result.Add((assemblyName, options));
        }

        return result;
    }
}

