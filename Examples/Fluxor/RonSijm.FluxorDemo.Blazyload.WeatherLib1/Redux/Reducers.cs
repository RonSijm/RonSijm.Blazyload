using Fluxor;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux;

public class Reducers
{
    [ReducerMethod]
    public static WeatherLib1CounterState ReduceUpdateWeatherStateAction(WeatherLib1CounterState state, IncreaseWeatherLib1CounterAction action)
    {
        var stateCount = state.Count + 1;
        return new WeatherLib1CounterState
        {
            Count = stateCount
        };
    }
}