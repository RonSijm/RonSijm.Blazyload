// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;

internal sealed class ConstructorCallSite : ServiceCallSite
{
    internal ConstructorInfo ConstructorInfo { get; }
    internal ServiceCallSite[] ParameterCallSites { get; }

    public ConstructorCallSite(ResultCache cache, Type serviceType, ConstructorInfo constructorInfo) : this(cache, serviceType, constructorInfo, Array.Empty<ServiceCallSite>())
    {
    }

    public ConstructorCallSite(ResultCache cache, Type serviceType, ConstructorInfo constructorInfo, ServiceCallSite[] parameterCallSites) : base(cache)
    {
        if (!serviceType.IsAssignableFrom(constructorInfo.DeclaringType))
        {
            throw new ArgumentException(StringResources.Format("ImplementationTypeCantBeConvertedToServiceType", constructorInfo.DeclaringType, serviceType));
        }

        ServiceType = serviceType;
        ConstructorInfo = constructorInfo;
        ParameterCallSites = parameterCallSites;
    }

    public override Type ServiceType { get; }

    public override CallSiteKind Kind => CallSiteKind.Constructor;
}