using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Interface for initializing load-after extensions.
/// </summary>
public interface IExtensionInitializer
{
    /// <summary>
    /// Initializes all extensions with the service provider reference.
    /// </summary>
    void Initialize();
}

