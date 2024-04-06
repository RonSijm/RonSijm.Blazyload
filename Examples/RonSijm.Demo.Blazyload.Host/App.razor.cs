using Microsoft.AspNetCore.Components.Routing;
using System.Reflection;

namespace RonSijm.Demo.Blazyload.Host;

public partial class App
{
    private readonly List<Assembly> _lazyLoadedAssemblies = new();

    private async Task OnNavigateAsync(NavigationContext args)
    {
        try
        {
            if (args.Path == "fetchdata1")
            {
                var assemblies = await BlazyAssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLib1.wasm");
                _lazyLoadedAssemblies.AddRange(assemblies);
            }
            else if (args.Path == "fetchdata2")
            {
                var assemblies = await BlazyAssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLib2.wasm");
                _lazyLoadedAssemblies.AddRange(assemblies);
            }
            else if (args.Path == "fetchdata3")
            {
                var assemblies = await BlazyAssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLib3.wasm");
                _lazyLoadedAssemblies.AddRange(assemblies);
            }
            else if (args.Path == "fetchdata4")
            {
                var assemblies = await BlazyAssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLib4.Page.wasm");
                _lazyLoadedAssemblies.AddRange(assemblies);
            }
            else if (args.Path == "fetchdataOptionalNull")
            {
                var assemblies = await BlazyAssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLibOptionalNull.wasm");
                _lazyLoadedAssemblies.AddRange(assemblies);
            }
            else if (args.Path == "fetchdataOptionalNotNull")
            {
                var assemblies = await BlazyAssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLibOptionalNotNull.wasm");
                _lazyLoadedAssemblies.AddRange(assemblies);
            }
        }
        catch (Exception)
        {
            // Do Nothing
        }
    }
}