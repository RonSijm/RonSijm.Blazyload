namespace RonSijm.Blazyload;

/// <summary>
/// Interface for discovering preloaded assemblies from blazor.boot.json.
/// </summary>
public interface IPreloadedAssemblyDiscoverer
{
    /// <summary>
    /// Gets the set of assemblies that are preloaded by Blazor (from blazor.boot.json).
    /// </summary>
    /// <returns>A HashSet of preloaded assembly names.</returns>
    Task<HashSet<string>> GetPreloadedAssembliesAsync();
}

