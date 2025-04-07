using Carbon.Plugins;
using Facepunch;
using HizenLabs.FluentUI.Extensions;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.Internals;

/// <summary>
/// Manages UI containers for plugins that use the FluentUI system.
/// Provides functionality for adding, tracking, and removing UI containers.
/// </summary>
internal class ContainerManager : IDisposable
{
    /// <summary>
    /// Singleton instance of the <see cref="ContainerManager"/> class.
    /// </summary>
    private static ContainerManager _instance;

    /// <summary>
    /// The instance of the <see cref="FluentUIEx"/> extension.
    /// </summary>
    private readonly FluentUIEx _extension;

    /// <summary>
    /// Contains all UI containers for each plugin that uses FluentUI.
    /// </summary>
    private Dictionary<long, List<string>> _pluginContainers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerManager"/> class.
    /// </summary>
    /// <param name="extension">The instance of the <see cref="FluentUIEx"/> extension.</param>
    private ContainerManager(FluentUIEx extension)
    {
        _extension = extension;
        _pluginContainers = Pool.Get<Dictionary<long, List<string>>>();
    }

    /// <summary>
    /// Initializes the ContainerManager.
    /// </summary>
    /// <remarks>
    /// Expected to be called from the <see cref="FluentUIEx.OnLoaded(EventArgs)"/> event.<br />
    /// Running this method multiple times will dispose the previous instance and create a new one.
    /// </remarks>
    /// <param name="extension">The instance of the <see cref="FluentUIEx"/> extension.</param>
    internal static void Initialize(FluentUIEx extension)
    {
        using var _ = FluentDebug.BeginScope();

        if (_instance != null)
        {
            if (_instance._extension == extension)
                return;

            _instance.Dispose();
        }

        _instance = new(extension);
    }

    /// <summary>
    /// Adds a UI container to the list of containers for a specific plugin.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <param name="name">The name of the container.</param>
    internal static void AddContainer(CarbonPlugin plugin, string name) =>
        _instance.AddContainer(plugin.ResourceId, name);

    /// <summary>
    /// Adds a UI container to the list of containers for a specific plugin.
    /// </summary>
    /// <param name="resourceId">The plugin id.</param>
    /// <param name="name">The name of the container.</param>
    private void AddContainer(long resourceId, string name)
    {
        using var debug = FluentDebug.BeginScope();
        debug.Log($"Adding container");
        debug.Log($"  Resource: {resourceId}");
        debug.Log($"  Container: {name}");

        var list = GetContainers(resourceId);

        if (!list.Contains(name))
        {
            list.Add(name);
        }
    }

    /// <summary>
    /// Retrieves the list of UI containers for a specific plugin.
    /// </summary>
    /// <param name="resourceId">The plugin id.</param>
    /// <returns>A list of container names.</returns>
    private List<string> GetContainers(long resourceId) =>
        _pluginContainers.GetOrAdd(resourceId, () => Pool.Get<List<string>>());

    /// <summary>
    /// Removes a UI container from the list of containers for a specific plugin.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    internal static void RemovePlugin(CarbonPlugin plugin) =>
        _instance.RemovePlugin(plugin.ResourceId);

    /// <summary>
    /// Removes all UI containers associated with a specific plugin.
    /// </summary>
    /// <param name="resourceId">The id of the plugin resource.</param>
    private void RemovePlugin(long resourceId)
    {
        using var debug = FluentDebug.BeginScope();

        if (!_pluginContainers.TryRemove(resourceId, out var containers))
        {
            debug.Log($"[{nameof(ContainerManager)}] Remove plugin failed. Resource not found: {resourceId}");
            return;
        }

        debug.Log($"Resource: {resourceId}");
        debug.Log($"Found {containers.Count} container(s)");

        foreach (var name in containers)
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                debug.Log($"  Destroying container {name} for player {player.UserIDString}");
                CuiHelper.DestroyUi(player, name);
            }
        }

        Pool.FreeUnmanaged(ref containers);
    }

    /// <summary>
    /// Initiates the shutdown process for the ContainerManager.
    /// </summary>
    /// <param name="instance">The FluentUIEx instance requesting shutdown.</param>
    /// <remarks>
    /// Validates that the current extension calling shutdown is the active handle
    /// before disposing resources.
    /// </remarks>
    internal static void ShutDown(FluentUIEx instance)
    {
        using var debug = FluentDebug.BeginScope();

        // Validate that the current extension calling shutdown is the active handle.
        if (_instance == null || _instance._extension != instance)
        {
            debug.Log($"!! Shutdown failed. Instance not found or mismatched.");
            return;
        }

        _instance.Dispose();
    }

    /// <summary>
    /// Disposal removes all registered UI containers for all active plugins (that are using FluentUI).
    /// </summary>
    public void Dispose()
    {
        using var debug = FluentDebug.BeginScope();

        if (_pluginContainers != null)
        {
            debug.Log($"Disposing {_pluginContainers.Count} container(s)");
            foreach (var resourceId in _pluginContainers.Keys)
            {
                RemovePlugin(resourceId);
            }

            Pool.FreeUnmanaged(ref _pluginContainers);
        }
        else
        {
            debug.Log($"{nameof(_pluginContainers)} is already null? Was dispose called too many times?");
        }

        if (_instance != null)
        {
            debug.Log($"Disposing instance");
            _instance = null;
        }
        else
        {
            debug.Log($"{nameof(_instance)} is already null? Was dispose called too many times?");
        }
    }
}
