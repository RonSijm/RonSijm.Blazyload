using Microsoft.Extensions.DependencyInjection;
namespace RonSijm.Demo.Blazyload.WeatherLib3;

// ReSharper disable once UnusedType.Global
public class CustomRegistrationClass
#if DEBUG
    : RonSijm.Blazyload.Library.Features.Consumer.IBlazyBootstrap
#endif
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWeatherResolver, WeatherResolver>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}