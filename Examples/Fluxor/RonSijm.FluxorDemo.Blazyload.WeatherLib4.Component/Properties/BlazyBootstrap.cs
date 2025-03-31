using Fluxor;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.FluxorDemo.Blazyload.WeatherLib4.Component.Properties;

// ReSharper disable once UnusedType.Global
public class BlazyBootstrap
{
    // ReSharper disable once UnusedMember.Global
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