using Facepunch;

namespace HizenLabs.FluentUI.API.Interfaces;

/// <summary>
/// Represents a builder for a specific type of UI element.
/// </summary>
public interface IFluentElementBuilder : Pool.IPooled
{
}

/// <inheritdoc cref="IFluentElementBuilder"/>
/// typeparam name="TElement">The type of the UI element being built.</typeparam>
/// typeparam name="TBuilder">
/// The type of the builder itself. This is kind of hacky, but it allows 
/// the base class to be generic and still return the correct type.
/// There might be a better way to handle, but for now it works.
/// </typeparam>
public interface IFluentElementBuilder<TElement, TBuilder> : IFluentElementBuilder
    where TElement : IFluentElement
    where TBuilder : IFluentElementBuilder<TElement, TBuilder>
{
}
