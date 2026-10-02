namespace RonSijm.Blazyload;

public sealed class BlazyAssemblyLoadException(string assemblyName, Exception innerException) : Exception($"Failed loading assembly '{assemblyName}'.", innerException)
{
}
