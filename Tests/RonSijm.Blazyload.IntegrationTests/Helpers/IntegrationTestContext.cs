using Microsoft.Extensions.Hosting;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.IntegrationTests.Helpers;

/// <summary>
/// Context object containing all the components needed for integration tests.
/// </summary>
public class IntegrationTestContext : IDisposable, IAsyncDisposable
{
    public IHostBuilder HostBuilder { get; set; } = null!;
    public IHost Host { get; set; } = null!;
    public SyringeServiceProvider ServiceProvider { get; set; } = null!;
    public BlazyAssemblyLoader? AssemblyLoader { get; set; }
    public HttpClient? HttpClient { get; set; }

    public void Dispose()
    {
        HttpClient?.Dispose();
        Host?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        HttpClient?.Dispose();
        if (Host != null)
        {
            await Host.StopAsync();
            Host.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}

