using Carbon;
using Carbon.Components;
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

        Logger.Log($"Container created with parameters:");
        Logger.Log($"  ID: {Options.Id}");
        Logger.Log($"  BackgroundColor: {Options.BackgroundColor}");
        Logger.Log($"  Area: xMin={area.xMin}, xMax={area.xMax}, yMin={area.yMin}, yMax={area.yMax}");
        Logger.Log($"  Absolute: OxMin={area.OxMin}, OxMax={area.OxMax}, OyMin={area.OyMin}, OyMax={area.OyMax}");
        Logger.Log($"  FadeIn: {Options.FadeIn}, FadeOut: {Options.FadeOut}");
        Logger.Log($"  NeedsCursor: {Options.NeedsCursor}, NeedsKeyboard: {Options.NeedsKeyboard}");
        Logger.Log($"  DestroyUi: {Options.Id}");

        // Render child elements directly since the id is already defined for the container.
        RenderChildren(cui, container, Options.Id);
        return container;
    }

    /// <inheritdoc/>   
    protected override void RenderElement(CUI cui, CuiElementContainer container, string parent, string elementId)
    {
        // Skip since we are rendering elsewhere for the base container.
    }
}
