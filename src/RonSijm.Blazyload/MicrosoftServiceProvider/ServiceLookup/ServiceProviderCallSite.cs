// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;

internal sealed class ServiceProviderCallSite : ServiceCallSite
{
    public ServiceProviderCallSite() : base(ResultCache.None)
    {
    }

    public override Type ServiceType { get; } = typeof(IServiceProvider);
    public override CallSiteKind Kind => CallSiteKind.ServiceProvider;
}