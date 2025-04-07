using Fluxor.Blazor.Web.ReduxDevTools;
using RonSijm.Blazyload;

namespace RonSijm.FluxorDemo.Blazyload.HostLib.Wiring;

public static class DependencyInjectionService
{
    // Note: You don't *need* to declare your options like this, you can do it inside main.
    // I'm doing it like this so that my config accessible to bUnit
    public static Action<BlazyloadProviderOptions> CreateOptions(bool wireJavascriptServices = true)
    {
        var currAssembly = typeof(DependencyInjectionService).Assembly;

        var optionsFactory = new Action<BlazyloadProviderOptions>(options =>
        {
            options.LoadOnNavigation("fetchdata1", "RonSijm.FluxorDemo.Blazyload.WeatherLib1.wasm");
            options.LoadOnNavigation("Lib1ReduceInto", "RonSijm.FluxorDemo.Blazyload.WeatherLib1.wasm");
            options.LoadOnNavigation("Lib1ReduceIntoMethod", "RonSijm.FluxorDemo.Blazyload.WeatherLib1.wasm");
            options.LoadOnNavigation("Lib1ReduceIntoReducer", "RonSijm.FluxorDemo.Blazyload.WeatherLib1.wasm");

            options.LoadOnNavigation("fetchdata4", "RonSijm.FluxorDemo.Blazyload.WeatherLib4.Page.wasm");

            options.UseFluxor(fluxorOptions =>
            {
                fluxorOptions.ScanAssemblies(currAssembly);

#if DEBUG
                if (wireJavascriptServices)
                {
                    fluxorOptions.AddNativeExtension(x => x.UseReduxDevTools());
                }
#endif
            });
        });

        return optionsFactory;
    }
}