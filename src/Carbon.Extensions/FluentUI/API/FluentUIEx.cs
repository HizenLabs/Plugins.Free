using API.Assembly;
using API.Events;
using Carbon;
using Carbon.Core;
using Carbon.Plugins;
using HizenLabs.FluentUI.Core.Services;
using HizenLabs.FluentUI.Core.Services.Pooling;
using HizenLabs.FluentUI.Utils.Debug;
using HizenLabs.FluentUI.Utils.Extensions;
using Oxide.Plugins;
using System;

namespace HizenLabs.FluentUI.API;

/// <summary>
/// A helper plugin to simplify and extend Carbon's CUI with fluent syntax.
/// </summary>
[Info("FluentUI", "hizenxyz", "0.0.7")]
[Description("Helper library to extend CUI with fluent syntax.")]
[Hotloadable]
public class FluentUIEx : ICarbonExtension
{
    private static FluentUIEx _instance;
    private static RustPlugin _debugPlugin;

    /// <summary>
    /// Called when the extension is awakened (not used).
    /// </summary>
    /// <param name="args">Awake event arguments.</param>
    public void Awake(EventArgs args) { }

    /// <summary>
    /// Called when the extension is loaded. Initializes managers and subscribes to events.
    /// </summary>
    /// <param name="args">OnLoaded event arguments.</param>
    public void OnLoaded(EventArgs args)
    {
        _instance = this;

        FluentPool.Initialize();
        ContainerManager.Initialize(_instance);

#if DEBUG
        Community.Runtime.Events.Subscribe(CarbonEvent.AllPluginsLoaded, OnAllPluginsLoaded);
#endif
        Community.Runtime.Events.Subscribe(CarbonEvent.PluginUnloaded, OnPluginUnloaded);

    }

    private void OnAllPluginsLoaded(EventArgs args)
    {
#if DEBUG
        using var debug = FluentDebug.BeginScope();
        ModLoader.InitializePlugin(typeof(DebugFluentUIPlugin), out _debugPlugin, precompiled: true);
        debug.Log($"DebugFluentUIPlugin loaded, compiled in: {_debugPlugin.CompileTime}");
#endif
    }

    /// <summary>
    /// Called when the extension is unloaded. Cleans up resources and unsubscribes from events.
    /// </summary>
    /// <param name="args">OnUnloaded event arguments.</param>
    public void OnUnloaded(EventArgs args)
    {
#if DEBUG
        if (_debugPlugin != null && _debugPlugin.HasInitialized)
        {
            ModLoader.UninitializePlugin(_debugPlugin);
        }
#endif

        Community.Runtime.Events.Unsubscribe(CarbonEvent.PluginUnloaded, OnPluginUnloaded);

        ContainerManager.Shutdown(_instance);
        FluentPool.Shutdown();
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
