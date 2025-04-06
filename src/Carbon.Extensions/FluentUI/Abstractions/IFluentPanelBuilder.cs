using HizenLabs.FluentUI.Abstractions.IFluentBuilderSupports;
using HizenLabs.FluentUI.Builders;
using HizenLabs.FluentUI.Enums;
using HizenLabs.FluentUI.Primitives;
using System;
using UnityEngine;

namespace HizenLabs.FluentUI.Abstractions;

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
