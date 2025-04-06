using HizenLabs.FluentUI.Abstractions;
using HizenLabs.FluentUI.Abstractions.IFluentBuilderSupports;
using HizenLabs.FluentUI.Elements;
using HizenLabs.FluentUI.Enums;
using HizenLabs.FluentUI.Primitives;
using System;
using UnityEngine;

namespace HizenLabs.FluentUI.Builders;

internal class FluentPanelBuilder : FluentElementBuilder<FluentPanel, FluentPanelBuilder>, IFluentPanelBuilder
{
    IFluentPanelBuilder IFluentBuildPosition<IFluentPanelBuilder>.AbsolutePosition(float x, float y)
    {
        return AbsolutePosition(x, y);
    }

    IFluentPanelBuilder IFluentBuildPosition<IFluentPanelBuilder>.AbsolutePosition(Vector2 position)
    {
        return AbsolutePosition(position);
    }

    IFluentPanelBuilder IFluentBuildSize<IFluentPanelBuilder>.AbsoluteSize(float width, float height)
    {
        return AbsoluteSize(width, height);
    }

    IFluentPanelBuilder IFluentBuildSize<IFluentPanelBuilder>.AbsoluteSize(Vector2 size)
    {
        return AbsoluteSize(size);
    }

    IFluentPanelBuilder IFluentBuildPosition<IFluentPanelBuilder>.Anchor(FluentAnchor anchor)
    {
        return Anchor(anchor);
    }

    IFluentPanelBuilder IFluentBuildBackgroundColor<IFluentPanelBuilder>.BackgroundColor(byte r, byte g, byte b, float alpha)
    {
        return BackgroundColor(r, g, b, alpha);
    }

    IFluentPanelBuilder IFluentBuildBackgroundColor<IFluentPanelBuilder>.BackgroundColor(FluentColor color)
    {
        return BackgroundColor(color);
    }

    IFluentPanelBuilder IFluentBuildDelay<IFluentPanelBuilder>.Delay(float delay)
    {
        return Delay(delay);
    }

    IFluentPanelBuilder IFluentBuildFade<IFluentPanelBuilder>.Fade(float duration)
    {
        return Fade(duration);
    }

    IFluentPanelBuilder IFluentBuildFade<IFluentPanelBuilder>.Fade(float fadeInDuration, float fadeOutDuration)
    {
        return Fade(fadeInDuration, fadeOutDuration);
    }

    IFluentPanelBuilder IFluentBuildFade<IFluentPanelBuilder>.FadeIn(float duration)
    {
        return FadeIn(duration);
    }

    IFluentPanelBuilder IFluentBuildFade<IFluentPanelBuilder>.FadeOut(float duration)
    {
        return FadeOut(duration);
    }

    IFluentPanelBuilder IFluentBuildSize<IFluentPanelBuilder>.FillParent(float padding)
    {
        return FillParent(padding);
    }

    IFluentPanelBuilder IFluentBuildSubComponents<IFluentPanelBuilder>.Panel(Action<IFluentPanelBuilder> setupAction)
    {
        return Panel(setupAction);
    }

    IFluentPanelBuilder IFluentBuildPosition<IFluentPanelBuilder>.RelativePosition(float x, float y)
    {
        return RelativePosition(x, y);
    }

    IFluentPanelBuilder IFluentBuildPosition<IFluentPanelBuilder>.RelativePosition(Vector2 position)
    {
        return RelativePosition(position);
    }

    IFluentPanelBuilder IFluentBuildSize<IFluentPanelBuilder>.RelativeSize(float width, float height)
    {
        return RelativeSize(width, height);
    }

    IFluentPanelBuilder IFluentBuildSize<IFluentPanelBuilder>.RelativeSize(Vector2 size)
    {
        return RelativeSize(size);
    }
}
