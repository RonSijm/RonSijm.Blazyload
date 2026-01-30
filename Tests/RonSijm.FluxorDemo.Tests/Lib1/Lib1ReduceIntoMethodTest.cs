using AwesomeAssertions;
using RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.FetchData;
using RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.State2ReducerMethod;
using RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.State3ActualReducer;
using RonSijm.FluxorDemo.Tests.Helpers;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Tests.Lib1;

public class Lib1ReduceIntoMethodTest
{
    [Fact]
    public async Task ReduceIntoMethodTest()
    {
        var context = await TestSetup.Setup();

        var lib1Assembly = typeof(WeatherLib1CounterState).Assembly;
        await context.ServiceProvider.RegisterAssemblies(lib1Assembly);
        context.ServiceProvider.Build();

        var lib1CounterState = context.ServiceProvider.GetState<Lib1ReduceIntoCounterState2>();
        lib1CounterState.Value.Should().BeNull();

        context.Dispatcher.Dispatch(new Lib1Counterstate2UpdateAction());
        lib1CounterState.Value.CounterState.Count.Should().Be(1);

        context.Dispatcher.Dispatch(new Lib1Counterstate2UpdateAction());
        lib1CounterState.Value.CounterState.Count.Should().Be(2);
    }

    [Fact]
    public async Task ReduceIntReducerTest()
    {
        var context = await TestSetup.Setup();

        var lib1Assembly = typeof(WeatherLib1CounterState).Assembly;
        await context.ServiceProvider.RegisterAssemblies(lib1Assembly);
        context.ServiceProvider.Build();

        var lib1CounterState = context.ServiceProvider.GetState<Lib1ReduceIntoCounterState3>();
        lib1CounterState.Value.Should().BeNull();

        context.Dispatcher.Dispatch(new Lib1Counterstate3UpdateAction());
        lib1CounterState.Value.CounterState.Count.Should().Be(1);

        context.Dispatcher.Dispatch(new Lib1Counterstate3UpdateAction());
        lib1CounterState.Value.CounterState.Count.Should().Be(2);
    }
}
