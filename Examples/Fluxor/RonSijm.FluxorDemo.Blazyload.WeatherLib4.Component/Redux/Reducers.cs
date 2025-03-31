using Fluxor;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib4.Component.Redux;

public class Reducers
{
    [ReducerMethod]
    public static WeatherLib4CounterState ReduceUpdateWeatherStateAction(WeatherLib4CounterState state, IncreaseWeatherLib4CounterAction action)
    {
        return new WeatherLib4CounterState { Count = state.Count + 1 };
    }
}