using FluentAssertions;
using RonSijm.FluxorDemo.Blazyload.HostLib.Redux;
using RonSijm.FluxorDemo.Tests.Helpers;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Tests.Main;

public class MainProjectIndexTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TestIndexPage(bool rebuild)
    {
        var context = await CreateContext(rebuild);

        var counterState = context.ServiceProvider.GetState<MainProjectReducerMethodCounterState>();
        counterState.Value.Count.Should().Be(0);

        var count = counterState.Value.Count + 1;
        context.Dispatcher.Dispatch(new MainProjectReducerMethodCounterState { Count = count });

        counterState.Value.Count.Should().Be(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MainEffectPage(bool rebuild)
    {
        var context = await CreateContext(rebuild);

        var counterState = context.ServiceProvider.GetState<IncreaseMainProjectCounterThroughEffect_ParentState>();
        counterState.Value.CounterState.Should().BeNull();

        var count = counterState.Value?.CounterState?.Count + 1 ?? 1;
        context.Dispatcher.Update<IncreaseMainProjectCounterThroughEffect_CounterState>(state => state.Count = count);

        counterState.Value.CounterState.Count.Should().Be(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MainEffectInjectPage(bool rebuild)
    {
        var context = await CreateContext(rebuild);

        var counterState = context.ServiceProvider.GetState<IncreaseMainProjectCounterThroughEffectWithInjection>();
        counterState.Value.Count.Should().Be(0);

        context.Dispatcher.Dispatch(new IncreaseMainProjectCounterThroughEffectWithInjectionAction());

        counterState.Value.Count.Should().Be(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MainReduceIntoPage(bool rebuild)
    {
        var context = await CreateContext(rebuild);

        var counterState = context.ServiceProvider.GetState<MainProjectReduceIntoCounterState>();
        counterState.Value.CounterState.Should().BeNull();

        var count = counterState.Value?.CounterState?.Count + 1 ?? 1;
        context.Dispatcher.Dispatch(new MainProjectCounterState { Count = count });

        counterState.Value.CounterState.Count.Should().Be(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MainViewModelInterfaceStatePage(bool rebuild)
    {
        var context = await CreateContext(rebuild);

        var counterState = context.ServiceProvider.GetState<MainProjectCounterFromInterfaceState>();
        counterState.Value.Count.Should().Be(0);

        context.Dispatcher.Update<MainProjectCounterFromInterfaceState>(x => x.Count = 1);

        counterState.Value.Count.Should().Be(1);
    }

    private static async Task<TestContext> CreateContext(bool rebuild)
    {
        var context = await TestSetup.Setup();
        if (rebuild)
        {
            context.ServiceProvider.Build();
        }

        return context;
    }
}