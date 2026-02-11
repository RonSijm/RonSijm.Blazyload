namespace RonSijm.Blazyload;

/// <summary>
/// Default implementation of INavigationAssemblyResolver.
/// Resolves assemblies based on navigation paths using AssemblyLoadConfiguration.
/// </summary>
public class NavigationAssemblyResolver : INavigationAssemblyResolver
{
    private readonly AssemblyLoadConfiguration _configuration;

    public NavigationAssemblyResolver(AssemblyLoadConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public string GetAssemblyForPath(string path)
    {
        return _configuration.GetAssembly(path);
    }
}

