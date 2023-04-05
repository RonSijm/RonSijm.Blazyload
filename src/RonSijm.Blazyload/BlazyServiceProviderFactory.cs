using RonSijm.Blazyload.Options;

namespace RonSijm.Blazyload;

public class BlazyServiceProviderFactory : IServiceProviderFactory<BlazyBuilder>
{
    private readonly BlazyOptions _options;

    public BlazyServiceProviderFactory(Action<BlazyOptions> optionsFactory)
    {
        _options = new BlazyOptions();
        optionsFactory(_options);
    }

    public BlazyServiceProviderFactory(BlazyOptions options = null)
    {
        _options = options;
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