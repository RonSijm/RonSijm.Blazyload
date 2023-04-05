using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RonSijm.Blazyload;
using RonSijm.Blazyload.Options;

namespace RonSijm.Demo.Blazyload.Host.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // If you do not use any custom classes, and put the bootstrapping where it's expected, you can just use this:
        //builder.ConfigureContainer(new BlazyServiceProviderFactory());

        // Configuration if you don't want to use Properties.BlazyBootstrap
        builder.ConfigureContainer(new BlazyServiceProviderFactory(x =>
        {
            x.ResolveMode = ResolveMode.EnableOptional;
            x.UseCustomClass("RonSijm.Demo.Blazyload.WeatherLib3", "RonSijm.Demo.Blazyload.WeatherLib3.CustomRegistrationClass").DisableCascadeLoading();
        }));

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        await builder.Build().RunAsync();
    }
}