namespace HizenLabs.FluentUI.Core.Builders.Base.Capabilities;

public interface IFluentBuildDelay<TBuilderInterface>
{
    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.Delay(float)"/>
    TBuilderInterface Delay(float delay);
}
