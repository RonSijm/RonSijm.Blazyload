using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace RonSijm.Blazyload;
public class LazyLoadFromNavigationContextEffect : IEffect
{
    [Inject] public AssemblyLoadConfiguration LoadConfiguration { get; set; }

    public async Task HandleAsync(object action, IDispatcher dispatcher)
    {
        var context = (NavigationContext)action;
        var assembly = LoadConfiguration.GetAssembly(context.Path);

        // Remove the assembly from the list so it doesn't get attempted to be loaded again.
        LoadConfiguration.Remove(assembly);
        dispatcher.Dispatch(new LoadAssembly(assembly));
    }

    public bool ShouldReactToAction(object action)
    {
        return action is NavigationContext context && LoadConfiguration?.GetAssembly(context.Path) != null;
    }
}