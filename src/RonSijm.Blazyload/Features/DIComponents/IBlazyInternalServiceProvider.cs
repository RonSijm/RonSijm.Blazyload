namespace RonSijm.Blazyload.Features.DIComponents;

public interface IBlazyInternalServiceProvider
{
    bool TryGetServiceFromOverride(Type serviceType, out object value);
}