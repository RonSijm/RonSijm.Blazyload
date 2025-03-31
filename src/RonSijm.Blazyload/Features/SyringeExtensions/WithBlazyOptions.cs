using RonSijm.Syringe;

namespace RonSijm.Blazyload;

public static class WithBlazyOptions
{
    public static void UseBlazyload(this SyringeServiceProviderOptions providerOptions, Action<BlazyloadProviderOptions> options = null)
    {
        var blazyloadProviderOptions = new BlazyloadProviderOptions();

        options?.Invoke(blazyloadProviderOptions);

        providerOptions.AddOption(blazyloadProviderOptions);
    }
}