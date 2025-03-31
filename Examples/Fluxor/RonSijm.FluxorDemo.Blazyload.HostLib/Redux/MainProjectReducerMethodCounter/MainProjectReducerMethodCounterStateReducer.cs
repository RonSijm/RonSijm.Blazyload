using Fluxor;

namespace RonSijm.FluxorDemo.Blazyload.HostLib.Redux;

public class MainProjectReducerMethodCounterStateReducer
{
    [ReducerMethod]
    public static MainProjectReducerMethodCounterState ReduceUpdateWeatherStateAction(MainProjectReducerMethodCounterState state, IncreaseMainProjectCounterAction action)
    {
        return new MainProjectReducerMethodCounterState { Count = state.Count + 1 };
    }
}