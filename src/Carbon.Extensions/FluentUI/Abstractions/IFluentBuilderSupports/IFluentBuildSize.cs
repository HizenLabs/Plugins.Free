using HizenLabs.FluentUI.Builders;
using UnityEngine;

namespace HizenLabs.FluentUI.Abstractions.IFluentBuilderSupports;

public interface IFluentBuildSize<TBuilderInterface>
{
    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.AbsoluteSize(float, float)"/>
    TBuilderInterface AbsoluteSize(float width, float height);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.AbsoluteSize(Vector2)"/>
    TBuilderInterface AbsoluteSize(Vector2 size);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.RelativeSize(float, float)"/>
    TBuilderInterface RelativeSize(float width, float height);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.RelativeSize(Vector2)"/>
    TBuilderInterface RelativeSize(Vector2 size);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.FillParent(float)"/>
    TBuilderInterface FillParent(float padding = 0);
}
