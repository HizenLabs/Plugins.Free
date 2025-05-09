﻿using HizenLabs.FluentUI.Primitives;

namespace HizenLabs.FluentUI.Core.Builders.Base.Capabilities;

public interface IFluentBuildBackgroundColor<TBuilderInterface>
{
    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.BackgroundColor(byte, byte, byte, float)"/>
    TBuilderInterface BackgroundColor(byte r, byte g, byte b, float alpha = 1);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.BackgroundColor(FluentColor)"/>
    TBuilderInterface BackgroundColor(FluentColor color);

}
