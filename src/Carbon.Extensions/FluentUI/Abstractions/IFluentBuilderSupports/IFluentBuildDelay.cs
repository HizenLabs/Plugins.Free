using HizenLabs.FluentUI.Builders;

namespace HizenLabs.FluentUI.Abstractions.IFluentBuilderSupports;

public interface IFluentBuildDelay<TBuilderInterface>
{
    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.Delay(float)"/>
    TBuilderInterface Delay(float delay);
}
