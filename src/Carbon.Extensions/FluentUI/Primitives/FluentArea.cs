using HizenLabs.FluentUI.Enums;
using System;
using UnityEngine;

namespace HizenLabs.FluentUI.Primitives;

#pragma warning disable IDE1006 // Naming Styles

/// <summary>
/// Represents a rectangular area for positioning elements in the Fluent UI system.
/// Uses a coordinate system with origin at the bottom-left.
/// </summary>
public readonly struct FluentArea
{
    public static readonly FluentArea Empty = Absolute(0, 0, 0, 0);
    public static readonly FluentArea FullScreen = Relative(0, 0, 1, 1);

    /// <summary>Gets the minimum X coordinate (left edge) in relative units (0-1).</summary>
    public float xMin { get; }

    /// <summary>Gets the maximum X coordinate (right edge) in relative units (0-1).</summary>
    public float xMax { get; }

    /// <summary>Gets the minimum Y coordinate (bottom edge) in relative units (0-1).</summary>
    public float yMin { get; }

    /// <summary>Gets the maximum Y coordinate (top edge) in relative units (0-1).</summary>
    public float yMax { get; }

    /// <summary>Gets the minimum X coordinate (left edge) in absolute pixels.</summary>
    public float OxMin { get; }

    /// <summary>Gets the maximum X coordinate (right edge) in absolute pixels.</summary>
    public float OxMax { get; }

    /// <summary>Gets the minimum Y coordinate (bottom edge) in absolute pixels.</summary>
    public float OyMin { get; }

    /// <summary>Gets the maximum Y coordinate (top edge) in absolute pixels.</summary>
    public float OyMax { get; }

    /// <summary>
    /// Creates a new FluentArea with both absolute and relative values.
    /// </summary>
    /// <param name="relX">The X position as a proportion (0.0-1.0).</param>
    /// <param name="relY">The Y position as a proportion (0.0-1.0).</param>
    /// <param name="relWidth">The width as a proportion (0.0-1.0).</param>
    /// <param name="relHeight">The height as a proportion (0.0-1.0).</param>
    /// <param name="absX">The X position in pixels.</param>
    /// <param name="absY">The Y position in pixels.</param>
    /// <param name="absWidth">The width in pixels.</param>
    /// <param name="absHeight">The height in pixels.</param>
    /// <param name="anchor">The anchor pofloat.</param>
    public FluentArea(
        float relX,
        float relY,
        float relWidth,
        float relHeight,
        float absX,
        float absY,
        float absWidth,
        float absHeight,
        FluentAnchor anchor = FluentAnchor.TopLeft
    )
    {
        // Clamp relative values to 0-1 range
        relX = Mathf.Clamp(relX, 0f, 1f);
        relY = Mathf.Clamp(relY, 0f, 1f);
        relWidth = Mathf.Clamp(relWidth, 0f, 1f);
        relHeight = Mathf.Clamp(relHeight, 0f, 1f);

        // Pre-calculate all coordinates based on anchor pofloat
        // For a bottom-left origin system, Y increases upward
        switch (anchor)
        {
            case FluentAnchor.TopLeft:
                // In bottom-left origin: TopLeft means X=0, Y=1-height
                xMin = relX;
                yMin = 1f - relY - relHeight;
                xMax = relX + relWidth;
                yMax = 1f - relY;

                OxMin = absX;
                OyMin = -absY - absHeight;
                OxMax = absX + absWidth;
                OyMax = -absY;
                break;

            case FluentAnchor.TopCenter:
                // In bottom-left origin: TopCenter means X=0.5, Y=1-height
                xMin = 0.5f + relX - relWidth / 2;
                yMin = 1f - relY - relHeight;
                xMax = 0.5f + relX + relWidth / 2;
                yMax = 1f - relY;

                OxMin = absX - absWidth / 2;
                OyMin = -absY - absHeight;
                OxMax = absX + absWidth / 2;
                OyMax = -absY;
                break;

            case FluentAnchor.TopRight:
                // In bottom-left origin: TopRight means X=1-width, Y=1-height
                xMin = 1f - relX - relWidth;
                yMin = 1f - relY - relHeight;
                xMax = 1f - relX;
                yMax = 1f - relY;

                OxMin = -absX - absWidth;
                OyMin = -absY - absHeight;
                OxMax = -absX;
                OyMax = -absY;
                break;

            case FluentAnchor.MiddleLeft:
                // In bottom-left origin: MiddleLeft means X=0, Y=0.5
                xMin = relX;
                yMin = 0.5f + relY - relHeight / 2;
                xMax = relX + relWidth;
                yMax = 0.5f + relY + relHeight / 2;

                OxMin = absX;
                OyMin = absY - absHeight / 2;
                OxMax = absX + absWidth;
                OyMax = absY + absHeight / 2;
                break;

            case FluentAnchor.Center:
                // In bottom-left origin: Center means X=0.5, Y=0.5
                xMin = 0.5f + relX - relWidth / 2;
                yMin = 0.5f + relY - relHeight / 2;
                xMax = 0.5f + relX + relWidth / 2;
                yMax = 0.5f + relY + relHeight / 2;

                OxMin = absX - absWidth / 2;
                OyMin = absY - absHeight / 2;
                OxMax = absX + absWidth / 2;
                OyMax = absY + absHeight / 2;
                break;

            case FluentAnchor.MiddleRight:
                // In bottom-left origin: MiddleRight means X=1-width, Y=0.5
                xMin = 1f - relX - relWidth;
                yMin = 0.5f + relY - relHeight / 2;
                xMax = 1f - relX;
                yMax = 0.5f + relY + relHeight / 2;

                OxMin = -absX - absWidth;
                OyMin = absY - absHeight / 2;
                OxMax = -absX;
                OyMax = absY + absHeight / 2;
                break;

            case FluentAnchor.BottomLeft:
                // In bottom-left origin: BottomLeft means X=0, Y=0
                xMin = relX;
                yMin = relY;
                xMax = relX + relWidth;
                yMax = relY + relHeight;

                OxMin = absX;
                OyMin = absY;
                OxMax = absX + absWidth;
                OyMax = absY + absHeight;
                break;

            case FluentAnchor.BottomCenter:
                // In bottom-left origin: BottomCenter means X=0.5, Y=0
                xMin = 0.5f + relX - relWidth / 2;
                yMin = relY;
                xMax = 0.5f + relX + relWidth / 2;
                yMax = relY + relHeight;

                OxMin = absX - absWidth / 2;
                OyMin = absY;
                OxMax = absX + absWidth / 2;
                OyMax = absY + absHeight;
                break;

            case FluentAnchor.BottomRight:
                // In bottom-left origin: BottomRight means X=1-width, Y=0
                xMin = 1f - relX - relWidth;
                yMin = relY;
                xMax = 1f - relX;
                yMax = relY + relHeight;

                OxMin = -absX - absWidth;
                OyMin = absY;
                OxMax = -absX;
                OyMax = absY + absHeight;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(anchor), anchor, "Unknown anchor pofloat");
        }
    }

    public FluentArea(
        Vector2 relativePosition,
        Vector2 relativeSize,
        Vector2 absolutePosition,
        Vector2 absoluteSize,
        FluentAnchor anchor = FluentAnchor.TopLeft
    ) : this(
        relativePosition.x,
        relativePosition.y,
        relativeSize.x,
        relativeSize.y,
        absolutePosition.x,
        absolutePosition.y,
        absoluteSize.x,
        absoluteSize.y,
        anchor
    )
    {
    }

    /// <summary>
    /// Creates a new FluentArea with absolute values only.
    /// </summary>
    /// <param name="x">The X position in pixels.</param>
    /// <param name="y">The Y position in pixels.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="anchor">The anchor pofloat.</param>
    public static FluentArea Absolute(float x, float y, float width, float height, FluentAnchor anchor = FluentAnchor.TopLeft) =>
        new(0, 0, 0, 0, x, y, width, height);

    /// <summary>
    /// Creates a new FluentArea with relative values only.
    /// </summary>
    /// <param name="x">The X position as a proportion (0.0-1.0).</param>
    /// <param name="y">The Y position as a proportion (0.0-1.0).</param>
    /// <param name="width">The width as a proportion (0.0-1.0).</param>
    /// <param name="height">The height as a proportion (0.0-1.0).</param>
    /// <param name="anchor">The anchor pofloat.</param>
    public static FluentArea Relative(float x, float y, float width, float height, FluentAnchor anchor = FluentAnchor.TopLeft) =>
        new(x, y, width, height, 0, 0, 0, 0, FluentAnchor.TopLeft);

    public override string ToString() => $"[{xMin} {xMax} {yMin} {yMax} {OxMin} {OxMax} {OyMin} {OyMax}]";
}