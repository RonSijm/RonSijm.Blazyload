namespace RonSijm.Blazyload;

public class AssemblyLoadConfiguration
{
    private readonly Dictionary<string, string> AssembliesToLoadOnNavigation = new();

    public string GetAssembly(string path)
    {
        var fromDictionary = AssembliesToLoadOnNavigation.TryGetValue(path, out var value);
        return value;
    }

    public void Add(string path, string assembly)
    {
        AssembliesToLoadOnNavigation[path] = assembly;
    }

    public void Remove(string assembly)
    {
        var entriesForAssembly = AssembliesToLoadOnNavigation.Where(x => x.Value == assembly).ToList();

        foreach (var entry in entriesForAssembly)
        {
            AssembliesToLoadOnNavigation.Remove(entry.Key);
        }
    }
}