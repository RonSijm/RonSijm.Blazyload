using RonSijm.Blazyload;
using RonSijm.Demo.Blazyload.CustomPathing.Host.Auth;

namespace RonSijm.Demo.Blazyload.CustomPathing.Host.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        var dummyAuthHandler = new DummyAuthHandler();
        var s3AuthHandler = new AWSAuthHandler();

        builder.UseBlazyload(options =>
        {
            // Register navigation-based lazy loading
            options.LoadOnNavigation("fetchdata1", "RonSijm.Demo.Blazyload.WeatherLib1.wasm");
            options.LoadOnNavigation("fetchdata2", "RonSijm.Demo.Blazyload.WeatherLib2.wasm");
            options.LoadOnNavigation("fetchdata3", "RonSijm.Demo.Blazyload.WeatherLib3.wasm");
            options.LoadOnNavigation("fetchdata4", "RonSijm.Demo.Blazyload.WeatherLib4.Page.wasm");

            #region CodeExample-CustomPaths
            // Load assemblies from custom relative paths
            options.UseSettingsForDll("RonSijm.Demo.Blazyload.WeatherLib1").UseCustomRelativePath("_framework/WeatherLib1/");
            options.UseSettingsForDll("RonSijm.Demo.Blazyload.WeatherLib2").UseCustomRelativePath("_framework/WeatherLib2/").UseHttpHandler(dummyAuthHandler.HandleAuth);
            #endregion

            #region CodeExample-AuthenticatedLoading
            // Load assemblies with authentication/custom HTTP handling
            options.UseSettingsForDll("RonSijm.Demo.Blazyload.WeatherLib3").UseOptions(x =>
            {
                // Note: absolute paths don't include the dll name, because these settings can be used for multiple dlls at once.
                // Note2: I'm not setting any custom url here, because the s3AuthHandler overrules it.
                //x.AbsolutePath = awsBucket;
                x.DisableCascadeLoading = true;
                x.ClassPath = "RonSijm.Demo.Blazyload.WeatherLib3.CustomRegistrationClass";
                x.HttpHandler = s3AuthHandler.HandleAuth;
            });
            #endregion

            #region CodeExample-CriteriaBasedPaths
            // Use lambda expressions to match multiple assemblies by criteria
            options.UseSettingsWhen(assembly => assembly.StartsWith("RonSijm.Demo.Blazyload.WeatherLib4", StringComparison.InvariantCultureIgnoreCase)).UseCustomRelativePath("_framework/WeatherLib4/");
            #endregion
        });

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddSingleton(s3AuthHandler);

        await builder.Build().RunAsync();
    }
}