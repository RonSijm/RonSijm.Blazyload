using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace RonSijm.Blazyload;

// ReSharper disable once UnusedType.Global - Justification: Used by library consumers
public static class BlazyloadBootstapper
{
    // ReSharper disable once UnusedMember.Global - Justification: Used by library consumers
    public static void UseSyringe(this WebAssemblyHostBuilder builder, Action<BlazyloadProviderOptions> optionsConfig = null)
    {
        var options = new BlazyloadProviderOptions();
        optionsConfig?.Invoke(options);

        // Not registering services here, but in BlazyServiceProviderFactory instead, so that a consumer (like bUnit) will always have the required services registered.
        builder.ConfigureContainer(new BlazyServiceProviderFactory(options));
    }

    public static void UseBlazyload(this WebAssemblyHostBuilder builder, Action<BlazyloadProviderOptions> options = null)
    {
        var blazyOptions = new BlazyloadProviderOptions();

        if (options != null)
        {
            options(blazyOptions);
        }

        blazyOptions.AddOption(blazyOptions);

        // Not registering services here, but in BlazyServiceProviderFactory instead, so that a consumer (like bUnit) will always have the required services registered.
        builder.ConfigureContainer(new BlazyServiceProviderFactory(blazyOptions));
    }
}