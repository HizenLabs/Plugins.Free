using API.Events;
using System;

namespace HizenLabs.FluentUI;

public static class EventArgsExtensions
{
    public static bool TryGet<T>(this EventArgs args, out T payload)
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
