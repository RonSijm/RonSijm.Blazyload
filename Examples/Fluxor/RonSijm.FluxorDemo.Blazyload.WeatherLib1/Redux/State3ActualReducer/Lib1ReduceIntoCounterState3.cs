using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.State3ActualReducer;

public class Lib1ReduceIntoCounterState3
{
    [ReduceInto]
    public Lib1CounterState3 CounterState { get; set; }
}