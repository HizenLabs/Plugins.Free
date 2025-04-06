using Carbon;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HizenLabs.FluentUI.Internals;

/// <summary>
/// Simplify debug logging. All methods should be empty in release mode.
/// </summary>
internal static class FluentDebug
{
    [Conditional("DEBUG")]
    internal static void Log(string message, params object[] args)
    {
#if DEBUG
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is Func<object> argFunc)
            {
                args[i] = argFunc();
            }
        }

        Logger.Debug(string.Format(message, args));
#endif
    }
}
