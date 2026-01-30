namespace RonSijm.Blazyload;

/// <summary>
/// Interface for logging in Blazyload.
/// Allows mocking logging in tests.
/// </summary>
public interface IBlazyLogger
{
    /// <summary>
    /// Writes a message to the log.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void WriteLine(string message);

    /// <summary>
    /// Writes an exception to the log.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    void WriteLine(Exception exception);
}

