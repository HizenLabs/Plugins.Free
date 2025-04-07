using Carbon.Components;
using Facepunch;
using HizenLabs.FluentUI.API.Interfaces;
using HizenLabs.FluentUI.Core.Elements;
using HizenLabs.FluentUI.Utils.Debug;
using HizenLabs.FluentUI.Utils.Delays;
using HizenLabs.FluentUI.Utils.Delays.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using static Facepunch.Pool;

namespace HizenLabs.FluentUI.Core.Services.Pooling;

/// <summary>
/// Wrapper that extends off the base <see cref="Pool"/> system.
/// We're using this to track pool calls during debug and handle custom elements.
/// </summary>
internal static class FluentPool
{
#if DEBUG
    private static Dictionary<Type, int> _pooledItemCount;
#endif

    public static int PooledItemCount => _pooledItemCount.Sum(x => x.Value);

    public static void Init()
    {
        if (_pooledItemCount != null)
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Warning: Trying to initialize the pool when there are still {PooledItemCount} pooled items? Was init called twice?");
            return;
        }

        _pooledItemCount = Pool.Get<Dictionary<Type, int>>();
    }

    public static void Shutdown()
    {
        if (_pooledItemCount == null)
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Warning: Trying to shutdown the pool when it was never initialized?");
            return;
        }

        if (PooledItemCount > 0)
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Warning: There are still {PooledItemCount} pooled items that have not been freed!");
            debug.Log($"            These need to be freed properly before extension unload is finalized.");
            foreach (var kvp in _pooledItemCount)
            {
                if (kvp.Value > 0)
                {
                    debug.Log($"  - {kvp.Key} still holding {kvp.Value} item(s)");
                }
            }
        }

        Pool.FreeUnmanaged(ref _pooledItemCount);
    }

    #region Pool.IPooled Wrappers

    public static T Get<T>() where T : class, new()
    {
        Push<T>();
        return Pool.Get<T>();
    }

    public static void Free<T>(ref T obj) where T : class, IPooled, new()
    {
        Pop<T>();
        Pool.Free(ref obj);
    }

    public static void Free<T>(ref List<T> obj, bool freeElements = false) where T : class, IPooled, new()
    {
        Pop<List<T>>();
        Pool.Free(ref obj, freeElements);
    }

    public static void FreeUnmanaged<T>(ref List<T> obj)
    {
        Pop<List<T>>();
        Pool.FreeUnmanaged(ref obj);
    }

    #endregion

    /// <summary>
    /// Helper method to free unknown types into the pool.
    /// This only works with types that we support as we switch through the list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    public static void FreeUnknown<T>(ref T obj) where T : IPooled
    {
        throw new NotImplementedException();
    }

    public static void FreeUnknown<T>(ref List<T> obj) where T : IPooled
    {
        throw new NotImplementedException();
    }

    private static void Push<T>()
    {
#if DEBUG
        if (!_pooledItemCount.ContainsKey(typeof(T)))
        {
            _pooledItemCount.Add(typeof(T), 0);
        }

        _pooledItemCount[typeof(T)]++;
#endif
    }

    private static void Pop<T>()
    {
#if DEBUG
        if (!_pooledItemCount.ContainsKey(typeof(T)))
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Error: Trying to free an item of type {typeof(T)} when tracked pool list was never initialized?");
        }
        else if (_pooledItemCount[typeof(T)] == 0)
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Error: Trying to free an item of type {typeof(T)} when tracked pool list is empty?");
        }
        else
        {
            _pooledItemCount[typeof(T)]--;
        }
#endif
    }

    /// <summary>
    /// Creates a new instance of the specified <see cref="IFluentElement"/> type.
    /// </summary>
    /// <param name="elements">The list of elements to free from the pool.</param
    /// <exception cref="NotImplementedException">Thrown if any of the element types are not handled.</exception>
    public static void FreeElements(ref List<IFluentElement> elements)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Frees the specified list of delayed actions from the pool.
    /// </summary>
    /// <param name="actions"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void FreeActions(ref List<IDelayedAction> actions)
    {
        throw new NotImplementedException();
    }
}
