using Fluxor;
using Microsoft.AspNetCore.Components;

namespace RonSijm.Blazyload;

public class LoadAssemblyEffect : Effect<LoadAssembly>
{
    [Inject]
    public IAssemblyLoader AssemblyLoader { get; set; }

    public override async Task HandleAsync(LoadAssembly action, IDispatcher dispatcher)
    {
        var assemblies = await AssemblyLoader.LoadAssemblyAsync(action.Assembly);

        if (assemblies.Any())
        {
            dispatcher.Dispatch(assemblies);
        }
    }
}