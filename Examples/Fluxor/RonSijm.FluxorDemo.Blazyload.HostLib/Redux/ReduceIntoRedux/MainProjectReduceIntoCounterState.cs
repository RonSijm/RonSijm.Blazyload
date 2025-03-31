using Fluxor;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Blazyload.HostLib.Redux;

[FeatureState]
public class MainProjectReduceIntoCounterState
{
    [ReduceInto]
    public MainProjectCounterState CounterState { get; set; }
}