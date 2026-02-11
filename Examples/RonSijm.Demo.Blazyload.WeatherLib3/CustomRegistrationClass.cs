using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Demo.Blazyload.WeatherLib3;

#region CodeExample-CustomRegistrationClass
// ReSharper disable once UnusedType.Global
public class CustomRegistrationClass
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWeatherResolver, WeatherResolver>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}
#endregion