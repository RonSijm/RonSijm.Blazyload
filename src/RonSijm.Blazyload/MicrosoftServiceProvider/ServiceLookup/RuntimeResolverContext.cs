namespace RonSijm.Blazyload.MicrosoftServiceProvider.ServiceLookup;

internal struct RuntimeResolverContext
{
    public ServiceProviderEngineScope Scope { get; set; }

    public RuntimeResolverLock AcquiredLocks { get; set; }
}