namespace RonSijm.Blazyload;

/// <summary>
/// Default implementation of IAssemblyLoadContext that uses AssemblyLoadContext.Default.
/// </summary>
public class DefaultAssemblyLoadContext : IAssemblyLoadContext
{
    public Assembly LoadFromStream(Stream assembly, Stream assemblySymbols = null)
    {
        return AssemblyLoadContext.Default.LoadFromStream(assembly, assemblySymbols);
    }
}

