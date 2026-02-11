// ReSharper disable global EventNeverSubscribedTo.Global - Justification: Used by library consumers
// ReSharper disable global UnusedMember.Global - Justification: Used by library consumers

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using RonSijm.Syringe;

namespace RonSijm.Blazyload;

public class BlazyAssemblyLoader : IBlazyAssemblyLoader
{
    private readonly BlazyAssemblyLoaderComponents _components;

    public BlazyAssemblyLoader(AssemblyLoaderOptions options, SyringeServiceProvider serviceProvider, NavigationManager navigationManager, AssemblyLoadConfiguration assemblyLoadConfiguration, IJSRuntime jsRuntime)
        : this(BlazyAssemblyLoaderComponents.CreateDefault(options, serviceProvider, navigationManager.BaseUri, new HttpClient(), new DefaultAssemblyLoadContext(), new ConsoleLogger(), new DefaultDebuggerDetector(), assemblyLoadConfiguration, jsRuntime))
    {
    }

    internal BlazyAssemblyLoader(AssemblyLoaderOptions options, SyringeServiceProvider serviceProvider, string baseUrl)
        : this(BlazyAssemblyLoaderComponents.CreateDefault(options, serviceProvider, baseUrl, new HttpClient(), new DefaultAssemblyLoadContext(), new ConsoleLogger(), new DefaultDebuggerDetector(), new AssemblyLoadConfiguration()))
    {
    }

    internal BlazyAssemblyLoader(AssemblyLoaderOptions options, SyringeServiceProvider serviceProvider, string baseUrl, HttpClient httpClient, IAssemblyLoadContext assemblyLoadContext, IBlazyLogger logger, IDebuggerDetector debuggerDetector, AssemblyLoadConfiguration assemblyLoadConfiguration, IJSRuntime jsRuntime = null)
        : this(BlazyAssemblyLoaderComponents.CreateDefault(options, serviceProvider, baseUrl, httpClient, assemblyLoadContext, logger, debuggerDetector, assemblyLoadConfiguration, jsRuntime))
    {
    }

    internal BlazyAssemblyLoader(BlazyAssemblyLoaderComponents components)
    {
        _components = components;
        _components.ExtensionInitializer.Initialize();
    }

    public List<Assembly> AdditionalAssemblies { get; } = [];

    public Task<List<Assembly>> LoadAssemblyAsync(string assemblyToLoad) => LoadAssembliesAsync([assemblyToLoad], false);

    public Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad) => LoadAssembliesAsync(assembliesToLoad, false);

    public Task OnNavigateAsync(NavigationContext args) => HandleNavigationInternal(args.Path);

    internal async Task HandleNavigationInternal(string path)
    {
        try
        {
            var assembly = _components.NavigationResolver.GetAssemblyForPath(path);
            if (assembly != null)
            {
                await LoadAssemblyAsync(assembly);
            }
        }
        catch
        {
            // Silently ignore navigation errors
        }
    }

    internal async Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad, bool isRecursive, List<ServiceDescriptor> loadedDescriptors = null)
    {
        await InitializeStateTrackerAsync();
        loadedDescriptors ??= [];

        var unattemptedAssemblies = _components.StateTracker.FilterUnattemptedAssemblies(assembliesToLoad);
        var loadedAssemblies = new List<Assembly>();

        try
        {
            var assemblyWithOptions = _components.OptionsResolver.ResolveOptions(unattemptedAssemblies);
            var assemblies = await _components.FileLoader.LoadAssembliesWithOptionsAsync(assemblyWithOptions);

            if (assemblies.Length == 0)
            {
                return loadedAssemblies;
            }

            loadedAssemblies.AddRange(assemblies);

            var cascadeResults = await _components.CascadeLoader.LoadReferencedAssembliesAsync(
                assemblies,
                names => LoadAssembliesAsync(names, true, loadedDescriptors));
            loadedAssemblies.AddRange(cascadeResults);

            var serviceDescriptors = await _components.ServiceRegistrar.RegisterServicesAsync(assemblies);
            loadedDescriptors.AddRange(serviceDescriptors);
        }
        catch (Exception e)
        {
            if (!isRecursive || _components.Options.EnableLoggingForCascadeErrors)
            {
                _components.Logger.WriteLine(e);
            }
        }

        _components.StateTracker.MarkAsLoaded(unattemptedAssemblies);

        if (isRecursive)
        {
            return loadedAssemblies;
        }

        _components.ServiceRegistrar.FinalizeLoading(loadedAssemblies, loadedDescriptors);
        AdditionalAssemblies.AddRange(loadedAssemblies);

        return loadedAssemblies;
    }

    private async Task InitializeStateTrackerAsync()
    {
        _components.StateTracker.PreloadedAssemblies ??= await _components.PreloadedDiscoverer.GetPreloadedAssembliesAsync();
        _components.StateTracker.RuntimeLoadedAssemblies ??= _components.StateTracker.GetRuntimeLoadedAssemblies();
    }

    /// <summary>
    /// Gets the DLL location URL based on the assembly options.
    /// Exposed for backward compatibility with tests.
    /// </summary>
    internal string GetDllLocationFromOptions(AssemblyOptions options) => _components.FileLoader.GetDllLocationFromOptions(options);
}
