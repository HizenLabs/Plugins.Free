namespace HizenLabs.FluentUI.Enums;

/// <summary>
/// Represents a fluent element's anchor points relative to its parent.
/// Each anchor point affects how position and size values are interpreted.
/// </summary>
public enum FluentAnchor
{
    /// <summary>
    /// Anchors to the top-left corner of the parent.
    /// <para>
    /// Position: X/Y are distances from top-left corner
    /// </para>
    /// <para>
    /// Size: Width/Height extend right and down from anchor
    /// </para>
    /// </summary>
    TopLeft,

    /// <summary>
    /// Anchors to the top edge, horizontally centered.
    /// <para>
    /// Position: X is horizontal offset from center (+ right, - left), Y is distance from top
    /// </para>
    /// <para>
    /// Size: Width extends equally left/right, Height extends down
    /// </para>
    /// </summary>
    TopCenter,

    /// <summary>
    /// Anchors to the top-right corner of the parent.
    /// <para>
    /// Position: X is distance from right edge (+ left, - right), Y is distance from top
    /// </para>
    /// <para>
    /// Size: Width extends left from anchor, Height extends down
    /// </para>
    /// </summary>
    TopRight,

    /// <summary>
    /// Anchors to the left edge, vertically centered.
    /// <para>
    /// Position: X is distance from left edge, Y is vertical offset from center (+ up, - down)
    /// </para>
    /// <para>
    /// Size: Width extends right, Height extends equally up/down
    /// </para>
    /// </summary>
    MiddleLeft,

    /// <summary>
    /// Anchors to the center of the parent.
    /// <para>
    /// Position: X/Y are offsets from center (X: + right/- left, Y: + up/- down)
    /// </para>
    /// <para>
    /// Size: Width/Height extend equally in all directions from center
    /// </para>
    /// </summary>
    Center,

    /// <summary>
    /// Anchors to the right edge, vertically centered.
    /// <para>
    /// Position: X is distance from right edge (+ left, - right), Y is vertical offset from center
    /// </para>
    /// <para>
    /// Size: Width extends left, Height extends equally up/down
    /// </para>
    /// </summary>
    MiddleRight,

    /// <summary>
    /// Anchors to the bottom-left corner of the parent.
    /// <para>
    /// Position: X is distance from left, Y is distance from bottom (+ up, - down)
    /// </para>
    /// <para>
    /// Size: Width extends right, Height extends up from anchor
    /// </para>
    /// </summary>
    BottomLeft,

    /// <summary>
    /// Anchors to the bottom edge, horizontally centered.
    /// <para>
    /// Position: X is horizontal offset from center, Y is distance from bottom
    /// </para>
    /// <para>
    /// Size: Width extends equally left/right, Height extends up
    /// </para>
    /// </summary>
    BottomCenter,

    /// <summary>
    /// Anchors to the bottom-right corner of the parent.
    /// <para>
    /// Position: X is distance from right, Y is distance from bottom
    /// </para>
    /// <para>
    /// Size: Width extends left, Height extends up from anchor
    /// </para>
    /// </summary>
    BottomRight,
}