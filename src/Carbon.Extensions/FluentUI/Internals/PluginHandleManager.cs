using Carbon.Plugins;
using HizenLabs.FluentUI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HizenLabs.FluentUI.Internals;

/// <summary>
/// Manager to handle all ui resources, ensuring proper disposal on completion.
/// </summary>
internal class PluginHandleManager : IDisposable
{
    private static PluginHandleManager _instance;
    private readonly Dictionary<int, PluginHandle> _handles = new();

    private PluginHandleManager()
    {
        _instance = this;
    }

    public static PluginHandle GetHandle(CarbonPlugin plugin) =>
        _instance._handles.GetOrAdd(plugin.ResourceId, () => new(plugin));

    public static void Remove(CarbonPlugin plugin) =>
        Remove(plugin.ResourceId);

    public static void Remove(int resourceId)
    {
        if (_instance._handles.TryRemove(resourceId, out var handle))
        {
            handle.Dispose();
        }
    }

    public static void ShutDown() =>
        _instance.Dispose();

    public void Dispose()
    {
        foreach (var resourceId in _handles.Keys.ToArray())
        {
            Remove(resourceId);
        }
    }
}
