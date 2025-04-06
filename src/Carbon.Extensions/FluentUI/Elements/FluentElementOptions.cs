using Facepunch;
using HizenLabs.FluentUI.Abstractions;
using HizenLabs.FluentUI.Enums;
using HizenLabs.FluentUI.Primitives;
using UnityEngine;

namespace HizenLabs.FluentUI.Elements;

/// <inheritdoc cref="IFluentElementOptions"/>
/// <typeparam name="T">The type of the element.</typeparam>
internal class FluentElementOptions<T> : IFluentElementOptions
    where T : IFluentElement
{
    /// <inheritdoc/>
    public string Id { get; set; }

    /// <inheritdoc/>
    public FluentColor BackgroundColor { get; set; }

    /// <inheritdoc/>
    public FluentColor FontColor { get; set; }

    /// <inheritdoc/>
    public FluentArea Area => new(RelativePosition, RelativeSize, AbsolutePosition, AbsoluteSize, Anchor);

    /// <inheritdoc/>
    public Vector2 AbsolutePosition { get; set; }

    /// <inheritdoc/>
    public Vector2 AbsoluteSize { get; set; }

    /// <inheritdoc/>
    public Vector2 RelativePosition { get; set; }

    /// <inheritdoc/>
    public Vector2 RelativeSize { get; set; }

    /// <inheritdoc/>
    public FluentAnchor Anchor { get; set; }

    /// <inheritdoc/>
    public float FadeIn { get; set; }

    /// <inheritdoc/>
    public float FadeOut { get; set; }

    /// <inheritdoc/>
    public bool NeedsCursor { get; set; }

    /// <inheritdoc/>
    public bool NeedsKeyboard { get; set; }

    /// <inheritdoc/>
    public float Duration { get; set; }

    /// <inheritdoc/>
    public float Delay { get; set; }

    /// <summary>
    /// Resets the id of the element.
    /// </summary>
    public void EnterPool()
    {
        Id = null;
    }

    /// <summary>
    /// Resets the element's properties to their default values.
    /// </summary>
    public void LeavePool()
    {
        Anchor = FluentAnchor.TopLeft;

        AbsolutePosition = Vector2.zero;
        AbsoluteSize = Vector2.zero;

        RelativePosition = Vector2.zero;
        RelativeSize = Vector2.zero;

        BackgroundColor = FluentColor.Transparent;
        FontColor = FluentColor.Black;

        FadeIn = 0f;
        FadeOut = 0f;

        NeedsCursor = false;
        NeedsKeyboard = false;

        // base container is set to fullscreen
        if (typeof(T) == typeof(FluentContainer))
        {
            RelativeSize = Vector2.one;
        }
    }
}
