using Carbon;
using Carbon.Components;
using HizenLabs.FluentUI.Abstractions;
using HizenLabs.FluentUI.Internals;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.Elements;

/// <summary>
/// The primary starting container in a fluent ui process.
/// </summary>
internal class FluentContainer : FluentElement<FluentContainer>
{
    public CuiElementContainer Render(
        CUI cui,
        List<DelayedAction<CUI>> delayedRenders,
        List<DelayedAction<CUI, BasePlayer>> destroyActions
    )
    {
        var area = Options.Area;
        var container = cui.CreateContainer(
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

        // Render child elements directly since the id is already defined for the container.
        RenderChildren(cui, container, Options.Id, delayedRenders, 0f, destroyActions);
        return container;
    }

    /// <inheritdoc/>   
    protected override void RenderElement(CUI cui, CuiElementContainer container, string parent, string elementId)
    {
        // Skip since we are rendering elsewhere for the base container.
    }
}
