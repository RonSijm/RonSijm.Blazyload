namespace RonSijm.Blazyload.Features.DIComponents;

public class BlazyServiceProvider : IServiceProvider
{
    private readonly ServiceCollection _services = new();
    private readonly Dictionary<Type, Func<IServiceProvider, object>> _typeOverwrites = new();
    private readonly List<(Func<Type, bool> Criteria, Func<Type, IServiceProvider, object> Factory)> _typeFunctionOverrides = new();

    private IServiceProvider _serviceProvider;
    private readonly DefaultServiceProviderFactory _serviceProviderFactory = new();
    
    public BlazyOptions Options { get; }

    public BlazyServiceProvider(IEnumerable<ServiceDescriptor> services, BlazyOptions options)
    {
        Options = options;
        foreach (var serviceDescriptor in services)
        {
            _services.Add(serviceDescriptor);
        }

        _services.AddSingleton(typeof(BlazyServiceProvider), this);

        _typeOverwrites.Add(typeof(IServiceScopeFactory), provider => new BlazyServiceScopeFactory(provider));

        if (options.ResolveMode == ResolveMode.EnableOptional)
        {
            _typeFunctionOverrides.RegisterOptional();
        }

        _serviceProvider = _serviceProviderFactory.CreateServiceProvider(_services);
    }

    public object GetService(Type serviceType)
    {
        var hasOverWrite = _typeOverwrites.TryGetValue(serviceType, out var objectFactory);

        if (hasOverWrite)
        {
            return objectFactory(this);
        }

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator - Justification: Generates shitty linq
        foreach (var typeFunctionOverride in _typeFunctionOverrides)
        {
            if (!typeFunctionOverride.Criteria(serviceType))
            {
                continue;
            }

            var result = typeFunctionOverride.Factory(serviceType, this);
            return result;
        }

        return _serviceProvider.GetService(serviceType);
    }

    public async Task Register(params Assembly[] assemblies)
    {
        if (assemblies == null)
        {
            return;
        }

        foreach (var assembly in assemblies)
        {
            var assemblyName = $"{assembly.GetName().Name}";
            var assemblyOptions = Options?.GetOptions(assemblyName);

            var expectedClassName = assemblyOptions?.ClassPath ?? $"{assemblyName}.Properties.BlazyBootstrap";

            var registration = assembly.CreateInstance(expectedClassName);

            if (registration == null)
            {
                continue;
            }

            var services = await TryLoadServices(registration);

            // If we weren't able to get the services from the assembly, we continue
            if (services == null)
            {
                // If there is a class but no method, that's an error, so we log out it.
                // Don't think there's a need to throw anything.
                Console.Write("Error: BlazyBootstrap Class found but no GetServices");
                continue;
            }

            foreach (var serviceDescriptor in services)
            {
                _services.Add(serviceDescriptor);
            }
        }
    }

    private static async Task<IEnumerable<ServiceDescriptor>> TryLoadServices(object registration)
    {
        // We can't throw an exception, we can't be certain that all lazy loaded assemblies require bootstrapping.
        IEnumerable<ServiceDescriptor> services = null;

        // If the registration is interfaced, it's easiest to use that.
        if (registration is IBlazyBootstrap registrationAsInterface)
        {
            services = await registrationAsInterface.Bootstrap();
        }
        else
        {
            // Otherwise we try to GetServices through reflection.
            var registrationMethodInfo = registration.GetType().GetMethod(nameof(IBlazyBootstrap.Bootstrap));
            if (registrationMethodInfo == null)
            {
                return null;
            }

            if (registrationMethodInfo.Invoke(registration, Array.Empty<object>()) is Task<IEnumerable<ServiceDescriptor>> taskResult)
            {
                services = await taskResult;
            }
        }

        return services;
    }


    public void CreateServiceProvider()
    {
        _serviceProvider = _serviceProviderFactory.CreateServiceProvider(_services);
    }
}