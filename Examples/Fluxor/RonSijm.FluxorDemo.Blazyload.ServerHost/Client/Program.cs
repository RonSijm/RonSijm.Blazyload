using RonSijm.Blazyload;
using ElectronNET.API;
using RonSijm.FluxorDemo.Blazyload.HostLib.Wiring;

namespace RonSijm.FluxorDemo.Blazyload.ServerHost.Client;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.UseBlazyload(DependencyInjectionService.CreateOptions());

        builder.ConfigureServices(services =>
        {
            services.AddScoped(_ => new HttpClient());
        });

        return builder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseElectron(args);
            webBuilder.UseEnvironment("Development");
            webBuilder.UseStartup<Startup>();
        });
    }
}