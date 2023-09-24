namespace RonSijm.Blazyload.Features.DIComponents;

public class BlazyServiceProviderFactory : IServiceProviderFactory<BlazyBuilder>
{
    private readonly BlazyOptions _options;

    public BlazyServiceProviderFactory(BlazyOptions options, IServiceCollection serviceCollection)
    {
        _options = options;
        serviceCollection.AddSingleton<BlazyAssemblyLoader>();
    }

    public BlazyBuilder CreateBuilder(IServiceCollection services)
    {
        var container = new BlazyBuilder(services);
        return container;
    }

    public IServiceProvider CreateServiceProvider(BlazyBuilder blazyBuilder)
    {
        var serviceProvider = blazyBuilder.GetServiceProvider(_options);

        return serviceProvider;
    }
}