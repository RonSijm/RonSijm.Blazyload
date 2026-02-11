namespace RonSijm.Blazyload;

/// <summary>
/// Interface for tracking the state of loaded assemblies.
/// Responsible for tracking which assemblies have been loaded, preloaded, or are already in the runtime.
/// </summary>
public interface IAssemblyStateTracker
{
    /// <summary>
    /// Gets the set of assemblies that have been loaded by this loader.
    /// </summary>
    HashSet<string> LoadedAssemblyHashes { get; }

    /// <summary>
    /// Gets or sets the set of assemblies that were preloaded (from blazor.boot.json).
    /// </summary>
    HashSet<string> PreloadedAssemblies { get; set; }

    /// <summary>
    /// Gets or sets the set of assemblies that are already loaded in the .NET runtime.
    /// </summary>
    HashSet<string> RuntimeLoadedAssemblies { get; set; }

    /// <summary>
    /// Checks if an assembly (by .wasm filename) is already loaded in the runtime.
    /// Handles both simple names and fingerprinted names.
    /// </summary>
    /// <param name="assemblyFileName">The assembly filename (e.g., "MyAssembly.wasm" or "MyAssembly.abc123.wasm")</param>
    /// <returns>True if the assembly is already loaded in the runtime.</returns>
    bool IsAlreadyLoadedInRuntime(string assemblyFileName);

    /// <summary>
    /// Marks assemblies as loaded by adding them to the loaded assembly hashes.
    /// </summary>
    /// <param name="assemblyNames">The assembly names to mark as loaded.</param>
    void MarkAsLoaded(IEnumerable<string> assemblyNames);

    /// <summary>
    /// Filters out assemblies that have already been loaded, preloaded, or are in the runtime.
    /// </summary>
    /// <param name="assembliesToLoad">The assemblies to filter.</param>
    /// <returns>Only the assemblies that haven't been loaded yet.</returns>
    List<string> FilterUnattemptedAssemblies(IEnumerable<string> assembliesToLoad);

    /// <summary>
    /// Gets the set of assemblies that are already loaded in the .NET runtime.
    /// </summary>
    /// <returns>A HashSet of assembly names loaded in the runtime.</returns>
    HashSet<string> GetRuntimeLoadedAssemblies();
}

