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
[Info("FluentUI", "hizenxyz", "0.0.3")]
[Description("Helper library to extend CUI with fluent syntax.")]
[Hotloadable]
public class FluentUIEx : ICarbonExtension
{
    private static FluentUIEx _instance;

    /// <summary>
    /// Called when the extension is loaded. Initializes managers and subscribes to events.
    /// </summary>
    /// <param name="args">OnLoaded event arguments.</param>
    public void OnLoaded(EventArgs args)
    {
        _instance = this;

        ContainerManager.Initialize(_instance);

        Community.Runtime.Events.Subscribe(CarbonEvent.PluginUnloaded, OnPluginUnloaded);
    }

    /// <summary>
    /// Called when the extension is awakened (not used).
    /// </summary>
    /// <param name="args">Awake event arguments.</param>
    public void Awake(EventArgs args) { }

    public void OnUnloaded(EventArgs args)
    {
        Community.Runtime.Events.Unsubscribe(CarbonEvent.PluginUnloaded, OnPluginUnloaded);

        ContainerManager.ShutDown(_instance);
    }

    /// <summary>
    /// Called when a plugin is unloaded. Removes it from the manager, closing out any active resources.
    /// </summary>
    /// <param name="args">An instance of <see cref="CarbonEventArgs"/> with a <see cref="CarbonPlugin"/> payload.</param>
    private void OnPluginUnloaded(EventArgs args)
    {
        if (args.TryGetPayload<CarbonPlugin>(out var plugin))
        {
            ContainerManager.RemovePlugin(plugin);
        }
    }
}
