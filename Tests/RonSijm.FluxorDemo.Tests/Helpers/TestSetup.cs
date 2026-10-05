using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.JSInterop;
using RonSijm.Blazyload;
using RonSijm.FluxorDemo.Blazyload.HostLib.Wiring;
using RonSijm.Syringe;

namespace RonSijm.FluxorDemo.Tests.Helpers;

public static class TestSetup
{
    public static async Task<FluxorTestContext> Setup()
    {
        var result = new FluxorTestContext
        {
            DefaultBuilder = Host.CreateDefaultBuilder()
        };

        var navigation = new TestNavigationManager();
        navigation.Configure();
        result.DefaultBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<NavigationManager>(navigation);
            services.AddSingleton<IJSRuntime, TestJSRuntime>();
        });
        result.DefaultBuilder.UseBlazyload(DependencyInjectionService.CreateOptions(false));

        result.Host = result.DefaultBuilder.Build();
        result.ServiceProvider = result.Host.Services as SyringeServiceProvider;
        result.Store = result.ServiceProvider.GetService<IStore>();
        await result.Store.InitializeAsync();

        result.Dispatcher = result.ServiceProvider.GetService<IDispatcher>();

        return result;
    }

    private sealed class TestNavigationManager() : NavigationManager
    {
        public void Configure() => Initialize("https://example.test/", "https://example.test/");
        protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException("Navigation is not available in the Fluxor unit-test host.");
    }

    private sealed class TestJSRuntime() : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args) => throw new NotSupportedException("Browser JavaScript is not available in the Fluxor unit-test host.");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object[] args) => throw new NotSupportedException("Browser JavaScript is not available in the Fluxor unit-test host.");
    }
}

public class FluxorTestContext
{
    public IHostBuilder DefaultBuilder { get; set; }
    public IHost Host { get; set; }
    public SyringeServiceProvider ServiceProvider { get; set; }
    public IStore Store { get; set; }
    public IDispatcher Dispatcher { get; set; }
}
