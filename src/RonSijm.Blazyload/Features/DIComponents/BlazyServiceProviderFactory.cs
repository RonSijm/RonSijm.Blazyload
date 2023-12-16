namespace RonSijm.Blazyload.Features.DIComponents;

public class BlazyServiceProviderFactory : IServiceProviderFactory<BlazyBuilder>
{
    private readonly BlazyOptions _options;

    public BlazyServiceProviderFactory(BlazyOptions options)
    {
        _options = options;
    }

    public BlazyBuilder CreateBuilder(IServiceCollection services)
    {
        services.AddSingleton<BlazyAssemblyLoader>();
        var container = new BlazyBuilder(services);
        return container;
    }

    public IServiceProvider CreateServiceProvider(BlazyBuilder blazyBuilder)
    {
        var serviceProvider = blazyBuilder.GetServiceProvider(_options);

        return serviceProvider;
    }
}