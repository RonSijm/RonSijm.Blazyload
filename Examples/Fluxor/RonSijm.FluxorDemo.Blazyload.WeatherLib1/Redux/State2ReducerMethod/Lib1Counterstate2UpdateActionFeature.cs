using Fluxor;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.State2ReducerMethod;

public class Lib1Counterstate2UpdateActionFeature : Feature<Lib1Counterstate2UpdateAction>
{
    public override string GetName()
    {
        return nameof(Lib1Counterstate2UpdateAction);
    }

    protected override Lib1Counterstate2UpdateAction GetInitialState()
    {
        return null;
    }

    public override void AddReducer(IReducer<Lib1Counterstate2UpdateAction> reducer)
    {
        base.AddReducer(reducer);
    }
}