// ReSharper disable UnusedType.Global - Justification: Used by DI
// ReSharper disable UnusedMember.Global - Justification: Used by DI

using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Demo.Blazyload.WeatherLib2.Properties;

/// <summary>
/// In this example you can see how to bootstrap something without using a reference to the library.
/// Note that you must exactly implement "public IEnumerable&lt;ServiceDescriptor&gt; Bootstrap()"
/// </summary>
public class BlazyBootstrap
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWeatherResolver, WeatherResolver>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}