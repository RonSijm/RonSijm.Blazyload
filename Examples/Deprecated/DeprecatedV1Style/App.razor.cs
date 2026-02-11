using Microsoft.AspNetCore.Components.Routing;

namespace RonSijm.Demo.Blazyload.Deprecated;

#region CodeExample-DeprecatedV1Style
/// <summary>
/// DEPRECATED: This is the old V1 style of manually handling navigation.
/// In V2, use LoadOnNavigation in Program.cs instead.
/// </summary>
public partial class App
{
    private async Task OnNavigateAsync(NavigationContext args)
    {
        try
        {
            // Old V1 style: manually check paths and load assemblies
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
        }
        catch (Exception)
        {
            // Do Nothing
        }
    }
}
#endregion

