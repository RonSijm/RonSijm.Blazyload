namespace RonSijm.Blazyload.Library.Features.Helpers;

public static class OpenGenericServiceRegistration
{
    public static (Func<Type, bool> isOpenFunc, Func<Type, IServiceProvider, object> optionalFactory) RegisterOpenGeneric(Type openGenericType, Func<Type, IServiceProvider, object> functionForInnerType)
    {
        var result = RegisterOpenGeneric(openGenericType, (_, childType, serviceProvider) => functionForInnerType.Invoke(childType, serviceProvider));
        return result;
    }

    public static (Func<Type, bool> isOpenFunc, Func<Type, IServiceProvider, object> optionalFactory) RegisterOpenGeneric(Type openGenericType, Func<Type, Type, IServiceProvider, object> functionForInnerType)
    {
        var isOpenGenericOfType = new Func<Type, bool>(serviceType => serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == openGenericType);
        var stateFactory = new Func<Type, IServiceProvider, object>((serviceType, provider) =>
        {
            var innerType = serviceType.GetGenericArguments()[0];

            var result = functionForInnerType.Invoke(openGenericType, innerType, provider);

            return result;
        });

        return (isOpenGenericOfType, stateFactory);
    }
}