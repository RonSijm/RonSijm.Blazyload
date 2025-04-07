using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe;
using RonSijm.Syringe.DependencyInjection;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Properties;

// ReSharper disable once UnusedType.Global
public class BlazyBootstrap : IBootstrapper
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new SyringeServiceCollection();

        serviceCollection.AddFluxorLibrary(options =>
        {
            options.ScanAssemblies<BlazyBootstrap>();
        });

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}