using Fluxor;

namespace RonSijm.FluxorDemo.Blazyload.HostLib.Redux;

public class MainProjectReducerMethodCounterState
{
    public int Count { get; set; }
}

public class MainProjectReducerMethodCounterStateFeature : Feature<MainProjectReducerMethodCounterState>
{
    public override string GetName()
    {
        return nameof(MainProjectReducerMethodCounterState);
    }

    protected override MainProjectReducerMethodCounterState GetInitialState()
    {
        return new MainProjectReducerMethodCounterState();
    }

    public override void AddReducer(IReducer<MainProjectReducerMethodCounterState> reducer)
    {
        base.AddReducer(reducer);
    }
}