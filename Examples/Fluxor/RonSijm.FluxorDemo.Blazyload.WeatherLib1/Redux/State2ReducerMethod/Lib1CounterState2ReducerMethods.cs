using Fluxor;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.State2ReducerMethod;

public class Lib1CounterState2ReducerMethods
{
    [ReducerMethod]
    public static Lib1CounterState2 Lib1CounterState2FromAction(Lib1CounterState2 state, Lib1Counterstate2UpdateAction action)
    {
        if (state == null)
        {
            return new Lib1CounterState2
            {
                Count = 1
            };
        }

        return new Lib1CounterState2
        {
            Count = state.Count + 1
        };
    }
}