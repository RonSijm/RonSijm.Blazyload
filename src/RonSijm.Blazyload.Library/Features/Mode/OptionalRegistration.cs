using RonSijm.Blazyload.Library.Features.Helpers;

namespace RonSijm.Blazyload.Library.Features.Mode;

public static class OptionalRegistration
{
    public static void RegisterOptional(this List<(Func<Type, bool> Criteria, Func<Type, IServiceProvider, object> Factory)> typeFunctionOverrides)
    {
        var result = OpenGenericServiceRegistration.RegisterOpenGeneric(typeof(Optional<>), (type, provider) => CreateOptionalRegistration(typeof(Optional<>), type, provider));

        typeFunctionOverrides.Add(result);
    }

    private static object CreateOptionalRegistration(Type parentType, Type childType, IServiceProvider provider)
    {
        var optional = parentType.MakeGenericType(childType);
        dynamic wrapper = Activator.CreateInstance(optional);

        var valueForInnerType = provider.GetService(childType);

        if (valueForInnerType == null)
        {
            return wrapper;
        }

        wrapper?.SetValue(valueForInnerType);
        return wrapper;
    }
}