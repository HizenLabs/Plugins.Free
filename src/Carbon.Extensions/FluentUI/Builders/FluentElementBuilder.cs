using Facepunch;
using HizenLabs.FluentUI.Abstractions;
using HizenLabs.FluentUI.Elements;
using HizenLabs.FluentUI.Primitives;
using System;

namespace HizenLabs.FluentUI.Builders;

internal class FluentElementBuilder<TElement, TBuilder> : IFluentElementBuilder<TElement, TBuilder>
    where TElement : FluentElement<TElement>, new()
    where TBuilder : class, IFluentElementBuilder<TElement, TBuilder>, new()
{
    private FluentElement<TElement> _element;

    internal TElement Build(string id)
    {
        _element.Options.Id = id;
        return (TElement)_element;
    }

    public TBuilder Panel(Action<IFluentPanelBuilder> setupAction) =>
        AddElement<FluentPanel, FluentPanelBuilder>(setupAction);

    private TBuilder AddElement<TInnerElement, TInnerBuilder>(Action<TInnerBuilder> setupAction)
        where TInnerElement : FluentElement<TInnerElement>, new()
        where TInnerBuilder : FluentElementBuilder<TInnerElement, TInnerBuilder>, new()
    {
        _element ??= Pool.Get<TElement>();

        var builder = Pool.Get<TInnerBuilder>();
        setupAction(builder);

        var child = builder.Build(_element.Options.Id);
        _element.AddElement(child);

        Pool.Free(ref builder);
        return (TBuilder)(object)this;
    }

    /// <summary>
    /// Enters the object pool, clearing out the existing reference.
    /// </summary>
    public void EnterPool()
    {
        _element = null;
    }

    /// <summary>
    /// Do not create a new element ref here, it will be created via Init() to prevent any duplications.
    /// </summary>
    public void LeavePool()
    {
    }
}
