using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        result.DefaultBuilder.UseBlazyload(DependencyInjectionService.CreateOptions(false));

        result.Host = result.DefaultBuilder.Build();
        result.ServiceProvider = result.Host.Services as SyringeServiceProvider;
        result.Store = result.ServiceProvider.GetService<IStore>();
        await result.Store.InitializeAsync();

        result.Dispatcher = result.ServiceProvider.GetService<IDispatcher>();

        return result;
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

