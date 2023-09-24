using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace RonSijm.Blazyload.Features.Bootstapping;

// ReSharper disable once UnusedType.Global - Justification: Used by library consumers
public static class BlazyloadBootstapper
{
    // ReSharper disable once UnusedMember.Global - Justification: Used by library consumers
    public static void UseBlazyload(this WebAssemblyHostBuilder builder, Action<BlazyOptions> optionsConfig = null)
    {
        var options = new BlazyOptions();
        optionsConfig?.Invoke(options);

        // Not registering services here, but in BlazyServiceProviderFactory instead, so that a consumer (like bUnit) will always have the required services registered.
        builder.ConfigureContainer(new BlazyServiceProviderFactory(options, builder.Services));
    }
}