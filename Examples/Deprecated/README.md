# Deprecated Examples

This folder contains deprecated examples showing the old V1 style of using Blazyload.

## V1 Style (Deprecated)

In V1, you had to manually handle navigation in `App.razor.cs`:

```csharp
public partial class App
{
    private async Task OnNavigateAsync(NavigationContext args)
    {
        if (args.Path == "fetchdata1")
        {
            await AssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLib1.wasm");
        }
        // ... more manual path checks
    }
}
```

## V2 Style (Recommended)

In V2, use `LoadOnNavigation` in `Program.cs` and let Blazyload handle navigation automatically:

```csharp
// Program.cs
builder.UseBlazyload(options =>
{
    options.LoadOnNavigation("fetchdata1", "RonSijm.Demo.Blazyload.WeatherLib1.wasm");
    options.LoadOnNavigation("fetchdata2", "RonSijm.Demo.Blazyload.WeatherLib2.wasm");
    // Or use a predicate for multiple routes:
    options.LoadOnNavigation(path => path.Contains("weather"), "WeatherLib.wasm");
});
```

```razor
@* App.razor - much simpler! *@
@inject IBlazyAssemblyLoader AssemblyLoader;

<Router AppAssembly="@typeof(App).Assembly" 
        OnNavigateAsync="@AssemblyLoader.OnNavigateAsync" 
        AdditionalAssemblies="@AssemblyLoader.AdditionalAssemblies">
    ...
</Router>
```

No more `App.razor.cs` code-behind file needed!

