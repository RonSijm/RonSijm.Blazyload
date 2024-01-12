// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;

internal sealed class CallSiteChain
{
    private readonly Dictionary<Type, ChainItemInfo> _callSiteChain = new();

    public void CheckCircularDependency(Type serviceType)
    {
        if (_callSiteChain.ContainsKey(serviceType))
        {
            throw new InvalidOperationException(CreateCircularDependencyExceptionMessage(serviceType));
        }
    }

    public void Remove(Type serviceType)
    {
        _callSiteChain.Remove(serviceType);
    }

    public void Add(Type serviceType, Type implementationType = null)
    {
        _callSiteChain[serviceType] = new ChainItemInfo(_callSiteChain.Count, implementationType);
    }

    private string CreateCircularDependencyExceptionMessage(Type type)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder.Append(StringResources.Format("CircularDependencyException", TypeNameHelper.GetTypeDisplayName(type)));
        messageBuilder.AppendLine();

        AppendResolutionPath(messageBuilder, type);

        return messageBuilder.ToString();
    }

    private void AppendResolutionPath(StringBuilder builder, Type currentlyResolving)
    {
        var ordered = new List<KeyValuePair<Type, ChainItemInfo>>(_callSiteChain);
        ordered.Sort((a, b) => a.Value.Order.CompareTo(b.Value.Order));

        foreach (var pair in ordered)
        {
            var serviceType = pair.Key;
            var implementationType = pair.Value.ImplementationType;
            if (implementationType == null || serviceType == implementationType)
            {
                builder.Append(TypeNameHelper.GetTypeDisplayName(serviceType));
            }
            else
            {
                builder.AppendFormat("{0}({1})",
                    TypeNameHelper.GetTypeDisplayName(serviceType),
                    TypeNameHelper.GetTypeDisplayName(implementationType));
            }

            builder.Append(" -> ");
        }

        builder.Append(TypeNameHelper.GetTypeDisplayName(currentlyResolving));
    }

    private readonly struct ChainItemInfo(int order, Type implementationType)
    {
        public int Order { get; } = order;
        public Type ImplementationType { get; } = implementationType;
    }
}