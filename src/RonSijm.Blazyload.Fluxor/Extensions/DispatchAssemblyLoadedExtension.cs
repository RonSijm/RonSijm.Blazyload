using System.Reflection;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.Extensions;
public class DispatchAssemblyLoadedExtension : BaseLoadAfterExtension
{
    private readonly Lazy<IDispatcher> _dispatcherFactory;
    private IDispatcher Dispatcher => _dispatcherFactory.Value;

    public DispatchAssemblyLoadedExtension()
    {
        _dispatcherFactory = new Lazy<IDispatcher>(() => ServiceProvider.GetRequiredService<IDispatcher>());
    }



    public override void AssembliesLoaded(List<Assembly> loadedAssemblies)
    {
        foreach (var loadedAssembly in loadedAssemblies)
        {
            Dispatcher.Dispatch(new AssemblyLoaded(loadedAssembly));
        }
    }
}