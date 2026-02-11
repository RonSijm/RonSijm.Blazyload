namespace RonSijm.Blazyload;

/// <summary>
/// Default implementation of IAssemblyStateTracker.
/// Tracks which assemblies have been loaded, preloaded, or are already in the runtime.
/// </summary>
public class AssemblyStateTracker : IAssemblyStateTracker
{
    /// <inheritdoc />
    public HashSet<string> LoadedAssemblyHashes { get; private set; } = [];

    /// <inheritdoc />
    public HashSet<string> PreloadedAssemblies { get; set; }

    /// <inheritdoc />
    public HashSet<string> RuntimeLoadedAssemblies { get; set; }

    /// <inheritdoc />
    public bool IsAlreadyLoadedInRuntime(string assemblyFileName)
    {
        if (RuntimeLoadedAssemblies == null)
        {
            return false;
        }

        // Remove .wasm extension to get the base name
        var baseName = assemblyFileName;
        if (baseName.EndsWith(".wasm", StringComparison.OrdinalIgnoreCase))
        {
            baseName = baseName[..^5]; // Remove ".wasm"
        }

        // Check if this exact name is loaded
        if (RuntimeLoadedAssemblies.Contains(baseName))
        {
            return true;
        }

        // Handle fingerprinted names like "Microsoft.Extensions.Options.moch3gvzt4"
        // The fingerprint is typically a 10-character alphanumeric string at the end
        var lastDotIndex = baseName.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            var potentialFingerprint = baseName[(lastDotIndex + 1)..];
            // Fingerprints are typically 8-12 alphanumeric characters
            if (potentialFingerprint.Length >= 8 && potentialFingerprint.Length <= 12 &&
                potentialFingerprint.All(c => char.IsLetterOrDigit(c)))
            {
                var nameWithoutFingerprint = baseName[..lastDotIndex];
                if (RuntimeLoadedAssemblies.Contains(nameWithoutFingerprint))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <inheritdoc />
    public void MarkAsLoaded(IEnumerable<string> assemblyNames)
    {
        LoadedAssemblyHashes = LoadedAssemblyHashes.Concat(assemblyNames).ToHashSet();
    }

    /// <inheritdoc />
    public List<string> FilterUnattemptedAssemblies(IEnumerable<string> assembliesToLoad)
    {
        return assembliesToLoad
            .Where(x => !LoadedAssemblyHashes.Contains(x))
            .Where(x => PreloadedAssemblies == null || !PreloadedAssemblies.Contains(x))
            .Where(x => !IsAlreadyLoadedInRuntime(x))
            .ToList();
    }

    /// <inheritdoc />
    public HashSet<string> GetRuntimeLoadedAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetName().Name)
            .Where(name => name != null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}

