using Facepunch;
using System;

namespace HizenLabs.FluentUI.Internals;

internal class DelayedAction<T1, T2> : Pool.IPooled
{
    public Action<T1, T2> Action { get; set; }

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
