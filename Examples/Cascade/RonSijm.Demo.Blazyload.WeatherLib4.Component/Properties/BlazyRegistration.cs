using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Demo.Blazyload.WeatherLib4.Component.Properties;

public class BlazyBootstrap
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWeatherResolver, WeatherResolver>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}