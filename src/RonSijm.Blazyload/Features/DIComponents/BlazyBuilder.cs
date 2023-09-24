namespace RonSijm.Blazyload.Features.DIComponents;

public class BlazyBuilder(IServiceCollection services)
{
    public IServiceProvider GetServiceProvider(BlazyOptions blazyOptions)
    {
        if (blazyOptions.ResolveMode == ResolveMode.EnableOptional)
        {
            blazyOptions.AdditionalFactories.RegisterOptional();
        }

        var serviceProvider = new BlazyServiceProvider(services, blazyOptions);

        return serviceProvider;
    }
}