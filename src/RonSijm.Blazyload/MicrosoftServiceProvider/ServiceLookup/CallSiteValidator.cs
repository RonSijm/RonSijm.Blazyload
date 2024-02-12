// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;
internal sealed class CallSiteValidator : CallSiteVisitor<CallSiteValidator.CallSiteValidatorState, Type>
{
    // Keys are services being resolved via GetService, values - first scoped service in their call site tree
    private readonly ConcurrentDictionary<Type, Type> _scopedServices = new();
    public void ValidateCallSite(ServiceCallSite callSite)
    {
        var scoped = VisitCallSite(callSite, default);
        if (scoped != null)
        {
            _scopedServices[callSite.ServiceType] = scoped;
        }
    }

    public void ValidateResolution(Type serviceType, IServiceScope scope, IServiceScope rootScope)
    {
        if (ReferenceEquals(scope, rootScope) && _scopedServices.TryGetValue(serviceType, out var scopedService))
        {
            if (serviceType == scopedService)
            {
                throw new InvalidOperationException(StringResources.Format("DirectScopedResolvedFromRootException", serviceType, nameof(ServiceLifetime.Scoped).ToLowerInvariant()));
            }

            throw new InvalidOperationException(StringResources.Format("ScopedResolvedFromRootException", serviceType, scopedService, nameof(ServiceLifetime.Scoped).ToLowerInvariant()));
        }
    }

    protected override Type VisitConstructor(ConstructorCallSite constructorCallSite, CallSiteValidatorState state)
    {
        Type result = null;
        foreach (var parameterCallSite in constructorCallSite.ParameterCallSites)
        {
            var scoped = VisitCallSite(parameterCallSite, state);
            result ??= scoped;
        }

        return result;
    }

    protected override Type VisitIEnumerable(EnumerableCallSite enumerableCallSite, CallSiteValidatorState state)
    {
        Type result = null;
        foreach (var serviceCallSite in enumerableCallSite.ServiceCallSites)
        {
            var scoped = VisitCallSite(serviceCallSite, state);
            result ??= scoped;
        }

        return result;
    }

    protected override Type VisitRootCache(ServiceCallSite singletonCallSite, CallSiteValidatorState state)
    {
        state.Singleton = singletonCallSite;
        return VisitCallSiteMain(singletonCallSite, state);
    }

    protected override Type VisitScopeCache(ServiceCallSite scopedCallSite, CallSiteValidatorState state)
    {
        // We are fine with having ServiceScopeService requested by singletons
        if (scopedCallSite.ServiceType == typeof(IServiceScopeFactory))
        {
            return null;
        }

        if (state.Singleton != null)
        {
            throw new InvalidOperationException(StringResources.Format("ScopedInSingletonException", scopedCallSite.ServiceType, state.Singleton.ServiceType, nameof(ServiceLifetime.Scoped).ToLowerInvariant(), nameof(ServiceLifetime.Singleton).ToLowerInvariant()));
        }

        VisitCallSiteMain(scopedCallSite, state);
        return scopedCallSite.ServiceType;
    }

    protected override Type VisitConstant(ConstantCallSite constantCallSite, CallSiteValidatorState state) => null;
    protected override Type VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, CallSiteValidatorState state) => null;
    protected override Type VisitFactory(FactoryCallSite factoryCallSite, CallSiteValidatorState state) => null;
    internal struct CallSiteValidatorState
    {
        [DisallowNull]
        public ServiceCallSite Singleton { get; set; }
    }
}