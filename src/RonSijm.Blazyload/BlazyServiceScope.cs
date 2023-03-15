namespace RonSijm.Blazyload;

public class BlazyServiceScope : IServiceScope
{
    public BlazyServiceScope(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public void Dispose()
    {

    }

    public IServiceProvider ServiceProvider { get; }
}