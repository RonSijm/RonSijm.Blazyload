// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;

internal sealed class EnumerableCallSite : ServiceCallSite
{
    internal Type ItemType { get; }
    internal ServiceCallSite[] ServiceCallSites { get; }

    public EnumerableCallSite(ResultCache cache, Type itemType, ServiceCallSite[] serviceCallSites) : base(cache)
    {
        ItemType = itemType;
        ServiceCallSites = serviceCallSites;
    }

    public override Type ServiceType => typeof(IEnumerable<>).MakeGenericType(ItemType);
    public override CallSiteKind Kind => CallSiteKind.Enumerable;
}