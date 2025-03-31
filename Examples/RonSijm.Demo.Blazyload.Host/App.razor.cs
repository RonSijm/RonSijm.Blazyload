using Microsoft.AspNetCore.Components.Routing;

namespace RonSijm.Demo.Blazyload.Host;

public partial class App
{

    private async Task OnNavigateAsync(NavigationContext args)
    {
        try
        {
            if (args.Path == "fetchdata1")
            {
                await AssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLib1.wasm");
            }
            else if (args.Path == "fetchdata2")
            {
                await AssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLib2.wasm");
            }
            else if (args.Path == "fetchdata3")
            {
                await AssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLib3.wasm");
            }
            else if (args.Path == "fetchdata4")
            {
                await AssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLib4.Page.wasm");
            }
            else if (args.Path == "fetchdataOptionalNull")
            {
                await AssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLibOptionalNull.wasm");
            }
            else if (args.Path == "fetchdataOptionalNotNull")
            {
                await AssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLibOptionalNotNull.wasm");
            }
        }
        catch (Exception)
        {
            // Do Nothing
        }
    }
}