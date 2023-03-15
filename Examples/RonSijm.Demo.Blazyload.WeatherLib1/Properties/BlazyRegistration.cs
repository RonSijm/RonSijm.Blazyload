using Microsoft.Extensions.DependencyInjection;
using RonSijm.Blazyload;

namespace RonSijm.Demo.Blazyload.WeatherLib1.Properties;

public class BlazyBootstrap : IBlazyBootstrap
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWeatherResolver, WeatherResolver>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}