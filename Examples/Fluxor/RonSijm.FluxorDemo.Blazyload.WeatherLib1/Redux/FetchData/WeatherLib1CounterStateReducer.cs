using Fluxor;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.FetchData;

public class WeatherLib1CounterStateReducer
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