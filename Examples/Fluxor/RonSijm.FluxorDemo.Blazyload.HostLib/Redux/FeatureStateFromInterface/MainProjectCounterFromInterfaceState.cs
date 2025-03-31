using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Blazyload.HostLib.Redux;

public class MainProjectCounterFromInterfaceState : IFeatureState
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Count { get; set; }
}