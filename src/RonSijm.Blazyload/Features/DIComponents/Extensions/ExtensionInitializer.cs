using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Default implementation of IExtensionInitializer.
/// Initializes load-after extensions with the service provider reference.
/// </summary>
public class ExtensionInitializer : IExtensionInitializer
{
    private readonly AssemblyLoaderOptions _options;
    private readonly SyringeServiceProvider _serviceProvider;

    public ExtensionInitializer(AssemblyLoaderOptions options, SyringeServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void Initialize()
    {
        foreach (var extension in _options.AfterLoadAssembliesExtensions)
        {
            extension.SetReference(_serviceProvider);
        }
    }
}

