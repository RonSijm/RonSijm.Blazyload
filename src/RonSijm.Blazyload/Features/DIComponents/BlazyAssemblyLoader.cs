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
        var assembly = _components.NavigationResolver.GetAssemblyForPath(path);
        if (assembly != null)
        {
            await LoadAssemblyAsync(assembly);
        }
    }

    internal async Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad, bool isRecursive, List<ServiceDescriptor> loadedDescriptors = null)
    {
        var requestedAssemblies = assembliesToLoad.ToList();
        try
        {
            await InitializeStateTrackerAsync();
            loadedDescriptors ??= [];
            var unattemptedAssemblies = _components.StateTracker.FilterUnattemptedAssemblies(requestedAssemblies);
            var loadedAssemblies = new List<Assembly>();
            var assemblyWithOptions = _components.OptionsResolver.ResolveOptions(unattemptedAssemblies);
            var assemblies = await _components.FileLoader.LoadAssembliesWithOptionsAsync(assemblyWithOptions);

            if (assemblies.Length != unattemptedAssemblies.Count)
            {
                throw new FileNotFoundException($"Could not load all requested assemblies: {string.Join(", ", unattemptedAssemblies)}.");
            }

            if (assemblies.Length == 0)
            {
                return loadedAssemblies;
            }

            loadedAssemblies.AddRange(assemblies);
            var cascadeResults = await _components.CascadeLoader.LoadReferencedAssembliesAsync(assemblies, names => LoadAssembliesAsync(names, true, loadedDescriptors));
            loadedAssemblies.AddRange(cascadeResults);
            var serviceDescriptors = await _components.ServiceRegistrar.RegisterServicesAsync(assemblies);
            loadedDescriptors.AddRange(serviceDescriptors);

            if (!isRecursive)
            {
                _components.ServiceRegistrar.FinalizeLoading(loadedAssemblies, loadedDescriptors);
                AdditionalAssemblies.AddRange(loadedAssemblies);
            }

            _components.StateTracker.MarkAsLoaded(unattemptedAssemblies);
            return loadedAssemblies;
        }
        catch (Exception exception) when (!isRecursive)
        {
            if (exception is BlazyAssemblyLoadException)
            {
                _components.Logger.WriteLine(exception);
                throw;
            }

            var loadException = new BlazyAssemblyLoadException(string.Join(", ", requestedAssemblies), exception);
            _components.Logger.WriteLine(loadException);
            throw loadException;
        }
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
