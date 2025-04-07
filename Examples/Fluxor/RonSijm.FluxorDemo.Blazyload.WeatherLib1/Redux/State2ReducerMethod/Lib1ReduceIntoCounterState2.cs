using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.State2ReducerMethod;

public class Lib1ReduceIntoCounterState2
{
    [ReduceInto]
    public Lib1CounterState2 CounterState { get; set; }
}