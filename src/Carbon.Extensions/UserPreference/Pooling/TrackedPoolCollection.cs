using Facepunch;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HizenLabs.Extensions.UserPreference.Pooling;

#if DEBUG
internal abstract class TrackedPoolCollection
{
    public IReadOnlyDictionary<Guid, string> AllocationStacks => _allocationStacks;
    protected readonly Dictionary<Guid, string> _allocationStacks = new();

    public IReadOnlyList<Guid> Tracking => _tracking.ToList();
    protected readonly ConcurrentHashSet<Guid> _tracking = new();

    public abstract int Count { get; }

    public bool IsEmpty => Count == 0;

    public IReadOnlyDictionary<Guid, long> AllocationIndex => _allocationIndex;
    protected readonly Dictionary<Guid, long> _allocationIndex = new();

    protected long NextIndex = 0;

    public void FullReset()
    {
        _allocationIndex.Clear();
        _allocationStacks.Clear();
        _tracking.Clear();
        NextIndex = 0;
    }
}

internal class TrackedPoolCollection<T> : TrackedPoolCollection
    where T : class, ITrackedPooled, new()
{

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

    private TrackedPoolCollection() { }

    public T Get()
    {
        var obj = Pool.Get<T>();

        if (obj.TrackingId != Guid.Empty)
        {
            throw new InvalidOperationException($"Attempting to get an object that is already tracked. Type: {typeof(T)}");
        }

        obj.TrackingId = Guid.NewGuid();
        Instance._tracking.Add(obj.TrackingId);

        _allocationStacks[obj.TrackingId] = Environment.StackTrace;
        _allocationIndex[obj.TrackingId] = NextIndex++;

        return obj;
    }

    public void Free(ref T obj)
    {
        if (obj.TrackingId == Guid.Empty)
        {
            throw new InvalidOperationException($"Attempting to free an object that was never tracked. Type: {typeof(T)}");
        }

        Instance._tracking.Remove(obj.TrackingId);
        obj.TrackingId = Guid.Empty;

        Pool.Free(ref obj);
    }
}
#endif