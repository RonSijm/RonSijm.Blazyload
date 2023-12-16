using Microsoft.Extensions.DependencyInjection;
using RonSijm.Blazyload.Library.Features.Consumer;

namespace RonSijm.Demo.Blazyload.WeatherLib1.Properties;

// ReSharper disable once UnusedType.Global
public class BlazyBootstrap : IBlazyBootstrap
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWeatherResolver, WeatherResolver>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}