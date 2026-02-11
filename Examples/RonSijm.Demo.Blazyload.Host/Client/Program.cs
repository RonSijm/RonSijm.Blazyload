using RonSijm.Blazyload;

namespace RonSijm.Demo.Blazyload.Host.Client;

public class Program
{
    #region CodeExample-BasicSetup
    // Note: You don't *need* to declare your options like this, you can do it inside main.
    // I'm doing it like this so that my config is accessible to bUnit tests
    public static Action<BlazyloadProviderOptions> BlazyConfig { get; } = options =>
    {
        // Register navigation-based lazy loading - assemblies load automatically when navigating to these routes
        options.LoadOnNavigation("fetchdata1", "RonSijm.Demo.Blazyload.WeatherLib1.wasm");
        options.LoadOnNavigation("fetchdata2", "RonSijm.Demo.Blazyload.WeatherLib2.wasm");
        options.LoadOnNavigation("fetchdata3", "RonSijm.Demo.Blazyload.WeatherLib3.wasm");
        options.LoadOnNavigation("fetchdata4", "RonSijm.Demo.Blazyload.WeatherLib4.Page.wasm");
        options.LoadOnNavigation("fetchdataOptionalNull", "RonSijm.Demo.Blazyload.WeatherLibOptionalNull.wasm");
        options.LoadOnNavigation("fetchdataOptionalNotNull", "RonSijm.Demo.Blazyload.WeatherLibOptionalNotNull.wasm");

        #region CodeExample-CustomClassConfig
        // Use a custom bootstrap class instead of the default BlazyBootstrap
        options.UseSettingsForDll("RonSijm.Demo.Blazyload.WeatherLib3")
            .UseCustomClass("RonSijm.Demo.Blazyload.WeatherLib3.CustomRegistrationClass")
            .DisableCascadeLoading();
        #endregion
    };
    #endregion

    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Configure Blazyload with navigation-based lazy loading
        builder.UseBlazyload(BlazyConfig);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        await builder.Build().RunAsync();
    }
}