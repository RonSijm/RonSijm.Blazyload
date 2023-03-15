namespace RonSijm.Blazyload;

public class BlazyServiceScopeFactory : IServiceScopeFactory
{
    private readonly IServiceProvider _serviceProvider;

    public BlazyServiceScopeFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IServiceScope CreateScope()
    {
        return new BlazyServiceScope(_serviceProvider);
    }
}