namespace RonSijm.Blazyload;

public class AssemblyLoadMatchConfig(string assembly, Func<string, bool> criteria) : AssemblyLoadConfigBase(assembly)
{
    public Func<string, bool> Criteria { get; private set; } = criteria;
    public override bool IsMatch(string path)
    {
        return Criteria(path);
    }
}