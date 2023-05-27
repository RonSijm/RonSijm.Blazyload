# RonSijm.Blazyload

[![.NET](https://github.com/RonSijm/RonSijm.Blazyload/actions/workflows/build_main.yml/badge.svg?branch=main)](https://github.com/RonSijm/RonSijm.Blazyload/actions/workflows/build_main.yml) [![NuGet](https://img.shields.io/nuget/v/RonSijm.Blazyload)](https://www.nuget.org/packages/RonSijm.Blazyload/) [![Codecov](https://codecov.io/gh/RonSijm/RonSijm.Blazyload/branch/main/graph/badge.svg?token=PIDRVFD6IW)](https://codecov.io/gh/RonSijm/RonSijm.Blazyload)

A C# Blazor library to effortlessly implement Lazy Loading and Dependency Injection

NuGet: https://www.nuget.org/packages/RonSijm.Blazyload/

# What is this library:

This library fixes all lazy-loading related things. Mainly dependency injection when dependencies are lazy-loaded.

# Features:
 - [x] Have Standalone packages with Dependency injection working.
 - [x] Cascade loads dependencies so you don't have to worry about dll orchestration
   - [x] you just load the "parent" dll you need, and the children are loaded automatically
 - [x] Have An Optional<T> Wrapper because Blazor won't let DI return null
 - [x] Dynamically loading dlls without blazor telling you "You haven't registered that one as dll as lazyloaded"
 - [x] Lazyload assemblies from alternative paths, other than /_framework/
   - [x] Put your libraries in /_framework/authenticated/feature1 or whatever
   - [x] Put your libraries in an absolute url and load it from a remote location
 - [x] Lazyload assemblies in an authenticated way
   - [x] Append headers to the load requests
   - [x] Append path parameters to the load requests
 - [x] Lazyload assemblies from class, not just from the router.
    - [x] Router is still aware of new pages/paths

# Demo / Tutorial video:  

[![Video of RonSijm.Blazyload](https://i.ytimg.com/vi_webp/2_ChpfpAcXI/maxresdefault.webp)](https://youtu.be/2_ChpfpAcXI)

# Deployed Demo of examples Demo1:  

https://ronsijm.github.io/RonSijm.Blazyload/

# Dependency loading explained:

Here is a picture to better explain the purpose:

<img width="1069" alt="lazy-loading" src="https://user-images.githubusercontent.com/337928/225349617-8d64deff-58e5-4ac3-a65b-cd45841b27d1.png">

In this example you have the main project, `"Host"` - which has a router which lazy-loads the `Weatherlib.dll` project, when the `/fetchdata/` page is visited.  
However, the FetchData requires a dependency `IWeatherResolver` that's also in the same `Weatherlib.dll` project. With default Blazor this will throw an exception, and it's not possible to lazy-load classes from dependencies

This article by Peter Himschoot describes the same problem that this library is trying to solve:
https://blogs.u2u.be/peter/post/blazor-lazy-loading-and-dependency-injection
However, at the critical point, he mentions:
> When you run this project, it will fail. This is because when we configure DependencyInjection in our Program.cs this line fails:
> System.IO.FileNotFoundException: Could not load file or assembly 'ServiceProxies, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies

His solution is the following approach:
> The way to fix this is to use a class that is not lazy loaded, and that will create the dependency after we have loaded the assembly.

So his solution creates a `ServiceProxy` / `ServiceLocator` that is not lazy-loaded, which he can inject in the non-lazy loaded project, and use it to locate services

There are plenty of [articles](https://freecontent.manning.com/the-service-locator-anti-pattern/) that outline why the service locator is an anti-pattern, so I didn't like that solution.  
(No criticism to Peter, there was no easy out-of-the-box way in Blazor to fix dependency injection without a service locator (until now :smile:))

# Getting started:

To get started with Lazy Loading in your general project, follow this tutorial to enable default lazy loading:
https://learn.microsoft.com/en-us/aspnet/core/blazor/webassembly-lazy-load-assemblies?view=aspnetcore-7.0

To enable Blazyload:  

In your `program.cs` add/change your container configuration to:  
` builder.UseBlazyload();`
	
For example:

````
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
         builder.UseBlazyload();
    	// other stuff
    }
````

 Additionally you can add some options in the constructor of the `BlazyServiceProviderFactory` which are described later.
  
The only difference between the implementation of the tutorial and using Blazyload:

In the tutorial they mention in `App.razor` to use:  
`@inject LazyAssemblyLoader AssemblyLoader`
 
To use Blazyload, replace that line with:  
`@inject BlazyAssemblyLoader AssemblyLoader;`

# Dependency registration

## Default with package reference to Blazyload
(See: `RonSijm.Demo.Blazyload.WeatherLib1` example 1)

In the library that you want to lazyload: 
- add a class `"BlazyRegistration"` in the Properties folder.
- Implement the interface IBlazyBootstrap from RonSijm.Blazyload.Library

Full sample:

````
public class BlazyBootstrap : IBlazyBootstrap
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWeatherResolver, WeatherResolver>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}
````

Pros: It's the easiest way to use Blazyload
Cons: With this approach all your libraries will need to have a reference to RonSijm.Blazyload.Library

## Default without package reference
(See: `RonSijm.Demo.Blazyload.WeatherLib2` example 2)

Do the following in the library that you want to lazyload: 
- add a class `"BlazyRegistration"` in the Properties folder.
- Implement the method `"public async Task<IEnumerable<ServiceDescriptor>> Bootstrap()"` - exactly as shown in the interface, but without using the interface explicitly

Full sample:
````
public class BlazyBootstrap
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWeatherResolver, WeatherResolver>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}
````

Pros: With this approach your library no longer needs a reference to RonSijm.Blazyload.Library
Pros: If you create a library, and your consumer does not want to use RonSijm.Blazyload.Library (or don't want to use it lazy) - the consumer of your library can still call BlazyBootstrap manually from program.cs and register it themselves

## Custom Class mode:
(See: `RonSijm.Demo.Blazyload.WeatherLib3` example 3)

Do the following in the library that you want to lazyload: 
- Create class that either implements IBlazyBootstrap OR Implements the method "public IEnumerable<ServiceDescriptor> Bootstrap()"
- In your `program.cs`, reference the class you're using:

  ```
    builder.ConfigureContainer(new BlazyServiceProviderFactory(x =>
    {
        x.UseCustomClass("RonSijm.Demo.Blazyload.WeatherLib3", "RonSijm.Demo.Blazyload.WeatherLib3.CustomRegistrationClass");
    }));
  ```

 Full sample:
 
````
public class CustomRegistrationClass : IBlazyBootstrap
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWeatherResolver, WeatherResolver>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}
````
  
Pros: You can now use a Custom class that's named differently than BlazyRegistration. 
Cons: You don't have a class bound to the interface, so if the interface changes you won't get any errors. (I'll try to keep the interface the same so this doesn't happen)
  
## Cascading dependencies
(See: `RonSijm.Demo.Blazyload.WeatherLib4.Page` example 4)

When you want to lazy load a library, and that other library also has references that need to be lazy loaded, the references will be lazy loaded automatically.
This will allow you to keep the lazy loading wiring in your router simple, and you don't have to worry about registering the entire dependency tree. You can reference the object you want to lazy load, and RonSijm.Blazyload will take care of loading the children

Here is a picture to demonstrate an example:
  
<img width="1182" alt="lazy-cascaded" src="https://user-images.githubusercontent.com/337928/225361023-3f516d65-75e2-42f1-9dc6-b51336edb762.png">

In this example you have the main project `"Host"` - has a router which lazy-loads the `Weatherlib.Page.dll` project, when the /fetchdata/ page is visited.  
The fetchData page requires a `IWeatherResolver`, which in turn is in the `WeatherLib4.Component.dll`  

With default Blazor this setup would not be possible. 

Besides that, with default lazy loading in your router you would have to specify all the dlls that you'd want to load - which discourages an optimized lazy-loaded architecture with a lot of small packages, because you will have to keep track of which dlls you need in which dependency tree.

With this package you can create a complicated dll dependency tree with a lot of smaller packages, which will optimize lazy loading
 
## Blazor optional decencies

Out of the box Blazor does not support Optional dependencies. This library fixes that by introducing `Optional<T>`

`@inject Optional<IWeatherResolver> WeatherResolver`

Note that it uses a custom `Optional<T>` instead of `Nullable<T>` because Nullable only accepts structs.
 
You can enable it in the program.cs by adding the following line:

````
builder.ConfigureContainer(new BlazyServiceProviderFactory(x =>
{
    x.ResolveMode = ResolveMode.EnableOptional;
}
````

 ## Loading Assemblies from any where:

 The default pattern of setting up your Router is like this:

 ````
     private async Task OnNavigateAsync(NavigationContext args)
    {
        try
        {
            if (args.Path == "fetchdata1")
            {
                var assemblies = await BlazyAssemblyLoader.LoadAssembliesAsync(new[] { "RonSijm.Demo.Blazyload.WeatherLib1.dll" });
                _lazyLoadedAssemblies.AddRange(assemblies);
            }
        }
    }
 ````

 This loads assemblies, and then adds them to the `AdditionalAssemblies` property.

 With this library you can still to that, but it's advised to just subscribe your router to new assemblies being loaded:

````
    protected override void OnInitialized()
    {
        BlazyAssemblyLoader.OnAssembliesLoaded += list =>
        {
            _lazyLoadedAssemblies.AddRange(list);
            StateHasChanged();
        };

        base.OnInitialized();
    }
````

Then you don't need the ` _lazyLoadedAssemblies.AddRange(assemblies);` line anymore, and you can load assemblies from everywhere, and your router will be aware of new pages

 ## Live Demo:

![blazyload](https://user-images.githubusercontent.com/337928/225398408-005411c6-6c71-4f66-b1a4-b2485730cbf0.gif)
  
 This is a video of the example projects in use. To try them yourself you can clone or fork this repo and run `RonSijm.Demo.Blazyload.Host`
 
 ## TODOS before V1.1.1+:

  - [ ] Expose some of the internal load stats of Blazyload, so you can inspect what happened at runtime
  - [ ] Figure out why bUnit won't use `AddFallbackServiceProvider`

 ## TODOS before V1.2+:

  - [ ] Figure how to have something like an <OptionalComponent> so that we can load components based on whether or not a dll is loaded
  - [ ] Possibly make a Roslyn Analyzer that compile time checks if your config is remotely correct
  - [ ] Add a LazyLoad background qeueue, because there's no point to wait until a user clicks something, and then start loading dlls. If the app is somewhat idle, we can load dlls in the background
  - [ ] Option to load the .dll.gz version, or to HEAD call to see what exists, and load the .dll.gz version if available. Either though options or something. HEAD call might give more overhead than just yolo calling dlls

 ## TODOS before V1.x+:
  
  - [ ] See how well this could work with other low level packages,
    - [ ] For example [Fluxor.Blazor.Web](https://www.nuget.org/packages/Fluxor.Blazor.Web) - and see whether it's possible to inject new state object trees into it after its initialized

 # Removed Features:
 - [ ] (Lazy) loading PDBs for debugging purposes. Microsoft claims you can use PDBs to debug lazy loaded dlls, but I haven't ever seen it work. Just normal debugging is already a hassle in Blazor, so I haven't bothered porting loading PDBs for debug purposes, because on my machine it doesn't work anyways


 ## Contributing:

 - Bugfixes:
Submit a PR with a bugfix + a unit-test that is red in the current version but green in the fixed version

 - Features:
Start a discussion to ask if the feature fits and if it's "up for grabs". If so you can build the feature

 ## Contact and questions:
I've created a Discord, though now it's just me, lol.. https://discord.gg/cDC6VkUn2X - but you can ask stuff here