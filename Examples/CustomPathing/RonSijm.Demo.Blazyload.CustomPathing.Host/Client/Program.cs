using RonSijm.Demo.Blazyload.CustomPathing.Host.Auth;

namespace RonSijm.Demo.Blazyload.CustomPathing.Host.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        var awsBucket = "https://blazyload.s3.eu-central-1.amazonaws.com/";

        // If you do not use any custom classes, and put the bootstrapping where it's expected, you can just use this:
        // builder.UseBlazyload();

        var dummyAuthHandler = new DummyAuthHandler();
        var s3AuthHandler = new AWSAuthHandler(awsBucket);

        builder.UseBlazyload(options =>
        {
            options.UseSettingsForDll("RonSijm.Demo.Blazyload.WeatherLib1").UseCustomRelativePath("_framework/WeatherLib1/");
            options.UseSettingsForDll("RonSijm.Demo.Blazyload.WeatherLib2").UseCustomRelativePath("_framework/WeatherLib2/").UseHttpHandler(dummyAuthHandler.HandleAuth);

            options.UseSettingsForDll("RonSijm.Demo.Blazyload.WeatherLib3").UseOptions(x =>
            {
                x.AbsolutePath = awsBucket; // Note: absolute paths don't include the dll name, because these settings can be used for multiple dlls at once.
                x.DisableCascadeLoading = true;
                x.ClassPath = "RonSijm.Demo.Blazyload.WeatherLib3.CustomRegistrationClass";
                x.HttpHandler = s3AuthHandler.HandleAuth;
            });

            options.UseSettingsWhen(assembly => assembly.StartsWith("RonSijm.Demo.Blazyload.WeatherLib4")).UseCustomRelativePath("_framework/WeatherLib4/");
        });

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddSingleton(s3AuthHandler);

        await builder.Build().RunAsync();
    }
}