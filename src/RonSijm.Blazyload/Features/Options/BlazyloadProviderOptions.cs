using RonSijm.Syringe;

namespace RonSijm.Blazyload;

public class BlazyloadProviderOptions : SyringeServiceProviderOptions
{
    public AssemblyLoaderOptions AssemblyLoaderOptions { get; } = new();
    internal AssemblyLoadConfiguration AssemblyLoadConfiguration { get; } = new();

    public BlazyloadProviderOptions LoadOnNavigation(string path, string assembly)
    {
        AssemblyLoadConfiguration.Add(path, assembly);
        return this;
    }
}