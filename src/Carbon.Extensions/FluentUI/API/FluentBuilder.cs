using Carbon.Components;
using Carbon.Plugins;
using Facepunch;
using HizenLabs.FluentUI.API.Interfaces;
using HizenLabs.FluentUI.Core.Builders;
using HizenLabs.FluentUI.Core.Elements;
using HizenLabs.FluentUI.Core.Services;
using HizenLabs.FluentUI.Core.Services.Pooling;
using HizenLabs.FluentUI.Utils.Delays;
using HizenLabs.FluentUI.Utils.Extensions;
using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.API;

/// <summary>
/// A helper class to simplify and extend Carbon's CUI with fluent syntax.
/// Facilitates the creation and management of UI elements.
/// </summary>
public class FluentBuilder : IDisposable
{
    private readonly CarbonPlugin _plugin;
    private readonly string _containerId;
    private BuilderPool _pool;
    private FluentContainerBuilder _containerBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentBuilder"/> class.
    /// </summary>
    /// <param name="plugin">The plugin that is calling this builder instance.</param>
    /// <param name="containerId">
    /// The unique identifier for the builder. This is used as the primary container id.<br/>
    /// Manually call <see cref="CUI.Destroy(string, BasePlayer)"/> 
    /// using <paramref name="containerId"/> to remove the ui from the player.
    /// </param>
    private FluentBuilder(CarbonPlugin plugin, string containerId)
    {
        _plugin = plugin;
        _pool = new(plugin);
        _containerBuilder = FluentPool.Get<FluentContainerBuilder>();
        _containerId = containerId;
    }

    /// <summary>
    /// Creates a new UI builder associated with a plugin and container ID.
    /// Registers the container with the <see cref="ContainerManager"/>.
    /// </summary>
    /// <param name="plugin">The plugin that owns this UI.</param>
    /// <param name="id">The unique identifier for this UI container.</param>
    /// <returns>A new instance of <see cref="FluentBuilder"/>.</returns>
    public static FluentBuilder Create(CarbonPlugin plugin, string id)
    {
        ContainerManager.AddContainer(plugin, id);

        return new(plugin, id);
    }

    /// <summary>
    /// Creates a basic panel element.
    /// </summary>
    /// <param name="setupAction">The setup instructions for the panel.</param>
    /// <returns>The current instance of <see cref="FluentBuilder"/>.</returns>
    public FluentBuilder Panel(Action<IFluentPanelBuilder> setupAction)
    {
        _containerBuilder.Panel(setupAction);
        return this;
    }

    /// <summary>
    /// Builds and sends the UI to the specified players.
    /// </summary>
    /// <param name="players">The players to show the UI to.</param>
    /// <returns>The current instance of <see cref="FluentBuilder"/>.</returns>
    public FluentBuilder Show(params BasePlayer[] players)
    {
        var containerElement = _containerBuilder.Build(_containerId);
        _pool.BeginTracking(containerElement);

        var options = containerElement.Options;
        if (options.Delay > 0)
        {
            var delayed = _pool.CreateDelayedAction(options.Delay, () =>
            {
                if (options.Duration > 0)
                {
                    DestroyAfter(options.Duration, options.Id, players);
                }

                SendContainer(containerElement, players);
            });
            delayed.ExecuteTimer(_plugin);

            return this;
        }
        else
        {
            if (containerElement.Options.Duration > 0)
            {
                DestroyAfter(containerElement.Options.Duration, containerElement.Options.Id, players);
            }

            SendContainer(containerElement, players);

            return this;
        }
    }

    private void DestroyAfter(float seconds, string id, params BasePlayer[] players)
    {
        var delayed = _pool.CreateDelayedAction(seconds, () =>
        {
            using var cui = _plugin.CreateCUI();
            foreach (var player in players)
            {
                cui.Destroy(id, player);
            }
        });
        delayed.ExecuteTimer(_plugin);
    }

    /// <summary>
    /// Builds the cui and sends it to the player.
    /// </summary>
    /// <param name="containerElement">The container element to render.</param>
    /// <param name="players">The players to whom the UI will be sent.</param>
    /// <returns>The current instance of <see cref="FluentBuilder"/>.</returns>
    private void SendContainer(FluentContainer containerElement, params BasePlayer[] players)
    {
        using var cui = _plugin.CreateCUI();
        var delayedRenders = FluentPool.Get<List<DelayedAction<CUI>>>();
        var destroyActions = FluentPool.Get<List<DelayedAction<CUI, BasePlayer[]>>>();

        // Render the container
        var container = containerElement.Render(cui, delayedRenders, destroyActions);

        // Send to all players
        cui.SendAll(container, players);

        // Setup delayed renders
        for (int i = 0; i < delayedRenders.Count; i++)
        {
            var render = delayedRenders[i];
            var delayed = _pool.CreateDelayedAction(render.Delay, () =>
            {
                using var cui = _plugin.CreateCUI();
                render.Delay = 0;
                render.ExecuteTimer(_plugin, cui);

                // send all after render to ensure they are dispalyed
                cui.SendAll(container, players);
            });
            delayed.ExecuteTimer(_plugin);
        }

        // Setup pending destroys (menus with durations)
        for (int i = 0; i < destroyActions.Count; i++)
        {
            var destroy = destroyActions[i];
            var delayed = _pool.CreateDelayedAction(destroy.Delay, () =>
            {
                using var cui = _plugin.CreateCUI();
                destroy.Delay = 0;
                destroy.ExecuteTimer(_plugin, cui, players);
            });
            delayed.ExecuteTimer(_plugin);
        }

        // Do not free internals (must be unmanaged)
        // internal resources are manually freed in the timer callback
        // otherwise, they will be disposed of before they can be called
        FluentPool.FreeUnmanaged(ref delayedRenders);
        FluentPool.FreeUnmanaged(ref destroyActions);
    }

    /// <summary>
    /// Sets the duration for the UI to be displayed.
    /// </summary>
    /// <param name="seconds">The duration, in seconds, before displaying the UI.</param>
    /// <returns>The current instance of <see cref="FluentBuilder"/>.</returns>
    public FluentBuilder Delay(float seconds)
    {
        _containerBuilder.Delay(seconds);
        return this;
    }

    /// <summary>
    /// Sets the duration for the UI to be displayed.
    /// </summary>
    /// <param name="seconds">The duration, in seconds, before destroying the UI.</param>
    /// <returns>The current instance of <see cref="FluentBuilder"/>.</returns>
    public FluentBuilder Duration(float seconds)
    {
        _containerBuilder.Duration(seconds);
        return this;
    }

    public FluentBuilder Fade(float seconds) =>
        FadeIn(seconds).FadeOut(seconds);

    public FluentBuilder FadeIn(float seconds)
    {
        _containerBuilder.FadeIn(seconds);
        return this;
    }

    public FluentBuilder FadeOut(float seconds)
    {
        _containerBuilder.FadeOut(seconds);
        return this;
    }

    /// <summary>
    /// Sets the fade-in and fade-out duration for the UI.
    /// </summary>
    /// <param name="seconds">The duration, in seconds, for the popup to stay up for.</param>
    /// <param name="fadeIn">The duration, in seconds, for the fade-in effect.</param>
    /// <param name="fadeOut">The duration, in seconds, for the fade-out effect.</param>
    /// <returns>The current instance of <see cref="FluentBuilder"/>.</returns>
    public FluentBuilder AsPopup(float seconds, float fadeIn = 0.5f, float fadeOut = 0.5f)
    {
        _containerBuilder
            .Duration(seconds)
            .FadeIn(fadeIn)
            .FadeOut(fadeOut);

        return this;
    }

    /// <summary>
    /// Disposes the builder and frees all allocated resources.
    /// Returns element lists to the object pool.
    /// </summary>
    public void Dispose()
    {
        _pool?.Shutdown();
        _pool = null;

        FluentPool.Free(ref _containerBuilder);
    }
}
