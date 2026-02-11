using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Default implementation of IAssemblyServiceRegistrar.
/// Handles service registration from loaded assemblies and notifies extensions.
/// </summary>
public class AssemblyServiceRegistrar : IAssemblyServiceRegistrar
{
    private readonly AssemblyLoaderOptions _options;
    private readonly SyringeServiceProvider _serviceProvider;
    private readonly IBlazyLogger _logger;

    public AssemblyServiceRegistrar(AssemblyLoaderOptions options, SyringeServiceProvider serviceProvider, IBlazyLogger logger)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<ServiceDescriptor>> RegisterServicesAsync(Assembly[] assemblies)
    {
        var optionsModel = _serviceProvider.Options.GetOptions<AssemblyLoadOptions>();
        var serviceDescriptors = LibraryLoader.GetServiceDescriptorsOfAssemblies(optionsModel, _serviceProvider, assemblies);
        var loadedServiceDescriptors = await _serviceProvider.LoadServiceDescriptors(serviceDescriptors);
        return loadedServiceDescriptors;
    }

    /// <inheritdoc />
    public void FinalizeLoading(List<Assembly> loadedAssemblies, List<ServiceDescriptor> loadedDescriptors)
    {
        // Rebuild the service provider
        _serviceProvider.Build();

        // Notify extensions
        _options.AfterLoadAssembliesExtensions.ForEach(x => x.AssembliesLoaded(loadedAssemblies));
        _options.AfterLoadAssembliesExtensions.ForEach(x => x.DescriptorsLoaded(loadedDescriptors));

        // Log loaded assemblies if logging is enabled
        if (_options.EnableLogging)
        {
            foreach (var loadedAssembly in loadedAssemblies)
            {
                _logger.WriteLine($"Loaded Assembly: {loadedAssembly.FullName}");
            }
        }
    }
}

