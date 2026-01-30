namespace RonSijm.Blazyload;

/// <summary>
/// Interface for loading assemblies from streams.
/// Allows mocking assembly loading in tests.
/// </summary>
public interface IAssemblyLoadContext
{
    /// <summary>
    /// Loads an assembly from a stream.
    /// </summary>
    /// <param name="assembly">The stream containing the assembly bytes.</param>
    /// <param name="assemblySymbols">Optional stream containing the symbol (PDB) bytes.</param>
    /// <returns>The loaded assembly.</returns>
    Assembly LoadFromStream(Stream assembly, Stream assemblySymbols = null);
}

