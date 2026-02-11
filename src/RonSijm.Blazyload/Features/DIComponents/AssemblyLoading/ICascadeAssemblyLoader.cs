using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Interface for handling cascade loading of referenced assemblies.
/// </summary>
public interface ICascadeAssemblyLoader
{
    /// <summary>
    /// Loads referenced assemblies for the given assemblies if cascade loading is enabled.
    /// </summary>
    /// <param name="assemblies">The assemblies to check for references.</param>
    /// <param name="loadAssemblyFunc">Function to load an assembly by name.</param>
    /// <returns>List of all cascade-loaded assemblies.</returns>
    Task<List<Assembly>> LoadReferencedAssembliesAsync(
        Assembly[] assemblies,
        Func<IEnumerable<string>, Task<List<Assembly>>> loadAssemblyFunc);
}

