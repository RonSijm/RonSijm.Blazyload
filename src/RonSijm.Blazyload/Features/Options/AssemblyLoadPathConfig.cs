namespace RonSijm.Blazyload;

public class AssemblyLoadPathConfig(string assembly, string path) : AssemblyLoadConfigBase(assembly)
{
    public override bool IsMatch(string sourcePath)
    {
        return path == sourcePath;
    }
}