using Fluxor;
using Microsoft.AspNetCore.Components;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Blazyload.HostLib.Redux;

public class IncreaseMainProjectCounterThroughEffectWithInjectionEffect : Effect<IncreaseMainProjectCounterThroughEffectWithInjectionAction>
{
    [Inject]
    public IState<IncreaseMainProjectCounterThroughEffectWithInjection> State { get; set; }

    public override async Task HandleAsync(IncreaseMainProjectCounterThroughEffectWithInjectionAction action, IDispatcher dispatcher)
    {
        var count = GetCount();
        LogState();

        dispatcher.Update<IncreaseMainProjectCounterThroughEffectWithInjection>(x => x.Count = count);
    }

    private void LogState()
    {
        if (State == null)
        {
            Console.WriteLine("State is null, should be injected.");
        }
        else if (State.Value == null)
        {
            Console.WriteLine("State.Value is null, should be injected.");
        }
    }

    private int GetCount()
    {
        int count;

        if (State.Value == null)
        {
            count = -1;
        }
        else
        {
            count = State.Value.Count + 1;
        }

        return count;
    }
}