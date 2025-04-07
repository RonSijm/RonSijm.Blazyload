using FluentAssertions;
using RonSijm.FluxorDemo.Blazyload.WeatherLib4.Component.Redux;
using RonSijm.FluxorDemo.Tests.Helpers;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Tests.Lib4;

public class Lib4FetchDataTest
{
    [Fact]
    public async Task TestLib4FetchDataPage()
    {
        var context = await TestSetup.Setup();

        var lib4 = typeof(WeatherLib4CounterState).Assembly;

        await context.ServiceProvider.RegisterAssemblies(lib4);
        context.ServiceProvider.Build();

        var lib4CounterState = context.ServiceProvider.GetState<WeatherLib4CounterState>();
        lib4CounterState.Value.Count.Should().Be(0);

        var lib4Count = lib4CounterState.Value.Count + 1;
        context.Dispatcher.Dispatch(new IncreaseWeatherLib4CounterAction());

        lib4CounterState.Value.Count.Should().Be(1);
    }
}