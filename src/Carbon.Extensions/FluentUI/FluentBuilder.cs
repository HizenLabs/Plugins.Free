using Carbon.Plugins;
using Facepunch;
using HizenLabs.FluentUI.Elements;
using HizenLabs.FluentUI.Managers;
using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI;

/// <summary>
/// A helper class to simplify and extend Carbon's CUI with fluent syntax.
/// Facilitates the creation and management of UI elements.
/// </summary>
public class FluentBuilder : IDisposable
{
    private readonly string _id;

    private List<FluentElement> _elements;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentBuilder"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the UI container.</param>
    private FluentBuilder(string id)
    {
        _id = id;
        _elements = Pool.Get<List<FluentElement>>();
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
        return new(id);
    }

    /// <summary>
    /// Disposes the builder and frees all allocated resources.
    /// Returns element lists to the object pool.
    /// </summary>
    public void Dispose()
    {
        if (_elements != null)
        {
            Pool.Free(ref _elements, true);
        }
    }
}
