using Fluxor;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.State3ActualReducer;

public class Lib1Counterstate3UpdateActionReducer : Reducer<Lib1CounterState3, Lib1Counterstate3UpdateAction>
{
    public override Lib1CounterState3 Reduce(Lib1CounterState3 state, Lib1Counterstate3UpdateAction action)
    {
        var stateCount = state.Count + 1;
        return new Lib1CounterState3
        {
            Count = stateCount
        };
    }
}