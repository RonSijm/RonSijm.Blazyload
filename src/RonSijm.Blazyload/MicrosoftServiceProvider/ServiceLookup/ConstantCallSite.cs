// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;

internal sealed class ConstantCallSite : ServiceCallSite
{
    private readonly Type _serviceType;
    internal object DefaultValue => Value;

    public ConstantCallSite(Type serviceType, object defaultValue): base(ResultCache.None)
    {
        _serviceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        if (defaultValue != null && !serviceType.IsInstanceOfType(defaultValue))
        {
            throw new ArgumentException(StringResources.Format("ConstantCantBeConvertedToServiceType", defaultValue.GetType(), serviceType));
        }

        Value = defaultValue;
    }

    public override Type ServiceType => _serviceType;
    public override CallSiteKind Kind => CallSiteKind.Constant;
}