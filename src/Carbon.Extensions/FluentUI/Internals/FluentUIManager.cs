using Carbon.Plugins;
using HizenLabs.FluentUI.Extensions;
using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.Internals;

/// <summary>
/// Manager to handle all ui resources, ensuring proper disposal on completion.
/// </summary>
internal class FluentUIManager : IDisposable
{
    private readonly Dictionary<int, PluginHandle> _handles = new();

    public PluginHandle GetHandle(CarbonPlugin plugin)
    {
        if (!_handles.TryGetValue(plugin.ResourceId, out var handle))
        {

        }

        return handle;
    }

    public void Remove(CarbonPlugin plugin)
    {
        if (_handles.TryRemove(plugin.ResourceId, out var handle))
        {

        }
    }

    public void Dispose()
    {

    }
}
