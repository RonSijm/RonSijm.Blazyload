namespace RonSijm.Blazyload;

/// <summary>
/// Default implementation of IBlazyLogger that writes to Console.
/// </summary>
public class ConsoleLogger : IBlazyLogger
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteLine(Exception exception)
    {
        Console.WriteLine(exception);
    }
}

