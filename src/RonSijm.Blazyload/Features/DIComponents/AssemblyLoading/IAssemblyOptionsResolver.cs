using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Interface for resolving assembly options from the service provider.
/// </summary>
public interface IAssemblyOptionsResolver
{
    /// <summary>
    /// Resolves assembly options for the given assembly names.
    /// </summary>
    /// <param name="assemblyNames">The assembly names to resolve options for.</param>
    /// <returns>List of assembly names with their resolved options.</returns>
    List<(string assemblyToLoad, AssemblyOptions options)> ResolveOptions(IEnumerable<string> assemblyNames);
}

