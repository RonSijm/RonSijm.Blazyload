using Fluxor;
using RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.State2ReducerMethod;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.ReduceIntoCounterState1;

public class Lib1ReduceIntoCounterState2Feature : Feature<Lib1ReduceIntoCounterState2>
{
    public override string GetName()
    {
        return nameof(Lib1ReduceIntoCounterState2);
    }

    protected override Lib1ReduceIntoCounterState2 GetInitialState()
    {
        return null;
    }

    public override void AddReducer(IReducer<Lib1ReduceIntoCounterState2> reducer)
    {
        base.AddReducer(reducer);
    }
}