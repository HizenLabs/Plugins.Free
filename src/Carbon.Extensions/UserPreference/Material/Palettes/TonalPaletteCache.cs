using HizenLabs.Extensions.UserPreference.Pooling;
using System;
using System.Collections.Generic;

namespace HizenLabs.Extensions.UserPreference.Material.Palettes;

internal class TonalPaletteCache : Dictionary<int, int>, IDisposable, ITrackedPooled
{
    public Guid TrackingId { get; set; }

    public void Dispose()
    {
        var obj = this;
        TrackedPool.Free(ref obj);
    }

    public void EnterPool()
    {
        Clear();
    }

    public void LeavePool()
    {
    }
}
