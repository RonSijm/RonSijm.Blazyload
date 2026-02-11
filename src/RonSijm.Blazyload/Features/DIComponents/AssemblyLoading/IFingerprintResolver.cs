namespace RonSijm.Blazyload;

/// <summary>
/// Interface for resolving fingerprinted assembly names.
/// In .NET 10+, WASM assemblies are published with fingerprinted names (e.g., MyAssembly.abc123.wasm)
/// but are referenced by their virtual names (e.g., MyAssembly.wasm).
/// This service resolves the virtual name to the actual fingerprinted filename.
/// </summary>
public interface IFingerprintResolver
{
    /// <summary>
    /// Initializes the fingerprint resolver by loading the mapping from the Blazor runtime.
    /// This should be called once during application startup.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    Task InitializeAsync();

    /// <summary>
    /// Gets whether the resolver has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Resolves a virtual assembly name to its fingerprinted filename.
    /// </summary>
    /// <param name="virtualName">The virtual/logical assembly name (e.g., "MyAssembly.wasm")</param>
    /// <returns>The fingerprinted filename, or the original name if no mapping exists.</returns>
    string ResolveFingerprintedName(string virtualName);

    /// <summary>
    /// Gets whether fingerprinting is enabled (i.e., if any assemblies have fingerprinted names).
    /// </summary>
    bool IsFingerprintingEnabled { get; }
}
