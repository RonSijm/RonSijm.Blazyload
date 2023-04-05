using RonSijm.Blazyload.Options;

namespace RonSijm.Blazyload;

public static class BlazyOptionsExtensions
{
    public static (BlazyOptions Blazy, BlazyAssemblyOptions assembly) UseCustomClass(this (BlazyOptions Blazy, BlazyAssemblyOptions assembly) options, string classPath)
    {
        options.assembly.ClassPath = classPath;
        return options;
    }

    public static (BlazyOptions Blazy, BlazyAssemblyOptions assembly) DisableCascadeLoading(this (BlazyOptions Blazy, BlazyAssemblyOptions assembly) options)
    {
        options.assembly.DisableCascadeLoading = true;
        return options;
    }
}