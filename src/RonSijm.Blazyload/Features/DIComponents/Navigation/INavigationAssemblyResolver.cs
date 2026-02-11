namespace RonSijm.Blazyload;

/// <summary>
/// Interface for resolving assemblies based on navigation paths.
/// </summary>
public interface INavigationAssemblyResolver
{
    /// <summary>
    /// Gets the assembly name to load for the given navigation path.
    /// </summary>
    /// <param name="path">The navigation path.</param>
    /// <returns>The assembly name to load, or null if no assembly is configured for this path.</returns>
    string GetAssemblyForPath(string path);
}

