using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Interface for loading assembly files from HTTP.
/// Responsible for downloading assembly bytes and loading them into the runtime.
/// </summary>
public interface IAssemblyFileLoader
{
    /// <summary>
    /// Loads assemblies from HTTP based on the provided options.
    /// </summary>
    /// <param name="assembliesToLoad">List of assembly names with their options.</param>
    /// <returns>Array of loaded assemblies.</returns>
    Task<Assembly[]> LoadAssembliesWithOptionsAsync(List<(string assemblyToLoad, AssemblyOptions options)> assembliesToLoad);

    /// <summary>
    /// Gets the DLL location URL based on the assembly options.
    /// </summary>
    /// <param name="options">The assembly options containing path configuration.</param>
    /// <returns>The URL where the DLL can be downloaded from.</returns>
    string GetDllLocationFromOptions(AssemblyOptions options);
}

