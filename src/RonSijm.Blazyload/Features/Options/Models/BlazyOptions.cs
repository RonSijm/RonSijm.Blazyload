﻿namespace RonSijm.Blazyload.Features.Options.Models;

public class BlazyOptions
{
    public static bool DisableCascadeLoadingGlobally { get; set; }
    public static bool EnableLoggingForCascadeErrors { get; set; }

    public ResolveMode ResolveMode { get; set; }

    private List<(Func<string, bool> Criteria, BlazyAssemblyOptions Options)> _assemblyMapping;

    public BlazyAssemblyOptions GetOptions(string assemblyName)
    {
        if (_assemblyMapping == null)
        {
            return null;
        }

        if (assemblyName.EndsWith(".dll"))
        {
            assemblyName = assemblyName.Replace(".dll", string.Empty);
        }

        foreach (var mapping in _assemblyMapping)
        {
            if (mapping.Criteria(assemblyName))
            {
                return mapping.Options;
            }
        }

        return null;
    }

    public SettingsForAssembly UseSettingsForDll(string assemblyPath)
    {
        return UseSettingsWhen(x => x == assemblyPath);
    }

    public SettingsForAssembly UseSettingsWhen(Func<string, bool> criteria)
    {
        _assemblyMapping ??= new List<(Func<string, bool> Criteria, BlazyAssemblyOptions Options)>();

        var assemblyOptions = new BlazyAssemblyOptions();
        _assemblyMapping.Add((criteria, assemblyOptions));

        return (this, assemblyOptions);
    }
}