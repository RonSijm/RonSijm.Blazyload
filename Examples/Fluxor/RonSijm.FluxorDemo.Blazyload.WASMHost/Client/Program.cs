using RonSijm.Blazyload;
using RonSijm.FluxorDemo.Blazyload.HostLib;
using RonSijm.FluxorDemo.Blazyload.HostLib.Wiring;

namespace RonSijm.FluxorDemo.Blazyload.WASMHost.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.UseBlazyload(DependencyInjectionService.CreateOptions());

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        await builder.Build().RunAsync();
    }
}