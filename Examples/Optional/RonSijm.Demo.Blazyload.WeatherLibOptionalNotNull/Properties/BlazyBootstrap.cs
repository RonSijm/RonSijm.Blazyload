using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe;

namespace RonSijm.Demo.Blazyload.WeatherLibOptionalNotNull.Properties;

// ReSharper disable once UnusedType.Global
public class BlazyBootstrap : IBootstrapper
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWeatherResolver, WeatherResolver>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}