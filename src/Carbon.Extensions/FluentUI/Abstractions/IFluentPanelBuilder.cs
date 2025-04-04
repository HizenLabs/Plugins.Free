﻿using HizenLabs.FluentUI.Enums;
using HizenLabs.FluentUI.Primitives;
using System;

namespace HizenLabs.FluentUI.Abstractions;

public interface IFluentPanelBuilder : IFluentElementBuilder<IFluentElement, IFluentPanelBuilder>
{
    IFluentPanelBuilder BackgroundColor(FluentColor color);

    IFluentPanelBuilder AbsolutePosition(float x, float y);

    IFluentPanelBuilder AbsoluteSize(float width, float height);

    IFluentPanelBuilder RelativePosition(float x, float y);

    IFluentPanelBuilder RelativeSize(float width, float height);

    IFluentPanelBuilder Anchor(FluentAnchor anchor);

    IFluentPanelBuilder Delay(float delay);

    IFluentPanelBuilder Fade(float duration);

    IFluentPanelBuilder Fade(float fadeInDuration, float fadeOutDuration);

    IFluentPanelBuilder FadeIn(float duration);

    IFluentPanelBuilder FadeOut(float duration);

    IFluentPanelBuilder Panel(Action<IFluentPanelBuilder> setupAction);
}
