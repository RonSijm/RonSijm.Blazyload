using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Demo.Blazyload.WeatherLib4.Component.Properties;

// ReSharper disable once UnusedType.Global
public class BlazyBootstrap
{
    // ReSharper disable once UnusedMember.Global
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWeatherResolver, WeatherResolver>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}