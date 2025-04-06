using RonSijm.Syringe;

namespace RonSijm.Blazyload.Extensions;

public interface ILoadAfterExtension
{
    void AssembliesLoaded(List<Assembly> loadedAssemblies);
    void DescriptorsLoaded(List<ServiceDescriptor> loadedDescriptors);
    void SetReference(SyringeServiceProvider serviceProvider);
}

public abstract class BaseLoadAfterExtension : ILoadAfterExtension
{
    public SyringeServiceProvider ServiceProvider { get; protected set; }

    public virtual void AssembliesLoaded(List<Assembly> loadedAssemblies)
    {
        // Do nothing by default
    }

    public virtual void DescriptorsLoaded(List<ServiceDescriptor> loadedDescriptors)
    {
        // Do nothing by default
    }

    public virtual void SetReference(SyringeServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}