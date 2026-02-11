using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Extension methods for BlazyloadProviderOptions to configure assembly-specific settings.
/// </summary>
public static class BlazyloadProviderOptionsExtensions
{
    /// <summary>
    /// Configure settings for a specific DLL by exact name match.
    /// </summary>
    /// <param name="options">The BlazyloadProviderOptions instance.</param>
    /// <param name="assemblyPath">The exact assembly name to match.</param>
    /// <returns>A tuple containing the options and assembly settings for fluent configuration.</returns>
    public static SettingsForAssembly UseSettingsForDll(this BlazyloadProviderOptions options, string assemblyPath)
    {
        return UseSettingsWhen(options, x => x == assemblyPath);
    }

    /// <summary>
    /// Configure settings for assemblies matching a criteria function.
    /// </summary>
    /// <param name="options">The BlazyloadProviderOptions instance.</param>
    /// <param name="criteria">A function that returns true for assembly names that should use these settings.</param>
    /// <returns>A tuple containing the options and assembly settings for fluent configuration.</returns>
    public static SettingsForAssembly UseSettingsWhen(this BlazyloadProviderOptions options, Func<string, bool> criteria)
    {
        var assemblyLoadOptions = GetAssemblyLoadOptions(options);
        var assemblyOptions = new AssemblyOptions();
        assemblyLoadOptions.Add(criteria, assemblyOptions);

        return (options, assemblyOptions);
    }

    /// <summary>
    /// Gets or creates the AssemblyLoadOptions from the extended options.
    /// </summary>
    private static AssemblyLoadOptions GetAssemblyLoadOptions(BlazyloadProviderOptions options)
    {
        if (options.ExtendedOptions.FirstOrDefault(x => x is AssemblyLoadOptions) is AssemblyLoadOptions assemblyLoadOptions)
        {
            return assemblyLoadOptions;
        }

        assemblyLoadOptions = new AssemblyLoadOptions();
        options.ExtendedOptions.Add(assemblyLoadOptions);

        return assemblyLoadOptions;
    }
}

