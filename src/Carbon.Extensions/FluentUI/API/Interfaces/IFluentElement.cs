using Carbon.Components;
using Facepunch;
using HizenLabs.FluentUI.Core.Elements.Base;
using HizenLabs.FluentUI.Utils.Delays;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.API.Interfaces;

/// <summary>
/// Represents a basic UI element in the Fluent UI system.
/// </summary>
public interface IFluentElement : Pool.IPooled
{
    /// <summary>
    /// The options used for rendering this element.
    /// </summary>
    internal IFluentElementOptions Options { get; }

    /// <summary>
    /// Renders the element with the given parameters and present <see cref="Options"/>
    /// </summary>
    /// <param name="cui">The CUI instance to use for rendering.</param>
    /// <param name="container">The primary container to attach the element to.</param>
    /// <param name="parent">The parent element id.</param>
    /// <param name="index">The index of the element in the parent. Used to determine the current element id.</param>
    /// <param name="delayedRenders">List of delayed render actions.</param>
    /// <param name="delayOffset">The delay offset for rendering.</param>
    /// <param name="destroyActions">List of destroy actions.</param>
    internal void Render(
        CUI cui,
        CuiElementContainer container,
        IFluentElement parent,
        int index,
        List<DelayedAction<CUI>> delayedRenders,
        float delayOffset,
        List<DelayedAction<CUI, BasePlayer[]>> destroyActions
    );
}
