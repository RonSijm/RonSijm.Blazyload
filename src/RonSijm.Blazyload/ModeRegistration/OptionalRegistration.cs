using RonSijm.Blazyload.Mode;

namespace RonSijm.Blazyload.ModeRegistration
{
    public static class OptionalRegistration
    {
        public static void RegisterOptional(this List<(Func<Type, bool> Criteria, Func<Type, IServiceProvider, object> Factory)> typeFunctionOverrides)
        {
            var isOptionalFunc = new Func<Type, bool>(serviceType => serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(Optional<>));
            var optionalFactory = new Func<Type, IServiceProvider, object>((serviceType, provider) =>
            {
                dynamic wrapper = Activator.CreateInstance(serviceType);

                var innerType = serviceType.GetGenericArguments()[0];
                var valueForInnerType = provider.GetService(innerType);

                if (valueForInnerType == null)
                {
                    return wrapper;
                }

                wrapper?.SetValue(valueForInnerType);
                return wrapper;
            });

            typeFunctionOverrides.Add((isOptionalFunc, optionalFactory));
        }
    }
}