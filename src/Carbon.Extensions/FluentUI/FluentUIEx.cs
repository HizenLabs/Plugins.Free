using API.Assembly;
using API.Events;
using Carbon;
using Carbon.Plugins;
using HizenLabs.FluentUI.Managers;
using System;

namespace HizenLabs.FluentUI;

/// <summary>
/// A helper plugin to simplify and extend Carbon's CUI with fluent syntax.
/// </summary>
[Info("FluentUI", "hizenxyz", "0.0.3")]
[Description("Helper library to extend CUI with fluent syntax.")]
[Hotloadable]
public class FluentUIEx : ICarbonExtension
{
    private static FluentUIEx _instance;

    public void OnLoaded(EventArgs args)
    {
        _instance = this;

        ContainerManager.Initialize(_instance);

        Community.Runtime.Events.Subscribe(CarbonEvent.PluginUnloaded, OnPluginUnloaded);
    }

    public void Awake(EventArgs args) { }

    public void OnUnloaded(EventArgs args)
    {
        Community.Runtime.Events.Unsubscribe(CarbonEvent.PluginUnloaded, OnPluginUnloaded);

        ContainerManager.ShutDown(_instance);
    }

    /// <summary>
    /// Disposes all UI containers when a plugin is unloaded.
    /// </summary>
    private void OnPluginUnloaded(EventArgs args)
    {
        if (args.TryGet<CarbonPlugin>(out var plugin))
        {
            ContainerManager.RemovePlugin(plugin);
        }
    }
}
