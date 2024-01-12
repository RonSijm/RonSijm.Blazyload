// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;

internal sealed class FactoryCallSite : ServiceCallSite
{
    public Func<IServiceProvider, object> Factory { get; }

    public FactoryCallSite(ResultCache cache, Type serviceType, Func<IServiceProvider, object> factory) : base(cache)
    {
        Factory = factory;
        ServiceType = serviceType;
    }

    public override Type ServiceType { get; }

    public override CallSiteKind Kind => CallSiteKind.Factory;
}