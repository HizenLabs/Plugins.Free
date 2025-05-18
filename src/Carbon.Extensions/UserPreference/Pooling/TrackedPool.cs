using Facepunch;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HizenLabs.Extensions.UserPreference.Pooling;

internal static class TrackedPool
{
    private static readonly ConcurrentDictionary<Type, TrackedPoolCollection> _trackedPools = new();

    public static IReadOnlyDictionary<Type, TrackedPoolCollection> TrackedPools => _trackedPools;

    public static int Transactions => AcquireCount + ReleaseCount;
    public static int AcquireCount;
    public static int ReleaseCount;

    public static int AllocatedCount => _trackedPools.Values.Sum(pool => pool.Count);

    public static T Get<T>() where T : class, ITrackedPooled, new()
    {
        Interlocked.Increment(ref AcquireCount);

        var trackedColledtion = (TrackedPoolCollection<T>)_trackedPools.GetOrAdd(typeof(T), _ => TrackedPoolCollection<T>.Instance);

        return trackedColledtion.Get();
    }

    public static void Free<T>(ref T obj) where T : class, ITrackedPooled, new()
    {
        Interlocked.Increment(ref ReleaseCount);

        var trackedColledtion = (TrackedPoolCollection<T>)_trackedPools.GetOrAdd(typeof(T), _ => TrackedPoolCollection<T>.Instance);

        trackedColledtion.Free(ref obj);
    }

    public static void FullReset()
    {
        foreach (var pool in _trackedPools)
        {
            pool.Value.FullReset();
        }

        Interlocked.Exchange(ref AcquireCount, 0);
        Interlocked.Exchange(ref ReleaseCount, 0);
    }
}
