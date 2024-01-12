// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;

internal sealed class CallSiteFactory : IServiceProviderIsService
{
    private const int DefaultSlot = 0;
    private readonly ServiceDescriptor[] _descriptors;
    private readonly ConcurrentDictionary<ServiceCacheKey, ServiceCallSite> _callSiteCache = new();
    private readonly Dictionary<Type, ServiceDescriptorCacheItem> _descriptorLookup = new();
    private readonly ConcurrentDictionary<Type, object> _callSiteLocks = new();

    private readonly StackGuard _stackGuard;
    private readonly IBlazyInternalServiceProvider _internalServiceProvider;

    public CallSiteFactory(ICollection<ServiceDescriptor> descriptors, IBlazyInternalServiceProvider internalServiceProvider)
    {
        _internalServiceProvider = internalServiceProvider;
        _stackGuard = new StackGuard();
        _descriptors = new ServiceDescriptor[descriptors.Count];
        descriptors.CopyTo(_descriptors, 0);

        Populate();
    }

    private void Populate()
    {
        foreach (var descriptor in _descriptors)
        {
            var serviceType = descriptor.ServiceType;
            if (serviceType.IsGenericTypeDefinition)
            {
                var implementationType = descriptor.ImplementationType;

                if (implementationType == null || !implementationType.IsGenericTypeDefinition)
                {
                    throw new ArgumentException(StringResources.Format("OpenGenericServiceRequiresOpenGenericImplementation", serviceType), nameof(_descriptors));
                }

                if (implementationType.IsAbstract || implementationType.IsInterface)
                {
                    throw new ArgumentException(
                        StringResources.Format("TypeCannotBeActivated", implementationType, serviceType));
                }

                var serviceTypeGenericArguments = serviceType.GetGenericArguments();
                var implementationTypeGenericArguments = implementationType.GetGenericArguments();
                if (serviceTypeGenericArguments.Length != implementationTypeGenericArguments.Length)
                {
                    throw new ArgumentException(StringResources.Format("ArityOfOpenGenericServiceNotEqualArityOfOpenGenericImplementation", serviceType, implementationType), nameof(_descriptors));
                }

                if (MicrosoftServiceProvider.VerifyOpenGenericServiceTrimmability)
                {
                    ValidateTrimmingAnnotations(serviceType, serviceTypeGenericArguments, implementationType, implementationTypeGenericArguments);
                }
            }
            else if (descriptor.ImplementationInstance == null && descriptor.ImplementationFactory == null)
            {
                Debug.Assert(descriptor.ImplementationType != null);
                var implementationType = descriptor.ImplementationType;

                if (implementationType.IsGenericTypeDefinition ||
                    implementationType.IsAbstract ||
                    implementationType.IsInterface)
                {
                    throw new ArgumentException(
                        StringResources.Format("TypeCannotBeActivated", implementationType, serviceType));
                }
            }

            var cacheKey = serviceType;
            _descriptorLookup.TryGetValue(cacheKey, out var cacheItem);
            _descriptorLookup[cacheKey] = cacheItem.Add(descriptor);
        }
    }

    /// <summary>
    /// Validates that two generic type definitions have compatible trimming annotations on their generic arguments.
    /// </summary>
    /// <remarks>
    /// When open generic types are used in DI, there is an error when the concrete implementation type
    /// has [DynamicallyAccessedMembers] attributes on a generic argument type, but the interface/service type
    /// doesn't have matching annotations. The problem is that the trimmer doesn't see the members that need to
    /// be preserved on the type being passed to the generic argument. But when the interface/service type also has
    /// the annotations, the trimmer will see which members need to be preserved on the closed generic argument type.
    /// </remarks>
    private static void ValidateTrimmingAnnotations(Type serviceType, Type[] serviceTypeGenericArguments, Type implementationType, Type[] implementationTypeGenericArguments)
    {
        Debug.Assert(serviceTypeGenericArguments.Length == implementationTypeGenericArguments.Length);

        for (var i = 0; i < serviceTypeGenericArguments.Length; i++)
        {
            var serviceGenericType = serviceTypeGenericArguments[i];
            var implementationGenericType = implementationTypeGenericArguments[i];

            var serviceDynamicallyAccessedMembers = GetDynamicallyAccessedMemberTypes(serviceGenericType);
            var implementationDynamicallyAccessedMembers = GetDynamicallyAccessedMemberTypes(implementationGenericType);

            if (!AreCompatible(serviceDynamicallyAccessedMembers, implementationDynamicallyAccessedMembers))
            {
                throw new ArgumentException(StringResources.Format("TrimmingAnnotationsDoNotMatch", implementationType, serviceType));
            }

            var serviceHasNewConstraint = serviceGenericType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint);
            var implementationHasNewConstraint = implementationGenericType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint);
            if (implementationHasNewConstraint && !serviceHasNewConstraint)
            {
                throw new ArgumentException(StringResources.Format("TrimmingAnnotationsDoNotMatch_NewConstraint", implementationType, serviceType));
            }
        }
    }

    private static DynamicallyAccessedMemberTypes GetDynamicallyAccessedMemberTypes(Type serviceGenericType)
    {
        foreach (var attributeData in serviceGenericType.GetCustomAttributesData())
        {
            if (attributeData.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute" &&
                attributeData.ConstructorArguments.Count == 1 &&
                attributeData.ConstructorArguments[0].ArgumentType.FullName == "System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes")
            {
                return (DynamicallyAccessedMemberTypes)(int)attributeData.ConstructorArguments[0].Value!;
            }
        }

        return DynamicallyAccessedMemberTypes.None;
    }

    private static bool AreCompatible(DynamicallyAccessedMemberTypes serviceDynamicallyAccessedMembers, DynamicallyAccessedMemberTypes implementationDynamicallyAccessedMembers)
    {
        // The DynamicallyAccessedMemberTypes don't need to exactly match.
        // The service type needs to preserve a superset of the members required by the implementation type.
        return serviceDynamicallyAccessedMembers.HasFlag(implementationDynamicallyAccessedMembers);
    }

    internal ServiceCallSite GetCallSite(Type serviceType, CallSiteChain callSiteChain)
    {
        if (_callSiteCache.TryGetValue(new ServiceCacheKey(serviceType, DefaultSlot), out var site))
        {
            return site;
        }
        else
        {
            return CreateCallSite(serviceType, callSiteChain);
        }
    }

    internal ServiceCallSite GetCallSite(ServiceDescriptor serviceDescriptor, CallSiteChain callSiteChain)
    {
        if (_descriptorLookup.TryGetValue(serviceDescriptor.ServiceType, out var descriptor))
        {
            return TryCreateExact(serviceDescriptor, serviceDescriptor.ServiceType, callSiteChain, descriptor.GetSlot(serviceDescriptor));
        }

        Debug.Fail("_descriptorLookup didn't contain requested serviceDescriptor");
        return null;
    }

    private ServiceCallSite CreateCallSite(Type serviceType, CallSiteChain callSiteChain)
    {
        if (!_stackGuard.TryEnterOnCurrentStack())
        {
            return _stackGuard.RunOnEmptyStack(CreateCallSite, serviceType, callSiteChain);
        }

        // We need to lock the resolution process for a single service type at a time:
        // Consider the following:
        // C -> D -> A
        // E -> D -> A
        // Resolving C and E in parallel means that they will be modifying the callsite cache concurrently
        // to add the entry for C and E, but the resolution of D and A is synchronized
        // to make sure C and E both reference the same instance of the callsite.

        // This is to make sure we can safely store singleton values on the callsites themselves

        var callsiteLock = _callSiteLocks.GetOrAdd(serviceType, static _ => new object());

        lock (callsiteLock)
        {
            callSiteChain.CheckCircularDependency(serviceType);

            var callSite = TryCreateExact(serviceType, callSiteChain);
            if (callSite == null)
            {
                callSite = TryCreateOpenGeneric(serviceType, callSiteChain);

                if (callSite == null)
                {
                    callSite = TryCreateEnumerable(serviceType, callSiteChain);
                }

                if (callSite == null)
                {
                    var resolved = _internalServiceProvider.TryGetServiceFromOverride(serviceType, out var value);
                    if (resolved)
                    {
                        var cache = new ResultCache(ServiceLifetime.Singleton, serviceType, DefaultSlot);
                        return new FactoryCallSite(cache, serviceType, _ =>
                        {
                            var result = _internalServiceProvider.TryGetServiceFromOverride(serviceType, out var innerValue);
                            return innerValue;
                        });
                    }
                }
            }

            return callSite;
        }
    }

    private ServiceCallSite TryCreateExact(Type serviceType, CallSiteChain callSiteChain)
    {
        if (_descriptorLookup.TryGetValue(serviceType, out var descriptor))
        {
            return TryCreateExact(descriptor.Last, serviceType, callSiteChain, DefaultSlot);
        }

        return null;
    }

    private ServiceCallSite TryCreateOpenGeneric(Type serviceType, CallSiteChain callSiteChain)
    {
        if (serviceType.IsConstructedGenericType
            && _descriptorLookup.TryGetValue(serviceType.GetGenericTypeDefinition(), out var descriptor))
        {
            return TryCreateOpenGeneric(descriptor.Last, serviceType, callSiteChain, DefaultSlot, true);
        }

        return null;
    }

    private ServiceCallSite TryCreateEnumerable(Type serviceType, CallSiteChain callSiteChain)
    {
        var callSiteKey = new ServiceCacheKey(serviceType, DefaultSlot);
        if (_callSiteCache.TryGetValue(callSiteKey, out var serviceCallSite))
        {
            return serviceCallSite;
        }

        try
        {
            callSiteChain.Add(serviceType);

            if (serviceType.IsConstructedGenericType &&
                serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var itemType = serviceType.GenericTypeArguments[0];
                var cacheLocation = CallSiteResultCacheLocation.Root;

                var callSites = new List<ServiceCallSite>();

                // If item type is not generic we can safely use descriptor cache
                if (!itemType.IsConstructedGenericType &&
                    _descriptorLookup.TryGetValue(itemType, out var descriptors))
                {
                    for (var i = 0; i < descriptors.Count; i++)
                    {
                        var descriptor = descriptors[i];

                        // Last service should get slot 0
                        var slot = descriptors.Count - i - 1;
                        // There may not be any open generics here
                        var callSite = TryCreateExact(descriptor, itemType, callSiteChain, slot);
                        Debug.Assert(callSite != null);

                        cacheLocation = GetCommonCacheLocation(cacheLocation, callSite.Cache.Location);
                        callSites.Add(callSite);
                    }
                }
                else
                {
                    var slot = 0;
                    // We are going in reverse so the last service in descriptor list gets slot 0
                    for (var i = _descriptors.Length - 1; i >= 0; i--)
                    {
                        var descriptor = _descriptors[i];
                        var callSite = TryCreateExact(descriptor, itemType, callSiteChain, slot) ??
                                       TryCreateOpenGeneric(descriptor, itemType, callSiteChain, slot, false);

                        if (callSite != null)
                        {
                            slot++;

                            cacheLocation = GetCommonCacheLocation(cacheLocation, callSite.Cache.Location);
                            callSites.Add(callSite);
                        }
                    }

                    callSites.Reverse();
                }


                var resultCache = ResultCache.None;
                if (cacheLocation == CallSiteResultCacheLocation.Scope || cacheLocation == CallSiteResultCacheLocation.Root)
                {
                    resultCache = new ResultCache(cacheLocation, callSiteKey);
                }

                return _callSiteCache[callSiteKey] = new EnumerableCallSite(resultCache, itemType, callSites.ToArray());
            }

            return null;
        }
        finally
        {
            callSiteChain.Remove(serviceType);
        }
    }

    private static CallSiteResultCacheLocation GetCommonCacheLocation(CallSiteResultCacheLocation locationA, CallSiteResultCacheLocation locationB)
    {
        return (CallSiteResultCacheLocation)Math.Max((int)locationA, (int)locationB);
    }

    private ServiceCallSite TryCreateExact(ServiceDescriptor descriptor, Type serviceType, CallSiteChain callSiteChain, int slot)
    {
        if (serviceType == descriptor.ServiceType)
        {
            var callSiteKey = new ServiceCacheKey(serviceType, slot);
            if (_callSiteCache.TryGetValue(callSiteKey, out var serviceCallSite))
            {
                return serviceCallSite;
            }

            ServiceCallSite callSite;
            var lifetime = new ResultCache(descriptor.Lifetime, serviceType, slot);
            if (descriptor.ImplementationInstance != null)
            {
                callSite = new ConstantCallSite(descriptor.ServiceType, descriptor.ImplementationInstance);
            }
            else if (descriptor.ImplementationFactory != null)
            {
                callSite = new FactoryCallSite(lifetime, descriptor.ServiceType, descriptor.ImplementationFactory);
            }
            else if (descriptor.ImplementationType != null)
            {
                callSite = CreateConstructorCallSite(lifetime, descriptor.ServiceType, descriptor.ImplementationType, callSiteChain);
            }
            else
            {
                throw new InvalidOperationException("InvalidServiceDescriptor");
            }

            return _callSiteCache[callSiteKey] = callSite;
        }

        return null;
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055:MakeGenericType",
        Justification = "MakeGenericType here is used to create a closed generic implementation type given the closed service type. " +
                        "Trimming annotations on the generic types are verified when 'Microsoft.Extensions.DependencyInjection.VerifyOpenGenericServiceTrimmability' is set, which is set by default when PublishTrimmed=true. " +
                        "That check informs developers when these generic types don't have compatible trimming annotations.")]
    private ServiceCallSite TryCreateOpenGeneric(ServiceDescriptor descriptor, Type serviceType, CallSiteChain callSiteChain, int slot, bool throwOnConstraintViolation)
    {
        if (serviceType.IsConstructedGenericType &&
            serviceType.GetGenericTypeDefinition() == descriptor.ServiceType)
        {
            var callSiteKey = new ServiceCacheKey(serviceType, slot);
            if (_callSiteCache.TryGetValue(callSiteKey, out var serviceCallSite))
            {
                return serviceCallSite;
            }

            Debug.Assert(descriptor.ImplementationType != null, "descriptor.ImplementationType != null");
            var lifetime = new ResultCache(descriptor.Lifetime, serviceType, slot);
            Type closedType;
            try
            {
                closedType = descriptor.ImplementationType.MakeGenericType(serviceType.GenericTypeArguments);
            }
            catch (ArgumentException)
            {
                if (throwOnConstraintViolation)
                {
                    throw;
                }

                return null;
            }

            return _callSiteCache[callSiteKey] = CreateConstructorCallSite(lifetime, serviceType, closedType, callSiteChain);
        }

        return null;
    }

    private ServiceCallSite CreateConstructorCallSite(ResultCache lifetime, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, CallSiteChain callSiteChain)
    {
        try
        {
            callSiteChain.Add(serviceType, implementationType);
            var constructors = implementationType.GetConstructors();

            ServiceCallSite[] parameterCallSites = null;

            if (constructors.Length == 0)
            {
                throw new InvalidOperationException(StringResources.Format("NoConstructorMatch", implementationType));
            }
            else if (constructors.Length == 1)
            {
                var constructor = constructors[0];
                var parameters = constructor.GetParameters();
                if (parameters.Length == 0)
                {
                    return new ConstructorCallSite(lifetime, serviceType, constructor);
                }

                parameterCallSites = CreateArgumentCallSites(implementationType, callSiteChain, parameters, throwIfCallSiteNotFound: true);

                return new ConstructorCallSite(lifetime, serviceType, constructor, parameterCallSites);
            }

            Array.Sort(constructors, (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));

            ConstructorInfo bestConstructor = null;
            HashSet<Type> bestConstructorParameterTypes = null;
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                var currentParameterCallSites = CreateArgumentCallSites(
                    implementationType,
                    callSiteChain,
                    parameters,
                    throwIfCallSiteNotFound: false);

                if (currentParameterCallSites != null)
                {
                    if (bestConstructor == null)
                    {
                        bestConstructor = constructor;
                        parameterCallSites = currentParameterCallSites;
                    }
                    else
                    {
                        // Since we're visiting constructors in decreasing order of number of parameters,
                        // we'll only see ambiguities or supersets once we've seen a 'bestConstructor'.

                        if (bestConstructorParameterTypes == null)
                        {
                            bestConstructorParameterTypes = new HashSet<Type>();
                            foreach (var p in bestConstructor.GetParameters())
                            {
                                bestConstructorParameterTypes.Add(p.ParameterType);
                            }
                        }

                        foreach (var p in parameters)
                        {
                            if (!bestConstructorParameterTypes.Contains(p.ParameterType))
                            {
                                // Ambiguous match exception
                                throw new InvalidOperationException(string.Join(Environment.NewLine, StringResources.Format("AmbiguousConstructorException", implementationType), bestConstructor, constructor));
                            }
                        }
                    }
                }
            }

            if (bestConstructor == null)
            {
                throw new InvalidOperationException(StringResources.Format("UnableToActivateTypeException", implementationType));
            }
            else
            {
                Debug.Assert(parameterCallSites != null);
                return new ConstructorCallSite(lifetime, serviceType, bestConstructor, parameterCallSites);
            }
        }
        finally
        {
            callSiteChain.Remove(serviceType);
        }
    }

    /// <returns>Not <b>null</b> if <b>throwIfCallSiteNotFound</b> is true</returns>
    private ServiceCallSite[] CreateArgumentCallSites(Type implementationType, CallSiteChain callSiteChain, ParameterInfo[] parameters, bool throwIfCallSiteNotFound)
    {
        var parameterCallSites = new ServiceCallSite[parameters.Length];
        for (var index = 0; index < parameters.Length; index++)
        {
            var parameterType = parameters[index].ParameterType;
            var callSite = GetCallSite(parameterType, callSiteChain);

            if (callSite == null && ParameterDefaultValue.TryGetDefaultValue(parameters[index], out var defaultValue))
            {
                callSite = new ConstantCallSite(parameterType, defaultValue);
            }

            if (callSite == null)
            {
                if (throwIfCallSiteNotFound)
                {
                    throw new InvalidOperationException(StringResources.Format("CannotResolveService",
                        parameterType,
                        implementationType));
                }

                return null;
            }

            parameterCallSites[index] = callSite;
        }

        return parameterCallSites;
    }


    public void Add(Type type, ServiceCallSite serviceCallSite)
    {
        _callSiteCache[new ServiceCacheKey(type, DefaultSlot)] = serviceCallSite;
    }

    public bool IsService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        // Querying for an open generic should return false (they aren't resolvable)
        if (serviceType.IsGenericTypeDefinition)
        {
            return false;
        }

        if (_descriptorLookup.ContainsKey(serviceType))
        {
            return true;
        }

        if (serviceType.IsConstructedGenericType && serviceType.GetGenericTypeDefinition() is { } genericDefinition)
        {
            // We special case IEnumerable since it isn't explicitly registered in the container
            // yet we can manifest instances of it when requested.
            return genericDefinition == typeof(IEnumerable<>) || _descriptorLookup.ContainsKey(genericDefinition);
        }

        // These are the built in service types that aren't part of the list of service descriptors
        // If you update these make sure to also update the code in ServiceProvider.ctor
        return serviceType == typeof(IServiceProvider) ||
               serviceType == typeof(IServiceScopeFactory) ||
               serviceType == typeof(IServiceProviderIsService);
    }

    private struct ServiceDescriptorCacheItem
    {
        [DisallowNull]
        private ServiceDescriptor _item;

        [DisallowNull]
        private List<ServiceDescriptor> _items;

        public readonly ServiceDescriptor Last
        {
            get
            {
                if (_items is { Count: > 0 })
                {
                    return _items[^1];
                }

                Debug.Assert(_item != null);
                return _item;
            }
        }

        public readonly int Count
        {
            get
            {
                if (_item == null)
                {
                    Debug.Assert(_items == null);
                    return 0;
                }

                return 1 + (_items?.Count ?? 0);
            }
        }

        public readonly ServiceDescriptor this[int index]
        {
            get
            {
                if (index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (index == 0)
                {
                    return _item!;
                }

                return _items![index - 1];
            }
        }

        public int GetSlot(ServiceDescriptor descriptor)
        {
            if (descriptor == _item)
            {
                return Count - 1;
            }

            if (_items != null)
            {
                var index = _items.IndexOf(descriptor);
                if (index != -1)
                {
                    return _items.Count - (index + 1);
                }
            }

            throw new InvalidOperationException("ServiceDescriptorNotExist");
        }

        public ServiceDescriptorCacheItem Add(ServiceDescriptor descriptor)
        {
            var newCacheItem = default(ServiceDescriptorCacheItem);
            if (_item == null)
            {
                Debug.Assert(_items == null);
                newCacheItem._item = descriptor;
            }
            else
            {
                newCacheItem._item = _item;
                newCacheItem._items = _items ?? new List<ServiceDescriptor>();
                newCacheItem._items.Add(descriptor);
            }
            return newCacheItem;
        }
    }
}