using API.Assembly;
using API.Events;
using Carbon;
using Carbon.Plugins;
using HizenLabs.FluentUI.Internals;
using System;

namespace HizenLabs.FluentUI;

/// <summary>
/// A helper plugin to simplify and extend Carbon's CUI with fluent syntax.
/// </summary>
[Info("FluentUI", "hizenxyz", "0.0.1")]
[Description("Helper library to extend CUI with fluent syntax.")]
[Hotloadable]
public class FluentUIEx : ICarbonExtension
{
    private static FluentUIEx _instance;
    private readonly FluentUIManager _manager = new();

    public void OnLoaded(EventArgs args)
    {
        _instance = this;
        Community.Runtime.Events.Subscribe(CarbonEvent.PluginUnloaded, OnPluginUnloaded);
    }

    public void Awake(EventArgs args) { }

    public void OnUnloaded(EventArgs args)
    {
        _manager.Dispose();
    }

    /// <summary>
    /// Disposes all UI containers when a plugin is unloaded.
    /// </summary>
    private void OnPluginUnloaded(EventArgs args)
    {
        if (args.TryGet<CarbonPlugin>(out var plugin))
        {
            _manager.Remove(plugin);
        }
    }
}
