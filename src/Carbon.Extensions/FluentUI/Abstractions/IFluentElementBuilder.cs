using Facepunch;

namespace HizenLabs.FluentUI.Abstractions;

public interface IFluentElementBuilder<TElement, TBuilder> : Pool.IPooled
    where TElement : IFluentElement
    where TBuilder : IFluentElementBuilder<TElement, TBuilder>
{
}
