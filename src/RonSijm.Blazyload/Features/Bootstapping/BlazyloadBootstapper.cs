namespace RonSijm.Blazyload.Features.Bootstapping;

public static class BlazyloadBootstapper
{
    public static void UseBlazyload(this WebAssemblyHostBuilder builder, Action<BlazyOptions> optionsConfig = null)
    {
        var options = new BlazyOptions();
        optionsConfig?.Invoke(options);

        // Not registering services here, but in BlazyServiceProviderFactory instead, so that a consumer (like bUnit) will always have the required services registered.
        builder.ConfigureContainer(new BlazyServiceProviderFactory(options, builder.Services));
    }
}