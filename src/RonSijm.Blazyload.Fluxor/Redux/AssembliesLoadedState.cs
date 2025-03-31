using System.Reflection;
using Fluxor;

namespace RonSijm.Blazyload;

[FeatureState]
public class AssembliesLoadedState
{
    public List<Assembly> LoadedAssemblies { get; set; }= [];
}