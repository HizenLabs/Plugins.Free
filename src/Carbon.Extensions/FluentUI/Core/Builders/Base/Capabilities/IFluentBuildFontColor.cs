﻿using HizenLabs.FluentUI.Primitives;

namespace HizenLabs.FluentUI.Core.Builders.Base.Capabilities;

public interface IFluentBuildFontColor<TBuilderInterface>
{
    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.FontColor(byte, byte, byte, float)"/>
    TBuilderInterface FontColor(byte r, byte g, byte b, float alpha = 1);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.FontColor(FluentColor)"/>
    TBuilderInterface FontColor(FluentColor color);
}
