using HizenLabs.FluentUI.Primitives.Enums;
using UnityEngine;

namespace HizenLabs.FluentUI.Core.Builders.Base.Capabilities;

public interface IFluentBuildPosition<TBuilderInterface>
{
    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.Anchor(FluentAnchor)"/>
    TBuilderInterface Anchor(FluentAnchor anchor);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.AbsolutePosition(float, float)"/>
    TBuilderInterface AbsolutePosition(float x, float y);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.AbsolutePosition(Vector2)"/>
    TBuilderInterface AbsolutePosition(Vector2 position);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.RelativePosition(float, float)"/>
    TBuilderInterface RelativePosition(float x, float y);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.RelativePosition(Vector2)"/>
    TBuilderInterface RelativePosition(Vector2 position);
}
