using Carbon.Plugins;
using System;

namespace HizenLabs.FluentUI.Internals;

internal class PluginHandle : IDisposable
{
    private readonly CarbonPlugin _pluginRef;

    public PluginHandle(CarbonPlugin pluginRef)
    {
        _pluginRef = pluginRef;
    }

    public void Dispose()
    {

    }
}
