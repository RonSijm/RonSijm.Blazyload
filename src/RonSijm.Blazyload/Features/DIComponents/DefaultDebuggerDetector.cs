using System.Diagnostics;

namespace RonSijm.Blazyload;

public class DefaultDebuggerDetector : IDebuggerDetector
{
    public bool IsAttached => Debugger.IsAttached;
}

