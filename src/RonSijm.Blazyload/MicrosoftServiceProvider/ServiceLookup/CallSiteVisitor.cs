// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;

internal abstract class CallSiteVisitor<TArgument, TResult>
{
    private readonly StackGuard _stackGuard;

    protected CallSiteVisitor()
    {
        _stackGuard = new StackGuard();
    }

    protected virtual TResult VisitCallSite(ServiceCallSite callSite, TArgument argument)
    {
        if (!_stackGuard.TryEnterOnCurrentStack())
        {
            return _stackGuard.RunOnEmptyStack(VisitCallSite, callSite, argument);
        }

        switch (callSite.Cache.Location)
        {
            case CallSiteResultCacheLocation.Root:
                return VisitRootCache(callSite, argument);
            case CallSiteResultCacheLocation.Scope:
                return VisitScopeCache(callSite, argument);
            case CallSiteResultCacheLocation.Dispose:
                return VisitDisposeCache(callSite, argument);
            case CallSiteResultCacheLocation.None:
                return VisitNoCache(callSite, argument);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected virtual TResult VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
    {
        switch (callSite.Kind)
        {
            case CallSiteKind.Factory:
                return VisitFactory((FactoryCallSite)callSite, argument);
            case  CallSiteKind.Enumerable:
                return VisitIEnumerable((EnumerableCallSite)callSite, argument);
            case CallSiteKind.Constructor:
                return VisitConstructor((ConstructorCallSite)callSite, argument);
            case CallSiteKind.Constant:
                return VisitConstant((ConstantCallSite)callSite, argument);
            case CallSiteKind.ServiceProvider:
                return VisitServiceProvider((ServiceProviderCallSite)callSite, argument);
            default:
                throw new NotSupportedException(StringResources.Format("CallSiteTypeNotSupported", callSite.GetType()));
        }
    }

    protected virtual TResult VisitNoCache(ServiceCallSite callSite, TArgument argument)
    {
        return VisitCallSiteMain(callSite, argument);
    }

    protected virtual TResult VisitDisposeCache(ServiceCallSite callSite, TArgument argument)
    {
        return VisitCallSiteMain(callSite, argument);
    }

    protected virtual TResult VisitRootCache(ServiceCallSite callSite, TArgument argument)
    {
        return VisitCallSiteMain(callSite, argument);
    }

    protected virtual TResult VisitScopeCache(ServiceCallSite callSite, TArgument argument)
    {
        return VisitCallSiteMain(callSite, argument);
    }

    protected abstract TResult VisitConstructor(ConstructorCallSite constructorCallSite, TArgument argument);

    protected abstract TResult VisitConstant(ConstantCallSite constantCallSite, TArgument argument);

    protected abstract TResult VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, TArgument argument);

    protected abstract TResult VisitIEnumerable(EnumerableCallSite enumerableCallSite, TArgument argument);

    protected abstract TResult VisitFactory(FactoryCallSite factoryCallSite, TArgument argument);
}