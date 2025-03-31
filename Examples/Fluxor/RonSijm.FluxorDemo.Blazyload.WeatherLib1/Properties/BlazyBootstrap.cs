using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib1.Properties;

// ReSharper disable once UnusedType.Global
public class BlazyBootstrap : IBootstrapper
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddFluxor(options =>
        {
            options.WithLifetime(StoreLifetime.Singleton);
            options.ScanAssemblies(typeof(BlazyBootstrap).Assembly);
        });

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}