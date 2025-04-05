using Facepunch;
using System;

namespace HizenLabs.FluentUI.Internals;

internal class DelayedAction<T> : Pool.IPooled
{
    public Action<T> Action { get; set; }

    public float Delay { get; set; }

    public void EnterPool()
    {
        Delay = 0f;
        Action = null;
    }

    public void LeavePool()
    {
    }
}
