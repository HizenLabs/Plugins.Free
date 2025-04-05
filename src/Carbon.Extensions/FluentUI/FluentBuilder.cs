using Carbon.Components;
using Carbon.Plugins;
using Epic.OnlineServices.IntegratedPlatform;
using Facepunch;
using HizenLabs.FluentUI.Abstractions;
using HizenLabs.FluentUI.Builders;
using HizenLabs.FluentUI.Elements;
using HizenLabs.FluentUI.Internals;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HizenLabs.FluentUI;

/// <summary>
/// A helper class to simplify and extend Carbon's CUI with fluent syntax.
/// Facilitates the creation and management of UI elements.
/// </summary>
public class FluentBuilder : IDisposable
{
    private readonly CarbonPlugin _plugin;
    private readonly string _containerId;
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
        _containerBuilder = Pool.Get<FluentContainerBuilder>();
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
        if (containerElement.Options.Delay > 0)
        {
            _plugin.timer.In(containerElement.Options.Delay, () =>
            {
                if (containerElement.Options.Duration > 0)
                {
                    DestroyAfter(containerElement.Options.Duration, containerElement.Options.Id, players);
                }

                SendContainer(containerElement, players);
            });

            return this;
        }
        else
        {
            SendContainer(containerElement, players);

            if (containerElement.Options.Duration > 0)
            {
                DestroyAfter(containerElement.Options.Duration, containerElement.Options.Id, players);
            }

            return this;
        }
    }

    private void DestroyAfter(float seconds, string id, params BasePlayer[] players)
    {
        _plugin.timer.In(seconds, () =>
        {
            using var cui = _plugin.CreateCUI();
            foreach (var player in players)
            {
                cui.Destroy(id, player);
            }
        });
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
        var delayedRenders = Pool.Get<List<DelayedAction<CUI>>>();
        var destroyActions = Pool.Get<List<DelayedAction<CUI, BasePlayer>>>();

        // Render the container
        var container = containerElement.Render(cui, delayedRenders, destroyActions);

        // Send to all players
        foreach (var player in players)
        {
            cui.Send(container, player);
        }

        // Sort the delayed renders by delay time
        delayedRenders.Sort((a, b) => a.Delay.CompareTo(b.Delay));

        // Process grouped actions with the same delay
        int index = 0;
        while (index < delayedRenders.Count)
        {
            float currentDelay = delayedRenders[index].Delay;
            int startIndex = index;

            // Find all actions with the same delay
            while (index < delayedRenders.Count && delayedRenders[index].Delay == currentDelay)
            {
                index++;
            }

            // Number of actions with this delay
            int count = index - startIndex;

            _plugin.Puts($"Delaying {count} for {currentDelay} seconds.");

            // Capture the range values to ensure they're not modified in the closure
            int capturedStart = startIndex;
            int capturedCount = count;

            // Schedule these actions
            _plugin.timer.In(currentDelay, () =>
            {
                using var delayCui = _plugin.CreateCUI();

                // Execute all actions with the same delay
                for (int i = capturedStart; i < capturedStart + capturedCount; i++)
                {
                    _plugin.Puts($"Executing action (delay was {currentDelay}s)");
                    delayedRenders[i].Action(delayCui);
                }
            });
        }

        // Do not free internals (must be unmanaged)
        // internal resources are manually freed in the timer callback
        // otherwise, they will be disposed of before they can be called
        Pool.FreeUnmanaged(ref delayedRenders);
        Pool.FreeUnmanaged(ref destroyActions);
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
        Pool.Free(ref _containerBuilder);
    }
}
