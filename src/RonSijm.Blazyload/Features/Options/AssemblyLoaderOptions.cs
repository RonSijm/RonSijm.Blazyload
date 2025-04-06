using RonSijm.Blazyload.Extensions;

namespace RonSijm.Blazyload;
public class AssemblyLoaderOptions
{
    public bool DisableCascadeLoading { get; set; }
    public bool EnableLoggingForCascadeErrors { get; set; }

    public List<ILoadAfterExtension> AfterLoadAssembliesExtensions { get; set; } = [];
    public bool EnableLogging { get; set; }
}