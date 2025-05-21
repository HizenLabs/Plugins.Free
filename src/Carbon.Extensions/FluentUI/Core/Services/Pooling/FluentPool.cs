using Carbon.Components;
using Facepunch;
using HizenLabs.FluentUI.API.Interfaces;
using HizenLabs.FluentUI.Core.Elements;
using HizenLabs.FluentUI.Utils.Debug;
using HizenLabs.FluentUI.Utils.Delays;
using HizenLabs.FluentUI.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HizenLabs.FluentUI.Core.Services.Pooling;

/// <summary>
/// Wrapper that extends off the base <see cref="Pool"/> system.
/// We're using this to track pool calls during debug and handle custom elements.
/// </summary>
internal static class FluentPool
{
#if DEBUG
    private static Dictionary<Type, int> _pooledTypeCounts;

    internal static IReadOnlyDictionary<Type, int> PooledTypeCounts => _pooledTypeCounts;

    public static int PooledTypeTotal => _pooledTypeCounts.Sum(x => x.Value);
#endif

    public static void Initialize()
    {
#if DEBUG
        if (_pooledTypeCounts != null)
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Warning: Trying to initialize the pool when there are still {PooledTypeTotal} pooled items? Was init called twice?");
            return;
        }

        _pooledTypeCounts = Pool.Get<Dictionary<Type, int>>();
#endif
    }

    public static void Shutdown()
    {
#if DEBUG
        if (_pooledTypeCounts == null)
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Warning: Trying to shutdown the pool when it was never initialized?");
            return;
        }

        if (PooledTypeTotal > 0)
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Warning: There are still {PooledTypeTotal} pooled items that have not been freed!");
            debug.Log($"            These need to be freed properly before extension unload is finalized.");
            foreach (var kvp in _pooledTypeCounts)
            {
                if (kvp.Value > 0)
                {
                    debug.Log($"  - {kvp.Key.GetFriendlyTypeName()} still holding {kvp.Value} item(s)");
                }
            }
        }

        Pool.FreeUnmanaged(ref _pooledTypeCounts);
#endif
    }

    #region Pool.IPooled Wrappers

    public static T Get<T>() where T : class, new()
    {
        Push<T>();
        return Pool.Get<T>();
    }

    public static void Free<T>(ref T obj) 
        where T : class, Pool.IPooled, new()
    {
        Pop<T>();
        Pool.Free(ref obj);
    }

    public static void Free<T>(ref List<T> obj, bool freeElements = false) 
        where T : class, Pool.IPooled, new()
    {
        Pop<List<T>>();
        Pool.Free(ref obj, freeElements);
    }

    public static void FreeUnmanaged<T>(ref List<T> obj)
    {
        Pop<List<T>>();
        Pool.FreeUnmanaged(ref obj);
    }

    public static void FreeUnmanaged<TKey, TVal>(ref Dictionary<TKey, TVal> dict)
    {
        Pop<Dictionary<TKey, TVal>>();
        Pool.FreeUnmanaged(ref dict);
    }

    #endregion

    /// <summary>
    /// Creates a new instance of the specified <see cref="IFluentElement"/> type.
    /// </summary>
    /// <param name="elements">The list of elements to free from the pool.</param
    /// <exception cref="NotImplementedException">Thrown if any of the element types are not handled.</exception>
    public static void FreeCustom<T>(ref List<T> elements)
    {
        foreach (var obj in elements)
        {
            var obj2 = obj;
            FreeCustom(ref obj2);
        }

        FreeUnmanaged(ref elements);
    }

    public static void FreeCustom<T>(ref T element)
    {
        if (element is FluentContainer container)
        {
            Free(ref container);
        }
        else if (element is FluentPanel panel)
        {
            Free(ref panel);
        }
        else if (element is DelayedAction action)
        {
            Free(ref action);
        }
        else if (element is DelayedAction<CUI> actionCui)
        {
            Free(ref actionCui);
        }
        else if (element is DelayedAction<CUI, BasePlayer[]> actionPlayer)
        {
            Free(ref actionPlayer);
        }
        else
        {
            throw new InvalidOperationException($"Trying to free an element of type {typeof(T).GetFriendlyTypeName()} that is not handled by the pool.");
        }
    }

    private static void Push<T>()
    {
#if DEBUG
        if (!_pooledTypeCounts.ContainsKey(typeof(T)))
        {
            _pooledTypeCounts.Add(typeof(T), 0);
        }

        _pooledTypeCounts[typeof(T)]++;
#endif
    }

    private static void Pop<T>()
    {
#if DEBUG
        if (!_pooledTypeCounts.ContainsKey(typeof(T)))
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Error: Trying to free an item of type {typeof(T).GetFriendlyTypeName()} when tracked pool list was never initialized?");
        }
        else if (_pooledTypeCounts[typeof(T)] == 0)
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Error: Trying to free an item of type {typeof(T).GetFriendlyTypeName()} when tracked pool list is empty?");
        }
        else
        {
            _pooledTypeCounts[typeof(T)]--;
        }
#endif
    }
}
