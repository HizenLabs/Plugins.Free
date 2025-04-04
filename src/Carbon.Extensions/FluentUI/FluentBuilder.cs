using Carbon.Plugins;
using Facepunch;
using HizenLabs.FluentUI.Abstractions;
using HizenLabs.FluentUI.Builders;
using HizenLabs.FluentUI.Internals;
using System;

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

    private float _duration = 0f;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentBuilder"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the UI container.</param>
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

    public FluentBuilder Panel(Action<IFluentPanelBuilder> setupAction)
    {
        _containerBuilder.Panel(setupAction);
        return this;
    }

    /// <summary>
    /// Builds the cui and sends it to the player.
    /// </summary>
    /// <param name="players">The players to whom the UI will be sent.</param>
    /// <returns>The current instance of <see cref="FluentBuilder"/>.</returns>
    public FluentBuilder Show(params BasePlayer[] players)
    {
        using var cui = _plugin.CreateCUI();
        var container = _containerBuilder
            .Build(_containerId)
            .Render(cui);

        foreach (var player in players)
        {
            cui.Send(container, player);
        }
        
        if (_duration > 0)
        {
            _plugin.timer.In(_duration, () =>
            {
                using var timerCui = _plugin.CreateCUI();
                foreach (var player in players)
                {
                    timerCui.Destroy(_containerId, player);
                }
            });
        }

        return this;
    }

    public FluentBuilder Duration(int seconds) =>
        Duration((float)seconds);

    public FluentBuilder Duration(float seconds)
    {
        _duration = seconds;
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
