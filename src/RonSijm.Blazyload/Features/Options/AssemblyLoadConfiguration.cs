using System.Diagnostics;

namespace RonSijm.Blazyload;

public class AssemblyLoadConfiguration
{
    private readonly List<AssemblyLoadConfigBase> _assemblyLoadModels = new();

    public string GetAssembly(string path)
    {
        Debug.WriteLine($"Trying to get assembly for path '{path}'");

        foreach (var assemblyLoadModel in _assemblyLoadModels)
        {
            if (assemblyLoadModel.IsMatch(path))
            {
                return assemblyLoadModel.Assembly;
            }
        }

        Debug.WriteLine($"Could not get assembly for path '{path}'");
        return null;
    }

    public void Add(string path, string assembly)
    {
        _assemblyLoadModels.Add(new AssemblyLoadPathConfig(assembly, path));
    }

    public void Add(Func<string, bool> criteria, string assembly)
    {
        _assemblyLoadModels.Add(new AssemblyLoadMatchConfig(assembly, criteria));
    }

    public void Remove(string assembly)
    {
        _assemblyLoadModels.RemoveAll(x => x.Assembly == assembly);
    }
}