using Facepunch;
using HizenLabs.FluentUI.Enums;
using HizenLabs.FluentUI.Primitives;
using UnityEngine;

namespace HizenLabs.FluentUI.Abstractions;

/// <summary>
/// Options for <see cref="IFluentElementBuilder{TElement, TBuilder}"/> implementations.
/// </summary>
/// <remarks>
/// Not all options are used in every implementation!
/// </remarks>
internal interface IFluentElementOptions : Pool.IPooled
{
    /// <summary>
    /// The unique identifier for this element.
    /// The CUI system uses this to attach elements to their parents as well as in disposal
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// The background color of the element.
    /// </summary>
    /// <remarks>
    /// At time of documenting, this does not inherit.
    /// </remarks>
    FluentColor BackgroundColor { get; set; }

    /// <summary>
    /// The color to use for any text in the element.
    /// </summary>
    /// <remarks>
    /// At time of documenting, this does not inherit.
    /// </remarks>
    FluentColor FontColor { get; set; }

    /// <summary>
    /// Contains all of the calculated xy values for the element.
    /// </summary>
    FluentArea Area { get; }

    /// <summary>
    /// The absolute offset, in pixels, from the parent container.
    /// </summary>
    Vector2 AbsolutePosition { get; set; }

    /// <summary>
    /// The absolute size, in pixels, of the element.
    /// </summary>
    Vector2 AbsoluteSize { get; set; }

    /// <summary>
    /// The position of the element relative to its parent container.
    /// </summary>
    /// <remarks>
    /// Must be a floating point value between 0 and 1 (percentage).
    /// </remarks>
    Vector2 RelativePosition { get; set; }

    /// <summary>
    /// The size of the element relative to its parent container.
    /// </summary>
    /// <remarks>
    /// Must be a floating point value between 0 and 1 (percentage).
    /// </remarks>
    Vector2 RelativeSize { get; set; }

    /// <summary>
    /// The point to anchor the element. See documentation for each of the values.
    /// </summary>
    /// <remarks>
    /// Changing this point updates the behavior of all other position and size values.
    /// </remarks>
    FluentAnchor Anchor { get; set; }

    /// <summary>
    /// The duration, in seconds, that the element should take to fade in on creation.
    /// </summary>
    float FadeIn { get; set; }

    /// <summary>
    /// The duration, in seconds, that the element should take to fade out on disposal.
    /// </summary>
    float FadeOut { get; set; }

    /// <summary>
    /// If set to true, cursor input will be captured on the UI until it is closed.
    /// </summary>
    bool NeedsCursor { get; set; }

    /// <summary>
    /// If set to true, keyboard input will be captured on the UI until it is closed.
    /// </summary>
    bool NeedsKeyboard { get; set; }

    /// <summary>
    /// The duration, in seconds, that the element will be displayed before being automatically disposed.
    /// </summary>
    /// <remarks>
    /// This can be used for displaying temporary windows, alerts, etc.
    /// If this is not specified, UI must be destroyed manually.
    /// </remarks>
    float Duration { get; set; }

    /// <summary>
    /// The delay, in seconds, before the element is displayed.
    /// </summary>
    /// <remarks>
    /// If the parent menu is destroyed before this delay is over, the element will not be displayed.
    /// </remarks>
    float Delay { get; set; }
}
