using RonSijm.Syringe;

namespace RonSijm.Blazyload;

public class BlazyloadProviderOptions : SyringeServiceProviderOptions
{
    public Syringe.AssemblyLoaderOptions AssemblyLoaderOptions { get; } = new();
    internal AssemblyLoadConfiguration AssemblyLoadConfiguration { get; } = new();

    public BlazyloadProviderOptions LoadOnNavigation(string path, string assembly)
    {
        AssemblyLoadConfiguration.Add(path, assembly);
        return this;
    }

    public BlazyloadProviderOptions LoadOnNavigation(Func<string, bool> criteria, string assembly)
    {
        AssemblyLoadConfiguration.Add(criteria, assembly);
        return this;
    }
}