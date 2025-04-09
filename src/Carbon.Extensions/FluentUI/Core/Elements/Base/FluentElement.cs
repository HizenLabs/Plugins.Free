using Carbon.Components;
using HizenLabs.FluentUI.API.Interfaces;
using HizenLabs.FluentUI.Core.Services.Pooling;
using HizenLabs.FluentUI.Utils.Debug;
using HizenLabs.FluentUI.Utils.Delays;
using HizenLabs.FluentUI.Utils.Extensions;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.Core.Elements.Base;

/// <summary>
/// Represents a basic UI element in the Fluent UI system.<br />
/// Implements pooling to optimize resource allocation.
/// </summary>
internal abstract class FluentElement<T> : IFluentElement
    where T : FluentElement<T>, new()
{
    private FluentElementOptions<T> _options;
    private List<IFluentElement> _children;

    /// <summary>
    /// The current rendering options for the element.
    /// </summary>
    public IFluentElementOptions Options => _options;

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
    /// <param name="index">The index of the element in the parent render tree.</param>
    /// <param name="delayedRenders">The list of delayed renders. Appends to this if there is a delay.</param>
    public void Render(
        CUI cui,
        CuiElementContainer container,
        IFluentElement parent,
        int index,
        List<DelayedAction<CUI>> delayedRenders,
        float delayOffset,
        List<DelayedAction<CUI, BasePlayer[]>> destroyActions
    )
    {
        // Force parent elements to render first to make delays relative.
        Options.Delay += delayOffset;

        Options.Id ??= Guid.NewGuid().ToString();
        var elementId = $"{parent.Options.Id}[{index}]:{Options.Id}";

        Options.ContainerId ??= parent.Options.ContainerId;

        if (_options.Delay > 0)
        {
            var delayedRender = FluentPool.Get<DelayedAction<CUI>>();
            delayedRender.Delay = _options.Delay;
            delayedRender.Action = actionCui => RenderElement(actionCui, container, parent, elementId, Options.ContainerId);
            delayedRenders.Add(delayedRender);
        }
        else
        {
            RenderElement(cui, container, parent, elementId);
        }

        if (_options.Duration > 0)
        {
            var destroyAction = FluentPool.Get<DelayedAction<CUI, BasePlayer[]>>();
            destroyAction.Delay = _options.Duration;
            destroyAction.Action = (actionCui, players) => cui.DestroyAll(Options.ContainerId, players);
        }

        RenderChildren(cui, container, this, delayedRenders, Options.Delay, destroyActions);
    }

    /// <summary>
    /// Renders the children of this element to the input container with the specified parent.
    /// </summary>
    /// <param name="cui">The CUI instance to use for rendering.</param>
    /// <param name="container">The base container.</param>
    /// <param name="parent">The parent element name.</param>
    /// <param name="delayedRenders">The list of delayed renders.</param>
    protected void RenderChildren(
        CUI cui,
        CuiElementContainer container,
        IFluentElement parent,
        List<DelayedAction<CUI>> delayedRenders,
        float delayOffset,
        List<DelayedAction<CUI, BasePlayer[]>> destroyActions
    )
    {
        if (_children != null)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Render(cui, container, parent, i, delayedRenders, delayOffset, destroyActions);
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
    /// <param name="destroyUi">The optional id of the ui to destroy on rendering this element.</param>
    protected abstract void RenderElement(CUI cui, CuiElementContainer container, IFluentElement parent, string elementId, string destroyUi = null);

    /// <summary>
    /// Prepares the element for return to the object pool.
    /// </summary>
    public void EnterPool()
    {
        FluentPool.Free(ref _options);
        FluentPool.FreeCustom(ref _children);
    }

    /// <summary>
    /// Initializes the element when retrieved from the object pool.
    /// </summary>
    public void LeavePool()
    {
        if (_options?.Id != null)
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Warning: Trying to leave the pool when the element is already initialized? {Options.Id}");
            return;
        }

        _options = FluentPool.Get<FluentElementOptions<T>>();
        _children = FluentPool.Get<List<IFluentElement>>();
    }
}
