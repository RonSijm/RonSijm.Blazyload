using Microsoft.AspNetCore.Components.Routing;

namespace RonSijm.Blazyload;

/// <summary>
/// Blazor-specific assembly loader interface that extends the generic Syringe IAssemblyLoader
/// with navigation-based assembly loading capabilities.
/// </summary>
public interface IBlazyAssemblyLoader : RonSijm.Syringe.IAssemblyLoader
{
    /// <summary>
    /// Handles navigation events to load assemblies based on the navigation path
    /// </summary>
    Task OnNavigateAsync(NavigationContext args);
}