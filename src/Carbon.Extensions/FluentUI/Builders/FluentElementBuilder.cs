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

    private TBuilder This => (TBuilder)(object)this;

    internal TElement Build(string id)
    {
        _element.Options.Id = id;
        return (TElement)_element;
    }

    public TBuilder BackgroundColor(FluentColor color)
    {
        _element.Options.BackgroundColor = color;
        return This;
    }

    public TBuilder AbsolutePosition(float x, float y)
    {
        _element.Options.AbsolutePosition = new(x, y);
        return This;
    }

    public TBuilder AbsoluteSize(float width, float height)
    {
        _element.Options.AbsoluteSize = new(width, height);
        return This;
    }

    public TBuilder RelativePosition(float x, float y)
    {
        _element.Options.RelativePosition = new(x, y);
        return This;
    }

    public TBuilder RelativeSize(float width, float height)
    {
        _element.Options.RelativeSize = new(width, height);
        return This;
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
        return This;
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
