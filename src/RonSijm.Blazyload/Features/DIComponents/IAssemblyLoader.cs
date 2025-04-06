using Microsoft.AspNetCore.Components.Routing;

namespace RonSijm.Blazyload;

public interface IAssemblyLoader
{
    Task<List<Assembly>> LoadAssemblyAsync(string assemblyToLoad);
    Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad);
    public List<Assembly> LoadedAssemblies { get; }
    public Task HandleNavigation(NavigationContext args);
}