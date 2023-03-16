namespace RonSijm.Blazyload;

public class BlazyServiceProvider : IServiceProvider
{
    private readonly ServiceCollection _services = new();

    private IServiceProvider _serviceProvider;
    private readonly DefaultServiceProviderFactory _serviceProviderFactory = new();
    private readonly BlazyOptions _options;

    public BlazyServiceProvider(IEnumerable<ServiceDescriptor> services, BlazyOptions options)
    {
        _options = options;
        foreach (var serviceDescriptor in services)
        {
            _services.Add(serviceDescriptor);
        }

        _services.AddSingleton(x => new BlazyAssemblyLoader(x.GetService<LazyAssemblyLoader>(), this));

        _serviceProvider = _serviceProviderFactory.CreateServiceProvider(_services);
    }

    public object GetService(Type serviceType)
    {
        if (serviceType == typeof(IServiceScopeFactory))
        {
            return new BlazyServiceScopeFactory(this);
        }

        if (!serviceType.IsGenericType || serviceType.GetGenericTypeDefinition() != typeof(Optional<>))
        {
            return _serviceProvider.GetService(serviceType);
        }

        dynamic wrapper = Activator.CreateInstance(serviceType);

        var innerType = serviceType.GetGenericArguments()[0];
        var valueForInnerType = _serviceProvider.GetService(innerType);
            
        if (valueForInnerType == null)
        {
            return wrapper;
        }

        wrapper?.SetValue(valueForInnerType);
        return wrapper;

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
            var assemblyOptions = _options?.GetOptions(assemblyName);

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

    public IEnumerable<string> FindReferencedAssemblies(params Assembly[] assemblies)
    {
        var referencedAssemblies = new List<string>();

        foreach (var assembly in assemblies)
        {
            var assemblyName = $"{assembly.GetName().Name}";
            var assemblyOptions = _options?.GetOptions(assemblyName);

            if (BlazyOptions.DisableCascadeLoadingGlobally && assemblyOptions is { DisableCascadeLoading: true })
            {
                continue;
            }

            var referenceAssemblies = assembly.GetReferencedAssemblies();
            referencedAssemblies.AddRange(referenceAssemblies.Select(referenceAssembly => $"{referenceAssembly.Name}.dll"));
        }

        return referencedAssemblies;
    }

    public void CreateServiceProvider()
    {
        _serviceProvider = _serviceProviderFactory.CreateServiceProvider(_services);
    }
}