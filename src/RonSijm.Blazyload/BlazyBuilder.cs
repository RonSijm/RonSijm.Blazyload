using RonSijm.Blazyload.Options;

namespace RonSijm.Blazyload;

public class BlazyBuilder
{
    private readonly IServiceCollection _services;

    public BlazyBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public IServiceProvider GetServiceProvider(BlazyOptions blazyOptions)
    {
        var serviceProvider = new BlazyServiceProvider(_services, blazyOptions);

        return serviceProvider;
    }
}