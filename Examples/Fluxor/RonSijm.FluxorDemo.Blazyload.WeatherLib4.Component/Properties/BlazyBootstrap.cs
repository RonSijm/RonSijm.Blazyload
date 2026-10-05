using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe;
using RonSijm.Syringe.DependencyInjection;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib4.Component.Properties;

// ReSharper disable once UnusedType.Global
public class BlazyBootstrap
{
    // ReSharper disable once UnusedMember.Global
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddFluxorLibrary(options =>
        {
            options.ScanAssemblies(typeof(BlazyBootstrap).Assembly);
        });

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}