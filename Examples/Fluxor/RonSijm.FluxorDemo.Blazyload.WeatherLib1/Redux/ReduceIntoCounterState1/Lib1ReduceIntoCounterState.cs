using Fluxor;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.ReduceIntoCounterState1;

[FeatureState]
public class Lib1ReduceIntoCounterState
{
    [ReduceInto]
    public Lib1CounterState CounterState { get; set; }
}