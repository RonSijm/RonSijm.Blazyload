using Fluxor;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.State3ActualReducer;

public class Lib1ReduceIntoCounterState3Feature : Feature<Lib1ReduceIntoCounterState3>
{
    public override string GetName()
    {
        return nameof(Lib1ReduceIntoCounterState3);
    }

    protected override Lib1ReduceIntoCounterState3 GetInitialState()
    {
        return null;
    }

    public override void AddReducer(IReducer<Lib1ReduceIntoCounterState3> reducer)
    {
        base.AddReducer(reducer);
    }
}