namespace RonSijm.Blazyload;

public class BlazyOptions
{
    public static bool DisableCascadeLoadingGlobally { get; set; }
    public static bool EnableLoggingForCascadeErrors { get; set; }

    private Dictionary<string, BlazyAssemblyOptions> _assemblyMapping;

    public BlazyAssemblyOptions GetOptions(string assemblyName)
    {
        if (_assemblyMapping == null)
        {
            return null;
        }

        _assemblyMapping.TryGetValue(assemblyName, out var result);
        return result;
    }

    public (BlazyOptions Blazy, BlazyAssemblyOptions assembly) UseCustomClass(string assemblyPath, string classPath)
    {
        _assemblyMapping ??= new Dictionary<string, BlazyAssemblyOptions>();

        _assemblyMapping.TryGetValue(assemblyPath, out var assemblyOptions);

        if (assemblyOptions == null)
        {
            assemblyOptions = new BlazyAssemblyOptions();
            _assemblyMapping.Add(assemblyPath, assemblyOptions);
        }

        assemblyOptions.ClassPath = classPath;

        return (this, assemblyOptions);
    }

    public (BlazyOptions Blazy, BlazyAssemblyOptions assembly) DisableCascadeLoading(string assemblyPath)
    {
        _assemblyMapping ??= new Dictionary<string, BlazyAssemblyOptions>();

        _assemblyMapping.TryGetValue(assemblyPath, out var assemblyOptions);

        if (assemblyOptions == null)
        {
            assemblyOptions = new BlazyAssemblyOptions();
            _assemblyMapping.Add(assemblyPath, assemblyOptions);
        }

        assemblyOptions.DisableCascadeLoading = true;

        return (this, assemblyOptions);
    }
}