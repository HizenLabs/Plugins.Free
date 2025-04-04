using Carbon.Components;
using Facepunch;
using HizenLabs.FluentUI.Elements;
using HizenLabs.FluentUI.Internals;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.Abstractions;

/// <summary>
/// Represents a basic UI element in the Fluent UI system.<br />
/// Implements pooling to optimize resource allocation.
/// </summary>
internal abstract class FluentElement<T> : IFluentElement
    where T : FluentElement<T>, new()
{
    private FluentElementOptions<T> _options;
    private List<IFluentElement> _children;

    internal FluentElementOptions<T> Options => _options;

    /// <summary>
    /// Adds a child element to this element.
    /// </summary>
    /// <param name="child">The child element to add.</param>
    public void AddElement(IFluentElement child) =>
        _children.Add(child);

    /// <summary>
    /// Renders the element and its children to the input container with the specified parent.
    /// </summary>
    /// <param name="cui">The CUI instance to use for rendering.</param>
    /// <param name="container">The base container.</param>
    /// <param name="parent">The parent element name.</param>
    public void Render(CUI cui, CuiElementContainer container, string parent)
    {
        var elementId = $"{parent}.{Options.Id}";
        RenderElement(cui, container, parent, elementId);

        if (_children != null)
        {
            foreach (var child in _children)
            {
                child.Render(cui, container, elementId);
            }
        }
    }

    /// <summary>
    /// Renders just this element to the container with the specified parent and id.
    /// </summary>
    /// <param name="cui">The CUI instance to use for rendering.</param>
    /// <param name="container">The container to render to.</param>
    /// <param name="parent">The parent element name.</param>
    /// <param name="elementId">The id of the element.</param>
    protected abstract void RenderElement(CUI cui, CuiElementContainer container, string parent, string elementId);

    /// <summary>
    /// Prepares the element for return to the object pool.
    /// </summary>
    public void EnterPool()
    {
        Pool.Free(ref _options);
        PoolHelper.FreeElements(ref _children);
    }

    /// <summary>
    /// Initializes the element when retrieved from the object pool.
    /// </summary>
    public void LeavePool()
    {
        _options = Pool.Get<FluentElementOptions<T>>();
        _children = Pool.Get<List<IFluentElement>>();
    }
}
