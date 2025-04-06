using Fluxor;
using Microsoft.AspNetCore.Components;

namespace RonSijm.Blazyload;

public class LoadAssemblyForPathEffect : Effect<LoadAssemblyForPath>
{
    [Inject]
    public AssemblyLoadConfiguration AssemblyLoadConfiguration { get; set; }

    public override Task HandleAsync(LoadAssemblyForPath action, IDispatcher dispatcher)
    {
        var path = AssemblyLoadConfiguration.GetAssembly(action.Path);
        if (!string.IsNullOrWhiteSpace(path))
        {
            dispatcher.Dispatch(new LoadAssembly(path));
        }

        return Task.CompletedTask;
    }
}