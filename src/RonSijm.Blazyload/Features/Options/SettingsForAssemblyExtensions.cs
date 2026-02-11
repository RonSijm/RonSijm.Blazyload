using RonSijm.Syringe;

namespace RonSijm.Blazyload;

/// <summary>
/// Extension methods for the SettingsForAssembly tuple to configure assembly-specific options.
/// </summary>
public static class SettingsForAssemblyExtensions
{
    /// <summary>
    /// Sets a custom class path for bootstrapping the assembly.
    /// </summary>
    public static SettingsForAssembly UseCustomClass(this SettingsForAssembly options, string classPath)
    {
        options.assembly.ClassPath = classPath;
        return options;
    }

    /// <summary>
    /// Sets a custom relative path to load the assembly from.
    /// </summary>
    public static SettingsForAssembly UseCustomRelativePath(this SettingsForAssembly options, string relativePath)
    {
        options.assembly.RelativePath = relativePath;
        return options;
    }

    /// <summary>
    /// Configures assembly options using an action.
    /// </summary>
    public static SettingsForAssembly UseOptions(this SettingsForAssembly options, Action<AssemblyOptions> optionsConfig)
    {
        optionsConfig(options.assembly);
        return options;
    }

    /// <summary>
    /// Disables cascade loading for this specific assembly.
    /// </summary>
    public static SettingsForAssembly DisableCascadeLoading(this SettingsForAssembly options)
    {
        options.assembly.DisableCascadeLoading = true;
        return options;
    }

    /// <summary>
    /// Configures a custom HTTP handler for loading this assembly.
    /// </summary>
    public static SettingsForAssembly UseHttpHandler(this SettingsForAssembly options, Func<string, HttpRequestMessage, AssemblyOptions, bool> handler)
    {
        options.assembly.HttpHandler = handler;
        return options;
    }
}

