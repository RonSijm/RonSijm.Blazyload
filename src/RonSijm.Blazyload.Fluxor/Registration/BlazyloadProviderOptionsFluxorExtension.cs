using RonSijm.Syringe;

namespace RonSijm.Blazyload;

public static class BlazyloadProviderOptionsFluxorExtension
{
    public static void UseFluxor(this BlazyloadProviderOptions providerOptions, Action<BlazyFluxorOptions> configure = null)
    {
        var fluxorOptions = new BlazyFluxorOptions(providerOptions.Services);
        configure?.Invoke(fluxorOptions);

        UseFluxor(providerOptions, fluxorOptions);
    }

    public static void UseFluxor(this BlazyloadProviderOptions providerOptions, BlazyFluxorOptions fluxorOptions)
    {
        fluxorOptions.ScanAssemblies<BlazyFluxorOptions>();
        SyringeServiceProviderOptionsFluxorExtension.UseFluxor(providerOptions, fluxorOptions);
    }
}