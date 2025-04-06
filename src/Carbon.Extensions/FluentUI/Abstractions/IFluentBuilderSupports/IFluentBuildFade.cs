using HizenLabs.FluentUI.Builders;

namespace HizenLabs.FluentUI.Abstractions.IFluentBuilderSupports;

public interface IFluentBuildFade<TBuilderInterface>
{
    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.Fade(float)"/>
    TBuilderInterface Fade(float duration);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.Fade(float, float)"/>
    TBuilderInterface Fade(float fadeInDuration, float fadeOutDuration);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.FadeIn(float)"/>
    TBuilderInterface FadeIn(float duration);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.FadeOut(float)"/>
    TBuilderInterface FadeOut(float duration);
}
