using HizenLabs.FluentUI.API.Interfaces;
using System;

namespace HizenLabs.FluentUI.Core.Builders.Base.Capabilities;

public interface IFluentBuildSubComponents<TBuilderInterface>
{
    /// <inheritdoc cref="FluentElementBuilder{TElement, TBuilder}.Panel(Action{IFluentPanelBuilder})"/>
    TBuilderInterface Panel(Action<IFluentPanelBuilder> setupAction);
}
