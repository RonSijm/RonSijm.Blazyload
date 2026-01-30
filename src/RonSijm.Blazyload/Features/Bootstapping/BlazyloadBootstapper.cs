using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace RonSijm.Blazyload;

// ReSharper disable once UnusedType.Global - Justification: Used by library consumers
public static class BlazyloadBootstapper
{
    internal static BlazyServiceProviderFactory CreateSyringeFactory(Action<BlazyloadProviderOptions> optionsConfig = null)
    {
        var options = new BlazyloadProviderOptions();
        optionsConfig?.Invoke(options);
        return new BlazyServiceProviderFactory(options);
    }

    internal static BlazyServiceProviderFactory CreateBlazyloadFactory(Action<BlazyloadProviderOptions> optionsConfig = null)
    {
        var blazyOptions = new BlazyloadProviderOptions();

        if (optionsConfig != null)
        {
            optionsConfig(blazyOptions);
        }

        blazyOptions.AddOption(blazyOptions);
        return new BlazyServiceProviderFactory(blazyOptions);
    }


    // ReSharper disable once UnusedMember.Global - Justification: Used by library consumers
    public static void UseSyringe(this WebAssemblyHostBuilder builder, Action<BlazyloadProviderOptions> optionsConfig = null)
    {
        builder.ConfigureContainer(CreateSyringeFactory(optionsConfig));
    }

    public static void UseBlazyload(this WebAssemblyHostBuilder builder, Action<BlazyloadProviderOptions> options = null)
    {
        builder.ConfigureContainer(CreateBlazyloadFactory(options));
    }
}