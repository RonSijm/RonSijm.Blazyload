using Fluxor;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Blazyload.HostLib.Redux;

public class EffectDispatchesSelfEffect : Effect<EffectDispatchesSelfState>
{
    public override async Task HandleAsync(EffectDispatchesSelfState action, IDispatcher dispatcher)
    {
        if (action == null)
        {
            return;
        }

        if (action.Count % 3 != 0)
        {
        }
        else
        {
            dispatcher.Update<EffectDispatchesSelfState>(x => x.Count = action.Count + 1);
        }
    }
}