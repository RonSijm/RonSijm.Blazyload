namespace RonSijm.Blazyload.Features.DIComponents;

public interface IAssemblyLoader
{
    event Action<List<Assembly>> OnAssembliesLoaded;
    Task<List<Assembly>> LoadAssemblyAsync(string assemblyToLoad);
    Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad);
}