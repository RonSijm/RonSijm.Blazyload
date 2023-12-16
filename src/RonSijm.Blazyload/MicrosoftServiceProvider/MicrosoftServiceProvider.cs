// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;

namespace RonSijm.Blazyload.MicrosoftServiceProvider
{
    public sealed class MicrosoftServiceProvider : IServiceProvider, IDisposable, IAsyncDisposable
    {
        private readonly CallSiteValidator _callSiteValidator;

        private readonly Func<Type, Func<ServiceProviderEngineScope, object>> _createServiceAccessor;

        // Internal for testing
        private readonly ServiceProviderEngine _engine;

        private bool _disposed;

        private readonly ConcurrentDictionary<Type, Func<ServiceProviderEngineScope, object>> _realizedServices;

        private CallSiteFactory CallSiteFactory { get; }

        internal ServiceProviderEngineScope Root { get; }

        internal static bool VerifyOpenGenericServiceTrimmability { get; } = AppContext.TryGetSwitch("Microsoft.Extensions.DependencyInjection.VerifyOpenGenericServiceTrimmability", out var verifyOpenGenerics) && verifyOpenGenerics;

        public MicrosoftServiceProvider(ICollection<ServiceDescriptor> serviceDescriptors, ServiceProviderOptions options, IBlazyInternalServiceProvider internalServiceProvider)
        {
            // note that Root needs to be set before calling GetEngine(), because the engine may need to access Root
            Root = new ServiceProviderEngineScope(this, isRootScope: true);
            _engine = GetEngine();
            _createServiceAccessor = CreateServiceAccessor;
            _realizedServices = new ConcurrentDictionary<Type, Func<ServiceProviderEngineScope, object>>();

            CallSiteFactory = new CallSiteFactory(serviceDescriptors, internalServiceProvider);
            // The list of built in services that aren't part of the list of service descriptors
            // keep this in sync with CallSiteFactory.IsService
            CallSiteFactory.Add(typeof(IServiceProvider), new ServiceProviderCallSite());
            CallSiteFactory.Add(typeof(IServiceScopeFactory), new ConstantCallSite(typeof(IServiceScopeFactory), Root));
            CallSiteFactory.Add(typeof(IServiceProviderIsService), new ConstantCallSite(typeof(IServiceProviderIsService), CallSiteFactory));

            if (options.ValidateScopes)
            {
                _callSiteValidator = new CallSiteValidator();
            }

            if (options.ValidateOnBuild)
            {
                List<Exception> exceptions = null;
                foreach (var serviceDescriptor in serviceDescriptors)
                {
                    try
                    {
                        ValidateService(serviceDescriptor);
                    }
                    catch (Exception e)
                    {
                        exceptions ??= new List<Exception>();
                        exceptions.Add(e);
                    }
                }

                if (exceptions != null)
                {
                    throw new AggregateException("Some services are not able to be constructed", exceptions.ToArray());
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>The service that was produced.</returns>
        public object GetService(Type serviceType) => GetService(serviceType, Root);

        internal bool IsDisposed() => _disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            DisposeCore();
            Root.Dispose();
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            DisposeCore();
            return Root.DisposeAsync();
        }

        private void DisposeCore()
        {
            _disposed = true;
        }

        private void OnCreate(ServiceCallSite callSite)
        {
            _callSiteValidator?.ValidateCallSite(callSite);
        }

        private void OnResolve(Type serviceType, IServiceScope scope)
        {
            _callSiteValidator?.ValidateResolution(serviceType, scope, Root);
        }

        internal object GetService(Type serviceType, ServiceProviderEngineScope serviceProviderEngineScope)
        {
            if (_disposed)
            {
                ThrowHelper.ThrowObjectDisposedException();
            }

            var realizedService = _realizedServices.GetOrAdd(serviceType, _createServiceAccessor);
            OnResolve(serviceType, serviceProviderEngineScope);

            var result = realizedService.Invoke(serviceProviderEngineScope);
            //System.Diagnostics.Debug.Assert(result is null || CallSiteFactory.IsService(serviceType));
            return result;
        }

        private void ValidateService(ServiceDescriptor descriptor)
        {
            if (descriptor.ServiceType.IsGenericType && !descriptor.ServiceType.IsConstructedGenericType)
            {
                return;
            }

            try
            {
                var callSite = CallSiteFactory.GetCallSite(descriptor, new CallSiteChain());
                if (callSite != null)
                {
                    OnCreate(callSite);
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error while validating the service descriptor '{descriptor}': {e.Message}", e);
            }
        }

        private Func<ServiceProviderEngineScope, object> CreateServiceAccessor(Type serviceType)
        {
            var callSite = CallSiteFactory.GetCallSite(serviceType, new CallSiteChain());
            if (callSite != null)
            {
                OnCreate(callSite);

                // Optimize singleton case
                if (callSite.Cache.Location == CallSiteResultCacheLocation.Root)
                {
                    var value = CallSiteRuntimeResolver.Instance.Resolve(callSite, Root);
                    return _ => value;
                }

                return _engine.RealizeService(callSite);
            }

            return _ => null;
        }

        internal IServiceScope CreateScope()
        {
            if (_disposed)
            {
                ThrowHelper.ThrowObjectDisposedException();
            }

            return new ServiceProviderEngineScope(this, isRootScope: false);
        }

        private ServiceProviderEngine GetEngine()
        {

            return RuntimeServiceProviderEngine.Instance;
        }
    }
}
