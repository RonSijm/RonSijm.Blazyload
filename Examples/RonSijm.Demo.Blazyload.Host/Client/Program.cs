namespace RonSijm.Demo.Blazyload.Host.Client;

public class Program
{
    // Note: You don't *need* to declare your options like this, you can do it inside main.
    // I'm doing it like this so that my config accessible to bUnit
    public static Action<BlazyOptions> BlazyConfig { get; } = x =>
    {
        x.ResolveMode = ResolveMode.EnableOptional;
        x.UseSettingsForDll("RonSijm.Demo.Blazyload.WeatherLib3").UseCustomClass("RonSijm.Demo.Blazyload.WeatherLib3.CustomRegistrationClass").DisableCascadeLoading();
    };

    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // If you do not use any custom classes, and put the bootstrapping where it's expected, you can just use this:
        //builder.ConfigureContainer(new BlazyServiceProviderFactory());

        // Configuration if you don't want to use Properties.BlazyBootstrap
        builder.UseBlazyload(BlazyConfig);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        await builder.Build().RunAsync();
    }
}