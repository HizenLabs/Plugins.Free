namespace HizenLabs.FluentUI.Core.Builders.Base.Capabilities;

public interface IFluentBuildArea<TBuilderInterface>
{
    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.AbsoluteArea(float, float, float, float)"/>
    TBuilderInterface AbsoluteArea(float x, float y, float width, float height);

    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.RelativeArea(float, float, float, float)"/>
    TBuilderInterface RelativeArea(float x, float y, float width, float height);
}
