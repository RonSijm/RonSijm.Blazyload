using FluentAssertions;
using RonSijm.FluxorDemo.Blazyload.HostLib.Redux;
using RonSijm.FluxorDemo.Blazyload.WeatherLib1.Redux.FetchData;
using RonSijm.FluxorDemo.Blazyload.WeatherLib4.Component.Redux;
using RonSijm.FluxorDemo.Tests.Helpers;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Tests.Full;

public class FullProjectIntegrationTest
{
    [Fact]
    public async Task TestAllTheThings()
    {
        var context = await TestSetup.Setup();

        var indexCounterState = context.ServiceProvider.GetState<MainProjectReducerMethodCounterState>();
        indexCounterState.Value.Count.Should().Be(0);

        context.Dispatcher.Dispatch(new MainProjectReducerMethodCounterState { Count = indexCounterState.Value.Count + 1 });

        indexCounterState.Value.Count.Should().Be(1);

        var throughEffectCounterState = context.ServiceProvider.GetState<IncreaseMainProjectCounterThroughEffect_ParentState>();
        throughEffectCounterState.Value.CounterState.Should().BeNull();

        var throughEffectCount = throughEffectCounterState.Value?.CounterState?.Count + 1 ?? 1;
        context.Dispatcher.Update<IncreaseMainProjectCounterThroughEffect_CounterState>(state => state.Count = throughEffectCount);

        throughEffectCounterState.Value.CounterState.Count.Should().Be(1);

        var injectCounterState = context.ServiceProvider.GetState<IncreaseMainProjectCounterThroughEffectWithInjection>();
        injectCounterState.Value.Count.Should().Be(0);

        context.Dispatcher.Dispatch(new IncreaseMainProjectCounterThroughEffectWithInjectionAction());

        injectCounterState.Value.Count.Should().Be(1);

        var reduceCounterState = context.ServiceProvider.GetState<MainProjectReduceIntoCounterState>();
        reduceCounterState.Value.CounterState.Should().BeNull();

        var reduceCount = reduceCounterState.Value?.CounterState?.Count + 1 ?? 1;
        context.Dispatcher.Dispatch(new MainProjectCounterState { Count = reduceCount });

        reduceCounterState.Value.CounterState.Count.Should().Be(1);

        var interfaceCounterState = context.ServiceProvider.GetState<MainProjectCounterFromInterfaceState>();
        interfaceCounterState.Value.Count.Should().Be(0);

        context.Dispatcher.Update<MainProjectCounterFromInterfaceState>(x => x.Count = 1);

        interfaceCounterState.Value.Count.Should().Be(1);

        var lib1Assembly = typeof(WeatherLib1CounterState).Assembly;
        await context.ServiceProvider.RegisterAssemblies(lib1Assembly);
        context.ServiceProvider.Build();

        var lib1CounterState = context.ServiceProvider.GetState<WeatherLib1CounterState>();
        lib1CounterState.Value.Count.Should().Be(0);

        var lib1Count = lib1CounterState.Value.Count + 1;
        context.Dispatcher.Dispatch(new IncreaseWeatherLib1CounterAction());

        lib1CounterState.Value.Count.Should().Be(1);

        var lib4 = typeof(WeatherLib4CounterState).Assembly;

        await context.ServiceProvider.RegisterAssemblies(lib4);
        context.ServiceProvider.Build();

        var lib4CounterState = context.ServiceProvider.GetState<WeatherLib4CounterState>();
        lib4CounterState.Value.Count.Should().Be(0);

        var lib4Count = lib4CounterState.Value.Count + 1;
        context.Dispatcher.Dispatch(new IncreaseWeatherLib4CounterAction());

        lib4CounterState.Value.Count.Should().Be(1);

        // Loaded and dispatched Everything

        // Re-evaluate all the states
        indexCounterState.Value.Count.Should().Be(1);
        throughEffectCounterState.Value.CounterState.Count.Should().Be(1);
        injectCounterState.Value.Count.Should().Be(1);
        reduceCounterState.Value.CounterState.Count.Should().Be(1);
        interfaceCounterState.Value.Count.Should().Be(1);
        lib1CounterState.Value.Count.Should().Be(1);
        lib4CounterState.Value.Count.Should().Be(1);

        context.Dispatcher.Dispatch(new MainProjectReducerMethodCounterState { Count = indexCounterState.Value.Count + 1 });
        indexCounterState.Value.Count.Should().Be(2);
    }
}