﻿using Carbon.Components;
using Facepunch;
using HizenLabs.FluentUI.Elements;
using HizenLabs.FluentUI.Internals;
using Oxide.Game.Rust.Cui;
using System;
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
    /// <param name="index">The index of the element in the parent render tree.</param>
    /// <param name="delayedRenders">The list of delayed renders. Appends to this if there is a delay.</param>
    public void Render(
        CUI cui, 
        CuiElementContainer container, 
        string parent, 
        int index, 
        List<DelayedAction<CUI>> delayedRenders, 
        float delayOffset,
        List<DelayedAction<CUI, BasePlayer>> destroyActions
    )
    {
        // Force parent elements to render first to make delays relative.
        Options.Delay += delayOffset;

        var elementId = $"{parent}.{Options.Id ?? typeof(T).Name}[{index}]";

        if (_options.Delay > 0)
        {
            var delayedRender = Pool.Get<DelayedAction<CUI>>();
            delayedRender.Delay = _options.Delay;
            delayedRender.Action = actionCui =>
            {
                RenderElement(actionCui, container, parent, elementId);
                Pool.Free(ref delayedRender);
            };

            delayedRenders.Add(delayedRender);
        }
        else
        {
            RenderElement(cui, container, parent, elementId);
        }

        if (_options.Duration > 0)
        {
            var destroyAction = Pool.Get<DelayedAction<CUI, BasePlayer>>();
            destroyAction.Delay = _options.Duration;
            destroyAction.Action = (actionCui, player) =>
            {
                cui.Destroy(elementId, player);
                Pool.Free(ref destroyAction);
            };
        }

        RenderChildren(cui, container, elementId, delayedRenders, Options.Delay, destroyActions);
    }

    private void DestroyAfter(float seconds, CUI cui)
    {

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
        string parent, 
        List<DelayedAction<CUI>> delayedRenders, 
        float delayOffset,
        List<DelayedAction<CUI, BasePlayer>> destroyActions
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
