﻿using Carbon.Components;
using HizenLabs.FluentUI.Abstractions;
using Oxide.Game.Rust.Cui;

namespace HizenLabs.FluentUI.Elements;

/// <summary>
/// The primary starting container in a fluent ui process.
/// </summary>
internal class FluentContainer : FluentElement<FluentContainer>
{
    public CuiElementContainer Render(CUI cui)
    {
        var area = Options.Area;
        return cui.CreateContainer(
            Options.Id,
            color: Options.BackgroundColor,
            xMin: area.xMin,
            xMax: area.xMax,
            yMin: area.yMin,
            yMax: area.yMax,
            OxMin: area.OxMin,
            OxMax: area.OxMax,
            OyMin: area.OyMin,
            OyMax: area.OyMax,
            fadeIn: Options.FadeIn,
            fadeOut: Options.FadeOut,
            needsCursor: Options.NeedsCursor,
            needsKeyboard: Options.NeedsKeyboard,
            destroyUi: Options.Id
        );
    }

    /// <inheritdoc/>
    protected override void RenderElement(CUI cui, CuiElementContainer container, string parent, string elementId)
    {
        // Skip since we are rendering elsewhere for the base container.
    }
}
