using API.Events;
using System;

namespace HizenLabs.FluentUI.Utils.Extensions;

/// <summary>
/// Extension methods for <see cref="EventArgs"/> to extract payload data.
/// </summary>
internal static class EventArgsExtensions
{
    /// <summary>
    /// Attempts to extract a payload of type <typeparamref name="T"/> from the <see cref="EventArgs"/>.
    /// </summary>
    /// <typeparam name="T">The type of payload to extract.</typeparam>
    /// <param name="args">The event arguments to extract the payload from.</param>
    /// <param name="payload">When this method returns, contains the payload if found; otherwise, the default value of <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if a payload of type <typeparamref name="T"/> was found; otherwise, <c>false</c>.</returns>
    public static bool TryGetPayload<T>(this EventArgs args, out T payload)
    {
        if (args is CarbonEventArgs cArgs && cArgs.Payload is T result)
        {
            payload = result;
            return true;
        }

        payload = default;
        return false;
    }
}
