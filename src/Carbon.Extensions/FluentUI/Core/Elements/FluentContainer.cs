using Carbon;
using Carbon.Components;
using HizenLabs.FluentUI.API.Interfaces;
using HizenLabs.FluentUI.Core.Elements.Base;
using HizenLabs.FluentUI.Utils.Debug;
using HizenLabs.FluentUI.Utils.Delays;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.Core.Elements;

/// <summary>
/// The primary starting container in a fluent ui process.
/// </summary>
internal class FluentContainer : FluentElement<FluentContainer>
{
    public CuiElementContainer Render(
        CUI cui,
        List<DelayedAction<CUI>> delayedRenders,
        List<DelayedAction<CUI, BasePlayer[]>> destroyActions
    )
    {
        Options.ContainerId = Options.Id = $"{Options.Id}[{Guid.NewGuid()}]";

        var area = Options.Area;
        Logger.Log($"Render FluentContainer: {Options.Id}");
        Logger.Log($"  Area: {area}");
        Logger.Log($"  FadeIn: {Options.FadeIn}");
        Logger.Log($"  FadeOut: {Options.FadeOut}");
        Logger.Log($"  NeedsCursor: {Options.NeedsCursor}");
        Logger.Log($"  NeedsKeyboard: {Options.NeedsKeyboard}");
        Logger.Log($"  BackgroundColor: {Options.BackgroundColor}");

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
        RenderChildren(cui, container, this, delayedRenders, 0f, destroyActions);
        return container;
    }

    /// <inheritdoc/>   
    protected override void RenderElement(CUI cui, CuiElementContainer container, IFluentElement parent, string elementId, string destroyUi = null)
    {
        // Skip since we are rendering elsewhere for the base container.
    }
}
