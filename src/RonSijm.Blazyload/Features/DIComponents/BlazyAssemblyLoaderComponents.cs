using Microsoft.JSInterop;
using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Container for all components used by BlazyAssemblyLoader.
/// Simplifies dependency injection and testing.
/// </summary>
public class BlazyAssemblyLoaderComponents
{
    public IAssemblyStateTracker StateTracker { get; init; }
    public IPreloadedAssemblyDiscoverer PreloadedDiscoverer { get; init; }
    public IAssemblyFileLoader FileLoader { get; init; }
    public ICascadeAssemblyLoader CascadeLoader { get; init; }
    public IAssemblyServiceRegistrar ServiceRegistrar { get; init; }
    public IAssemblyOptionsResolver OptionsResolver { get; init; }
    public INavigationAssemblyResolver NavigationResolver { get; init; }
    public IExtensionInitializer ExtensionInitializer { get; init; }
    public IFingerprintResolver FingerprintResolver { get; init; }
    public IBlazyLogger Logger { get; init; }
    public AssemblyLoaderOptions Options { get; init; }

    /// <summary>
    /// Creates default components for production use.
    /// </summary>
    public static BlazyAssemblyLoaderComponents CreateDefault(
        AssemblyLoaderOptions options,
        SyringeServiceProvider serviceProvider,
        string baseUrl,
        HttpClient httpClient,
        IAssemblyLoadContext assemblyLoadContext,
        IBlazyLogger logger,
        IDebuggerDetector debuggerDetector,
        AssemblyLoadConfiguration assemblyLoadConfiguration,
        IJSRuntime jsRuntime = null)
    {
        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += '/';
        }

        // Create fingerprint resolver - uses JS interop to read the lazy assembly mapping
        // directly from the Blazor runtime's in-memory configuration (getDotnetRuntime(0).getConfig().resources.lazyAssembly)
        // This is more efficient than fetching and parsing dotnet.js via HTTP
        var fingerprintResolver = jsRuntime != null
            ? new FingerprintResolver(jsRuntime, logger, options)
            : null;

        return new BlazyAssemblyLoaderComponents
        {
            Options = options,
            Logger = logger,
            StateTracker = new AssemblyStateTracker(),
            PreloadedDiscoverer = new PreloadedAssemblyDiscoverer(jsRuntime, httpClient, baseUrl, options, logger),
            FileLoader = new AssemblyFileLoader(httpClient, baseUrl, options, assemblyLoadContext, logger, debuggerDetector, fingerprintResolver),
            CascadeLoader = new CascadeAssemblyLoader(options, serviceProvider),
            ServiceRegistrar = new AssemblyServiceRegistrar(options, serviceProvider, logger),
            OptionsResolver = new AssemblyOptionsResolver(serviceProvider),
            NavigationResolver = new NavigationAssemblyResolver(assemblyLoadConfiguration),
            ExtensionInitializer = new ExtensionInitializer(options, serviceProvider),
            FingerprintResolver = fingerprintResolver
        };
    }
}
