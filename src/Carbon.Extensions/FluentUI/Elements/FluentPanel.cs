using Carbon;
using Carbon.Components;
using HizenLabs.FluentUI.Abstractions;
using Oxide.Game.Rust.Cui;

namespace HizenLabs.FluentUI.Elements;

internal class FluentPanel : FluentElement<FluentPanel>
{
    /// <inheritdoc/>
    protected override void RenderElement(CUI cui, CuiElementContainer container, string parent, string elementId)
    {
        var area = Options.Area;
        cui.CreatePanel(container, parent, id: elementId,
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
            needsKeyboard: Options.NeedsKeyboard
        );

        Logger.Log($"Panel created with parameters:");
        Logger.Log($"  Container: {(container?.Name ?? "null")}");
        Logger.Log($"  Parent: {parent}");
        Logger.Log($"  ElementId: {elementId}");
        Logger.Log($"  BackgroundColor: {Options.BackgroundColor}");
        Logger.Log($"  Area values: xMin={area.xMin}, xMax={area.xMax}, yMin={area.yMin}, yMax={area.yMax}");
        Logger.Log($"  Absolute values: OxMin={area.OxMin}, OxMax={area.OxMax}, OyMin={area.OyMin}, OyMax={area.OyMax}");
        Logger.Log($"  FadeIn: {Options.FadeIn}, FadeOut: {Options.FadeOut}");
        Logger.Log($"  NeedsCursor: {Options.NeedsCursor}, NeedsKeyboard: {Options.NeedsKeyboard}");
    }
}
