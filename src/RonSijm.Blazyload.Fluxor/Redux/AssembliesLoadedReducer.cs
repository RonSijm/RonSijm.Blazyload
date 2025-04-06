using Fluxor;
using RonSijm.Syringe;
using System.Reflection;

namespace RonSijm.Blazyload;

public class AssembliesLoadedReducer : Reducer<AssembliesLoadedState, List<Assembly>>
{
    public override AssembliesLoadedState Reduce(AssembliesLoadedState state, List<Assembly> action)
    {
        var newState = state.CopyProperties();
        newState.LoadedAssemblies.AddRange(action);

        return newState;
    }
}