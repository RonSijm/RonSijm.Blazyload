namespace RonSijm.Blazyload;

public abstract class AssemblyLoadConfigBase(string assembly)
{
    public string Assembly { get; private set; } = assembly;
    public abstract bool IsMatch(string path);
}