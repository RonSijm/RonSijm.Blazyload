using FluentAssertions;
using RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.FetchData;
using RonSijm.FluxorDemo.Tests.Helpers;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Tests.Lib1;

public class Lib1FetchDataTest
{
    [Fact]
    public async Task TestLib1FetchDataPage()
    {
        var context = await TestSetup.Setup();

        var lib1Assembly = typeof(WeatherLib1CounterState).Assembly;
        await context.ServiceProvider.RegisterAssemblies(lib1Assembly);
        context.ServiceProvider.Build();

        var lib1CounterState = context.ServiceProvider.GetState<WeatherLib1CounterState>();
        lib1CounterState.Value.Count.Should().Be(0);

        var lib1Count = lib1CounterState.Value.Count + 1;
        context.Dispatcher.Dispatch(new IncreaseWeatherLib1CounterAction());

        lib1CounterState.Value.Count.Should().Be(1);
    }
}