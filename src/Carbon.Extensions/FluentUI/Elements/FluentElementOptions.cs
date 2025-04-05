using Facepunch;
using HizenLabs.FluentUI.Abstractions;
using HizenLabs.FluentUI.Enums;
using HizenLabs.FluentUI.Primitives;
using UnityEngine;

namespace HizenLabs.FluentUI.Elements;

internal class FluentElementOptions<T> : Pool.IPooled
    where T : IFluentElement
{
    public string Id { get; set; }

    public FluentColor BackgroundColor { get; set; }

    public FluentColor FontColor { get; set; }

    public FluentArea Area => new(RelativePosition, RelativeSize, AbsolutePosition, AbsoluteSize, Anchor);

    public FluentAnchor Anchor { get; set; }

    public Vector2 AbsolutePosition { get; set; }

    public Vector2 AbsoluteSize { get; set; }

    public Vector2 RelativePosition { get; set; }

    public Vector2 RelativeSize { get; set; }

    public float FadeIn { get; set; }

    public float FadeOut { get; set; }

    public bool NeedsCursor { get; set; }

    public bool NeedsKeyboard { get; set; }

    public void EnterPool()
    {
        Id = null;
    }

    /// <summary>
    /// Resets the element's properties to their default values.
    /// The actual properties 
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
    }
}
