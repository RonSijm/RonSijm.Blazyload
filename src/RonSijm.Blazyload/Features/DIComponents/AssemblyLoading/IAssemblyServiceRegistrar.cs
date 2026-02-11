using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Interface for registering services from loaded assemblies.
/// </summary>
public interface IAssemblyServiceRegistrar
{
    /// <summary>
    /// Registers services from the loaded assemblies and returns the service descriptors.
    /// </summary>
    /// <param name="assemblies">The assemblies to register services from.</param>
    /// <returns>List of loaded service descriptors.</returns>
    Task<List<ServiceDescriptor>> RegisterServicesAsync(Assembly[] assemblies);

    /// <summary>
    /// Finalizes the loading process by rebuilding the service provider and notifying extensions.
    /// </summary>
    /// <param name="loadedAssemblies">All assemblies that were loaded.</param>
    /// <param name="loadedDescriptors">All service descriptors that were loaded.</param>
    void FinalizeLoading(List<Assembly> loadedAssemblies, List<ServiceDescriptor> loadedDescriptors);
}

