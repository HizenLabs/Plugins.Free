using Facepunch;
using Oxide.Core;
using System;
using System.Diagnostics;

namespace HizenLabs.Extensions.UserPreference.Pooling;

internal abstract class TrackedPoolCollection
{
    public abstract int Count { get; }

    public bool IsEmpty => Count == 0;
}

internal class TrackedPoolCollection<T> : TrackedPoolCollection
    where T : class, ITrackedPooled, new()
{
    private readonly ConcurrentHashSet<Guid> _tracking;
    private static TrackedPoolCollection<T> _instance;
    private static readonly object syncRoot = new();

    public override int Count => Instance?._tracking.Count ?? 0;

    public static TrackedPoolCollection<T> Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    _instance ??= new();
                }
            }
            return _instance;
        }
    }

    private TrackedPoolCollection()
    {
        _tracking = new();
    }

    public T Get()
    {
        var obj = Pool.Get<T>();

        if (obj.TrackingId != Guid.Empty)
        {
            Debug.WriteLine($"Warning: Attempting to get an object that is already tracked. Type: {typeof(T)}");
        }

        obj.TrackingId = Guid.NewGuid();
        Instance._tracking.Add(obj.TrackingId);

        return obj;
    }

    public void Free(ref T obj)
    {
        if (obj.TrackingId == Guid.Empty)
        {
            Debug.WriteLine($"Warning: Attempting to free an object that was never tracked. Type: {typeof(T)}");
        }
        else
        {
            Instance._tracking.Remove(obj.TrackingId);
            obj.TrackingId = Guid.Empty;
        }

        Pool.Free(ref obj);
    }
}
