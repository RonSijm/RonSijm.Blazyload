// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;

/// <summary>
/// Summary description for ServiceCallSite
/// </summary>
internal abstract class ServiceCallSite
{
    protected ServiceCallSite(ResultCache cache)
    {
        Cache = cache;
    }

    public abstract Type ServiceType { get; }
    public abstract CallSiteKind Kind { get; }
    public ResultCache Cache { get; }
    public object Value { get; set; }
}