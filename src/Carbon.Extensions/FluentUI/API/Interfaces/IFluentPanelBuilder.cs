using HizenLabs.FluentUI.Core.Builders.Base.Capabilities;

namespace HizenLabs.FluentUI.API.Interfaces;

/// <summary>
/// Represents a builder for creating and configuring Fluent UI panels.
/// </summary>
public interface IFluentPanelBuilder : IFluentElementBuilder<IFluentElement, IFluentPanelBuilder>,
    IFluentBuildBackgroundColor<IFluentPanelBuilder>,
    IFluentBuildPosition<IFluentPanelBuilder>,
    IFluentBuildSize<IFluentPanelBuilder>,
    IFluentBuildFade<IFluentPanelBuilder>,
    IFluentBuildDelay<IFluentPanelBuilder>,
    IFluentBuildSubComponents<IFluentPanelBuilder>
{
}
