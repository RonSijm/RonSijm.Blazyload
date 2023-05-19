namespace RonSijm.Blazyload.Features.DIComponents;

public class BlazyServiceScope : IServiceScope
{
    public BlazyServiceScope(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public IServiceProvider ServiceProvider { get; }
}