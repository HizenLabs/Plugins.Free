using Carbon;
using Carbon.Components;
using HizenLabs.FluentUI.API.Interfaces;
using HizenLabs.FluentUI.Core.Elements.Base;
using Oxide.Game.Rust.Cui;
using System;

namespace HizenLabs.FluentUI.Core.Elements;

internal class FluentPanel : FluentElement<FluentPanel>
{
    /// <inheritdoc/>
    protected override void RenderElement(CUI cui, CuiElementContainer container, IFluentElement parent, string elementId, string destroyUi = null)
    {
        var area = Options.Area;

        Logger.Log($"Render FluentPanel: {elementId}");
        Logger.Log($"  Container: {Options.ContainerId}");
        Logger.Log($"  Parent: {parent.Options.Id}");
        Logger.Log($"  Id: {Options.Id}");
        Logger.Log($"  Color: {Options.BackgroundColor}");
        Logger.Log($"  Area: {area}");
        Logger.Log($"  FadeIn: {Options.FadeIn}");
        Logger.Log($"  FadeOut: {Options.FadeOut}");
        Logger.Log($"  NeedsCursor: {Options.NeedsCursor}");
        Logger.Log($"  NeedsKeyboard: {Options.NeedsKeyboard}");
        Logger.Log($"  DestroyUi: {destroyUi}");

        cui.CreatePanel(
            container: container,
            parent: parent.Options.Id,
            id: elementId,
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
            destroyUi: destroyUi
        );
    }
}
