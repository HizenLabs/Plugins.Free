﻿using HizenLabs.FluentUI.Abstractions;
using HizenLabs.FluentUI.Elements;
using HizenLabs.FluentUI.Enums;
using HizenLabs.FluentUI.Primitives;
using System;

namespace HizenLabs.FluentUI.Builders;

internal class FluentPanelBuilder : FluentElementBuilder<FluentPanel, FluentPanelBuilder>, IFluentPanelBuilder
{
    IFluentPanelBuilder IFluentPanelBuilder.BackgroundColor(FluentColor color) =>
        BackgroundColor(color);

    IFluentPanelBuilder IFluentPanelBuilder.AbsolutePosition(float x, float y) =>
        AbsolutePosition(x, y);

    IFluentPanelBuilder IFluentPanelBuilder.AbsoluteSize(float width, float height) =>
        AbsoluteSize(width, height);

    IFluentPanelBuilder IFluentPanelBuilder.RelativePosition(float x, float y) =>
        RelativePosition(x, y);

    IFluentPanelBuilder IFluentPanelBuilder.RelativeSize(float width, float height) =>
        RelativeSize(width, height);

    IFluentPanelBuilder IFluentPanelBuilder.Anchor(FluentAnchor anchor) =>
        Anchor(anchor);

    IFluentPanelBuilder IFluentPanelBuilder.Delay(float delay) =>
        Delay(delay);

    IFluentPanelBuilder IFluentPanelBuilder.Fade(float duration) =>
        Fade(duration, duration);

    IFluentPanelBuilder IFluentPanelBuilder.Fade(float fadeInDuration, float fadeOutDuration) =>
        Fade(fadeInDuration, fadeOutDuration);

    IFluentPanelBuilder IFluentPanelBuilder.FadeIn(float duration) =>
        FadeIn(duration);

    IFluentPanelBuilder IFluentPanelBuilder.FadeOut(float duration) =>
        FadeOut(duration);

    IFluentPanelBuilder IFluentPanelBuilder.Panel(Action<IFluentPanelBuilder> setupAction) =>
        Panel(setupAction);
}
