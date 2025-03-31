using RonSijm.Syringe;

namespace RonSijm.Blazyload;

public class BlazyloadProviderOptions : SyringeServiceProviderOptions
{
    public static bool DisableCascadeLoadingGlobally { get; set; }
    public static bool EnableLoggingForCascadeErrors { get; set; }

    public ResolveMode ResolveMode { get; set; }
    
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static BlazyloadProviderOptions FlexDefault = new();

    internal AssemblyLoadConfiguration AssemblyLoadConfiguration { get; } = new();

    public BlazyloadProviderOptions LoadOnNavigation(string path, string assembly)
    {
        AssemblyLoadConfiguration.Add(path, assembly);
        return this;
    }
}