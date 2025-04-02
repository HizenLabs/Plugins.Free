using Carbon.Plugins;
using Facepunch;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HizenLabs.FluentUI.Internals;

internal class PluginHandle : IDisposable
{
    private readonly CarbonPlugin _pluginRef;
    private List<string> _containerIds;

    public PluginHandle(CarbonPlugin pluginRef)
    {
        _pluginRef = pluginRef;
        _containerIds = Pool.Get<List<string>>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RegisterContainerId(string containerId)
    {
        if (!_containerIds.Contains(containerId))
        {
            _containerIds.Add(containerId);
        }
    }

    public void Dispose()
    {
        if (_containerIds is null or { Count: 0 })
            return;

        var players = BasePlayer.activePlayerList;
        if (players.Count > 0)
        {
            using var cui = _pluginRef.CreateCUI();

            foreach (var player in players)
            {
                foreach (var id in _containerIds)
                {
                    cui.Destroy(id, player);
                }
            }
        }

        Pool.FreeUnmanaged(ref _containerIds);
    }
}
