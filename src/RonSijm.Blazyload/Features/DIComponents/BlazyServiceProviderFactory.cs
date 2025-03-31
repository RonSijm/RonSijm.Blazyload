using RonSijm.Syringe;

namespace RonSijm.Blazyload;

public class BlazyServiceProviderFactory(BlazyloadProviderOptions options) : IServiceProviderFactory<SyringeServiceProviderBuilder>
{
    public SyringeServiceProviderBuilder CreateBuilder(IServiceCollection services)
    {
        services.AddSingleton<IAssemblyLoader, BlazyAssemblyLoader>();
        services.AddSingleton(options.AssemblyLoadConfiguration);
        var container = new SyringeServiceProviderBuilder(services);
        return container;
    }

    public IServiceProvider CreateServiceProvider(SyringeServiceProviderBuilder builderOptions)
    {
        var serviceProvider = builderOptions.GetServiceProvider(options);

        return serviceProvider;
    }
}