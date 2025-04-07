using Carbon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Reflection;
namespace HizenLabs.FluentUI.Internals;
/// <summary>
/// Simplify debug logging. All methods should be empty in release mode.
/// </summary>
internal class FluentDebug : IDisposable
{
    // Static configuration and state
#if DEBUG
    private static int callDepth = 0;
    private static readonly string indentChar = "  "; // Four spaces for each level of indentation
    private static readonly Stack<string> methodCallStack = new();
    private static readonly Stack<string> classNameStack = new();

    // Default to not showing file/line info
    private static bool showFileInfo = false;

    // Timeout for auto-reset (milliseconds)
    private static readonly TimeSpan autoResetTimeout = TimeSpan.FromMilliseconds(1000);
    private static DateTime lastLogTime = DateTime.Now;
#endif

    // Instance members for scope tracking
#if DEBUG
    private readonly string methodName;
    private readonly string className;
    private readonly string sourceFile;
    private readonly int sourceLine;
#endif

    // Private constructor - only created through BeginScope
    private FluentDebug(
        string scopeName,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
#if DEBUG
        // Check for auto-reset due to timeout
        DateTime currentTime = DateTime.Now;
        if ((currentTime - lastLogTime) > autoResetTimeout)
        {
            Reset();
        }
        lastLogTime = currentTime;

        // Store the context
        methodName = scopeName ?? memberName;
        className = GetCallerClassName();
        sourceFile = sourceFilePath;
        sourceLine = sourceLineNumber;

        string methodContext = $"{className}.{methodName}";

        methodCallStack.Push(methodContext);
        classNameStack.Push(className);

        string indent = GetIndentation();
        string fileInfo = showFileInfo ? $" [{System.IO.Path.GetFileName(sourceFile)}:{sourceLine}]" : "";
        Logger.Log($"{indent}[{methodContext}]{fileInfo}");

        callDepth++;
#endif
    }

    /// <summary>
    /// Begins a new logging scope.
    /// </summary>
    /// <param name="scopeName">Optional scope name. If not provided, the caller method name will be used</param>
    /// <returns>A FluentDebug instance for logging within this scope</returns>
    internal static FluentDebug BeginScope(
        string scopeName = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        return new FluentDebug(scopeName, memberName, sourceFilePath, sourceLineNumber);
    }

    /// <summary>
    /// Logs a message with indentation based on the current scope level.
    /// </summary>
    [Conditional("DEBUG")]
    internal void Log(string message)
    {
#if DEBUG
        // Update last log time
        lastLogTime = DateTime.Now;

        string indent = GetIndentation();
        string fileInfo = showFileInfo ? $" [{System.IO.Path.GetFileName(sourceFile)}:{sourceLine}]" : "";
        Logger.Log($"{indent}{message}{fileInfo}");
#endif
    }

    /// <summary>
    /// Logs a formatted message with indentation based on the current scope level.
    /// </summary>
    [Conditional("DEBUG")]
    internal void Log(string format, params object[] args)
    {
#if DEBUG
        // Evaluate any lazy arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is Func<object> argFunc)
            {
                args[i] = argFunc();
            }
        }

        // Update last log time
        lastLogTime = DateTime.Now;

        string indent = GetIndentation();
        string formattedMessage = string.Format(format, args);
        string fileInfo = showFileInfo ? $" [{System.IO.Path.GetFileName(sourceFile)}:{sourceLine}]" : "";
        Logger.Log($"{indent}{formattedMessage}{fileInfo}");
#endif
    }

    /// <summary>
    /// Disposes the debug scope, decreasing the indent level.
    /// </summary>
    public void Dispose()
    {
#if DEBUG
        if (methodCallStack.Count > 0)
        {
            string stackMethod = methodCallStack.Pop();
            classNameStack.Pop();

            callDepth = Math.Max(0, callDepth - 1);

            // Note: We don't log method exit in this style as per requirements
        }

        lastLogTime = DateTime.Now;
#endif
    }

    // Static utility methods

    /// <summary>
    /// Enables or disables showing file/line information in debug logs.
    /// </summary>
    [Conditional("DEBUG")]
    internal static void ShowFileInfo(bool enable)
    {
#if DEBUG
        showFileInfo = enable;
#endif
    }

    /// <summary>
    /// Manually resets the debugging state, clearing any tracked method calls.
    /// </summary>
    [Conditional("DEBUG")]
    internal static void Reset()
    {
#if DEBUG
        methodCallStack.Clear();
        classNameStack.Clear();
        callDepth = 0;
        lastLogTime = DateTime.Now;
#endif
    }

#if DEBUG
    /// <summary>
    /// Gets the current indentation string based on call depth.
    /// </summary>
    private static string GetIndentation()
    {
        return string.Concat(System.Linq.Enumerable.Repeat(indentChar, callDepth));
    }

    /// <summary>
    /// Gets the class name of the caller.
    /// </summary>
    private static string GetCallerClassName()
    {
        StackFrame frame = new(2, false); // Skip method and its immediate caller
        return frame.GetMethod()?.DeclaringType?.Name ?? "Unknown";
    }
#endif
}