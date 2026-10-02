using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.Tests;

public class BlazyAssemblyLoadExceptionTests()
{
    [Fact]
    public async Task MissingRequiredAssemblyIsNotReportedAsSuccess()
    {
        var (loader, components, _) = CreateLoader();
        components.FileLoader.LoadAssembliesWithOptionsAsync(Arg.Any<List<(string assemblyToLoad, AssemblyOptions options)>>()).Returns(Task.FromResult(Array.Empty<Assembly>()));

        var exception = await Assert.ThrowsAsync<BlazyAssemblyLoadException>(() => loader.LoadAssemblyAsync("Feature.wasm"));

        Assert.IsType<FileNotFoundException>(exception.InnerException);
        Assert.Empty(loader.AdditionalAssemblies);
        Assert.Empty(components.StateTracker.LoadedAssemblyHashes);
    }

    [Fact]
    public void ExceptionRetainsAssemblyContextAndOriginalCause()
    {
        var original = new HttpRequestException("download failed");
        var exception = new BlazyAssemblyLoadException("Feature.wasm", original);
        Assert.Contains("Feature.wasm", exception.Message);
        Assert.Same(original, exception.InnerException);
    }

    [Fact]
    public async Task DownloadFailurePropagatesWithoutPublishingOrMarkingTheAssembly()
    {
        var (loader, components, _) = CreateLoader();
        var original = new HttpRequestException("download failed");
        components.FileLoader.LoadAssembliesWithOptionsAsync(Arg.Any<List<(string assemblyToLoad, AssemblyOptions options)>>()).Returns(Task.FromException<Assembly[]>(original));

        var exception = await Assert.ThrowsAsync<BlazyAssemblyLoadException>(() => loader.LoadAssemblyAsync("Feature.wasm"));

        Assert.Contains("Feature.wasm", exception.Message);
        Assert.Same(original, exception.InnerException);
        Assert.Empty(loader.AdditionalAssemblies);
        Assert.Empty(components.StateTracker.LoadedAssemblyHashes);
        components.Logger.Received(1).WriteLine(exception);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RegistrationAndFinalizationFailuresPropagate(bool failFinalization)
    {
        var (loader, components, _) = CreateLoader();
        var original = new InvalidOperationException("initialization failed");
        if (failFinalization)
        {
            components.ServiceRegistrar.When(registrar => registrar.FinalizeLoading(Arg.Any<List<Assembly>>(), Arg.Any<List<ServiceDescriptor>>())).Do(_ => throw original);
        }
        else
        {
            components.ServiceRegistrar.RegisterServicesAsync(Arg.Any<Assembly[]>()).Returns(Task.FromException<List<ServiceDescriptor>>(original));
        }

        var exception = await Assert.ThrowsAsync<BlazyAssemblyLoadException>(() => loader.LoadAssemblyAsync("Feature.wasm"));

        Assert.Same(original, exception.InnerException);
        Assert.Empty(loader.AdditionalAssemblies);
        Assert.Empty(components.StateTracker.LoadedAssemblyHashes);
    }

    [Fact]
    public async Task DependencyFailureIsWrappedOnceAtTheRequestedRoot()
    {
        var (loader, components, _) = CreateLoader();
        var original = new HttpRequestException("dependency download failed");
        components.CascadeLoader.LoadReferencedAssembliesAsync(Arg.Any<Assembly[]>(), Arg.Any<Func<IEnumerable<string>, Task<List<Assembly>>>>()).Returns(call => call.ArgAt<Func<IEnumerable<string>, Task<List<Assembly>>>>(1)(["Dependency.wasm"]));
        components.FileLoader.LoadAssembliesWithOptionsAsync(Arg.Is<List<(string assemblyToLoad, AssemblyOptions options)>>(requests => requests.Any(request => request.assemblyToLoad == "Dependency.wasm"))).Returns(Task.FromException<Assembly[]>(original));

        var exception = await Assert.ThrowsAsync<BlazyAssemblyLoadException>(() => loader.LoadAssemblyAsync("Feature.wasm"));

        Assert.Contains("Feature.wasm", exception.Message);
        Assert.Same(original, exception.InnerException);
        Assert.Empty(loader.AdditionalAssemblies);
    }

    [Fact]
    public async Task NavigationDoesNotSwallowLoadFailure()
    {
        var (loader, components, _) = CreateLoader();
        components.NavigationResolver.GetAssemblyForPath("feature").Returns("Feature.wasm");
        var original = new HttpRequestException("download failed");
        components.FileLoader.LoadAssembliesWithOptionsAsync(Arg.Any<List<(string assemblyToLoad, AssemblyOptions options)>>()).Returns(Task.FromException<Assembly[]>(original));

        var exception = await Assert.ThrowsAsync<BlazyAssemblyLoadException>(() => loader.HandleNavigationInternal("feature"));

        Assert.Same(original, exception.InnerException);
    }

    [Fact]
    public async Task SuccessfulLoadingRetainsExistingNewlyLoadedResults()
    {
        var (loader, components, assembly) = CreateLoader();

        Assert.Same(assembly, Assert.Single(await loader.LoadAssemblyAsync("Feature.wasm")));
        Assert.Empty(await loader.LoadAssemblyAsync("Feature.wasm"));
        Assert.Same(assembly, Assert.Single(loader.AdditionalAssemblies));
        components.ServiceRegistrar.Received(1).FinalizeLoading(Arg.Any<List<Assembly>>(), Arg.Any<List<ServiceDescriptor>>());
    }

    private static (BlazyAssemblyLoader Loader, BlazyAssemblyLoaderComponents Components, Assembly Assembly) CreateLoader()
    {
        var assembly = Substitute.For<Assembly>();
        assembly.GetName().Returns(new AssemblyName("Feature"));
        var fileLoader = Substitute.For<IAssemblyFileLoader>();
        fileLoader.LoadAssembliesWithOptionsAsync(Arg.Any<List<(string assemblyToLoad, AssemblyOptions options)>>()).Returns(call =>
        {
            var requests = call.ArgAt<List<(string assemblyToLoad, AssemblyOptions options)>>(0);
            var assemblies = requests.Select(_ => assembly).ToArray();
            return Task.FromResult(assemblies);
        });
        var optionsResolver = Substitute.For<IAssemblyOptionsResolver>();
        optionsResolver.ResolveOptions(Arg.Any<IEnumerable<string>>()).Returns(call => call.ArgAt<IEnumerable<string>>(0).Select(name => (name, new AssemblyOptions())).ToList());
        var registrar = Substitute.For<IAssemblyServiceRegistrar>();
        registrar.RegisterServicesAsync(Arg.Any<Assembly[]>()).Returns(Task.FromResult(new List<ServiceDescriptor>()));
        var cascade = Substitute.For<ICascadeAssemblyLoader>();
        cascade.LoadReferencedAssembliesAsync(Arg.Any<Assembly[]>(), Arg.Any<Func<IEnumerable<string>, Task<List<Assembly>>>>()).Returns(Task.FromResult(new List<Assembly>()));
        var components = new BlazyAssemblyLoaderComponents
        {
            Options = new AssemblyLoaderOptions(),
            Logger = Substitute.For<IBlazyLogger>(),
            StateTracker = new AssemblyStateTracker { PreloadedAssemblies = [] },
            FileLoader = fileLoader,
            OptionsResolver = optionsResolver,
            ServiceRegistrar = registrar,
            CascadeLoader = cascade,
            NavigationResolver = Substitute.For<INavigationAssemblyResolver>(),
            ExtensionInitializer = Substitute.For<IExtensionInitializer>()
        };
        return (new BlazyAssemblyLoader(components), components, assembly);
    }
}
