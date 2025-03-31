using Fluxor;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Blazyload.HostLib.Redux;

[FeatureState]
public class IncreaseMainProjectCounterThroughEffect_ParentState
{
    [ReduceInto]
    public IncreaseMainProjectCounterThroughEffect_CounterState CounterState { get; set; }
}