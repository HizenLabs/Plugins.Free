using HizenLabs.FluentUI.Builders;
using System;

namespace HizenLabs.FluentUI.Abstractions.IFluentBuilderSupports;

public interface IFluentBuildSubComponents<TBuilderInterface>
{
    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.Panel(Action{IFluentPanelBuilder})"/>
    TBuilderInterface Panel(Action<IFluentPanelBuilder> setupAction);
}
