using HizenLabs.FluentUI.API.Interfaces;
using HizenLabs.FluentUI.Core.Elements;
using HizenLabs.FluentUI.Core.Elements.Base;
using HizenLabs.FluentUI.Core.Services.Pooling;
using HizenLabs.FluentUI.Primitives;
using HizenLabs.FluentUI.Primitives.Enums;
using HizenLabs.FluentUI.Utils.Debug;
using System;
using UnityEngine;

namespace HizenLabs.FluentUI.Core.Builders.Base;

/// <summary>
/// Base builder class for all FluentUI elements.
/// Provides common builder methods for configuring UI elements with a fluent API.
/// </summary>
/// <typeparam name="TElement">The type of element being built.</typeparam>
/// <typeparam name="TBuilder">The concrete builder type, for proper method chaining.</typeparam>
internal class FluentElementBuilder<TElement, TBuilder> : IFluentElementBuilder<TElement, TBuilder>
    where TElement : FluentElement<TElement>, new()
    where TBuilder : FluentElementBuilder<TElement, TBuilder>, new()
{
    private TElement _element;

    /// <summary>
    /// Gets a reference to this instance cast to the concrete builder type.
    /// Used for fluent method chaining.
    /// </summary>
    private TBuilder This => (TBuilder)this;

    /// <summary>
    /// Finalizes the element being built and returns it.
    /// </summary>
    /// <param name="id">The unique identifier for the element.</param>
    /// <returns>The built element instance.</returns>
    internal TElement Build(string id)
    {
        _element.Options.Id = id;
        return _element;
    }

    /// <summary>
    /// Sets the background color of the element using RGB values.
    /// </summary>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="alpha">Alpha (opacity) component (0.0-1.0).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder BackgroundColor(byte r, byte g, byte b, float alpha = 1)
        => BackgroundColor(new(r, g, b, alpha));

    /// <summary>
    /// Sets the background color of the element.
    /// </summary>
    /// <param name="color">The background color to apply.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder BackgroundColor(FluentColor color)
    {
        _element.Options.BackgroundColor = color;
        return This;
    }

    /// <summary>
    /// Sets the font color of the element using RGB values.
    /// </summary>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="alpha">Alpha (opacity) component (0.0-1.0).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder FontColor(byte r, byte g, byte b, float alpha = 1) =>
        FontColor(new(r, g, b, alpha));

    /// <summary>
    /// Sets the font color of the element.
    /// </summary>
    /// <param name="color">The font color to apply.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder FontColor(FluentColor color)
    {
        _element.Options.FontColor = color;
        return This;
    }

    /// <summary>
    /// Sets both absolute position and size in a single call.
    /// </summary>
    /// <param name="x">The X position in pixels.</param>
    /// <param name="y">The Y position in pixels.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder AbsoluteArea(float x, float y, float width, float height) =>
        AbsolutePosition(x, y)
        .AbsoluteSize(width, height);

    /// <summary>
    /// Sets the absolute position of the element in pixels.
    /// </summary>
    /// <param name="x">The X position in pixels.</param>
    /// <param name="y">The Y position in pixels.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder AbsolutePosition(float x, float y) =>
        AbsolutePosition(new(x, y));

    /// <summary>
    /// Sets the absolute position of the element using a Vector2.
    /// </summary>
    /// <param name="position">The position as a Vector2.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder AbsolutePosition(Vector2 position)
    {
        _element.Options.AbsolutePosition = position;
        return This;
    }

    /// <summary>
    /// Sets the absolute size of the element in pixels.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder AbsoluteSize(float width, float height) =>
        AbsoluteSize(new(width, height));

    /// <summary>
    /// Sets the absolute size of the element using a Vector2.
    /// </summary>
    /// <param name="size">The size as a Vector2.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder AbsoluteSize(Vector2 size)
    {
        _element.Options.AbsoluteSize = size;
        return This;
    }

    /// <summary>
    /// Sets both relative position and size in a single call.
    /// </summary>
    /// <param name="x">The X position as a proportion (0.0-1.0).</param>
    /// <param name="y">The Y position as a proportion (0.0-1.0).</param>
    /// <param name="width">The width as a proportion (0.0-1.0).</param>
    /// <param name="height">The height as a proportion (0.0-1.0).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder RelativeArea(float x, float y, float width, float height) =>
        RelativePosition(x, y)
        .RelativeSize(width, height);

    /// <summary>
    /// Sets the relative position of the element as a proportion (0.0-1.0) of its parent container.
    /// </summary>
    /// <param name="x">The X position as a proportion (0.0-1.0).</param>
    /// <param name="y">The Y position as a proportion (0.0-1.0).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder RelativePosition(float x, float y) =>
        RelativePosition(new(x, y));

    /// <summary>
    /// Sets the relative position of the element using a Vector2.
    /// </summary>
    /// <param name="position">The position as a Vector2 with values between 0.0 and 1.0.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder RelativePosition(Vector2 position)
    {
        _element.Options.RelativePosition = position;
        return This;
    }

    /// <summary>
    /// Sets the relative size of the element as a proportion (0.0-1.0) of its parent container.
    /// </summary>
    /// <param name="width">The width as a proportion (0.0-1.0).</param>
    /// <param name="height">The height as a proportion (0.0-1.0).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder RelativeSize(float width, float height) =>
        RelativeSize(new(width, height));

    /// <summary>
    /// Sets the relative size of the element using a Vector2.
    /// </summary>
    /// <param name="size">The size as a Vector2 with values between 0.0 and 1.0.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder RelativeSize(Vector2 size)
    {
        _element.Options.RelativeSize = size;
        return This;
    }

    /// <summary>
    /// Sets the anchor point of the element, which affects how position and size values are interpreted.
    /// </summary>
    /// <param name="anchor">The anchor point to use.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder Anchor(FluentAnchor anchor)
    {
        _element.Options.Anchor = anchor;
        return This;
    }

    /// <summary>
    /// Sets the element to fill its parent container.
    /// </summary>
    /// <param name="padding">Optional padding as a proportion (0.0-1.0).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder FillParent(float padding = 0) =>
        RelativePosition(padding, padding)
        .RelativeSize(1f - padding * 2, 1f - padding * 2);

    /// <summary>
    /// Sets the fade-in and fade-out durations for the element to the same value.
    /// </summary>
    /// <param name="duration">The fade duration in seconds.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder Fade(float duration) =>
        Fade(duration, duration);

    /// <summary>
    /// Sets the fade-in and fade-out durations for the element.
    /// </summary>
    /// <param name="fadeInDuration">The fade-in duration in seconds.</param>
    /// <param name="fadeOutDuration">The fade-out duration in seconds.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder Fade(float fadeInDuration, float fadeOutDuration) =>
        FadeIn(fadeInDuration)
        .FadeOut(fadeOutDuration);

    /// <summary>
    /// Sets the fade-in duration for the element, creating a smooth appearance animation.
    /// </summary>
    /// <param name="duration">The duration in seconds.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder FadeIn(float duration)
    {
        _element.Options.FadeIn = duration;
        return This;
    }

    /// <summary>
    /// Sets the fade-out duration for the element, creating a smooth disappearance animation.
    /// </summary>
    /// <param name="duration">The duration in seconds.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder FadeOut(float duration)
    {
        _element.Options.FadeOut = duration;
        return This;
    }

    /// <summary>
    /// Specifies whether the element requires cursor input.
    /// If set to true, cursor input will be captured by the UI until it is closed.
    /// </summary>
    /// <param name="needs">Whether cursor input is required (default: true).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder NeedsCursor(bool needs = true)
    {
        _element.Options.NeedsCursor = needs;
        return This;
    }

    /// <summary>
    /// Specifies whether the element requires keyboard input.
    /// If set to true, keyboard input will be captured by the UI until it is closed.
    /// </summary>
    /// <param name="needs">Whether keyboard input is required (default: true).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder NeedsKeyboard(bool needs = true)
    {
        _element.Options.NeedsKeyboard = needs;
        return This;
    }

    /// <summary>
    /// Sets both cursor and keyboard input requirements in a single call.
    /// </summary>
    /// <param name="needsCursor">Whether cursor input is required.</param>
    /// <param name="needsKeyboard">Whether keyboard input is required.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder NeedsInput(bool needsCursor, bool needsKeyboard) =>
        NeedsCursor(needsCursor)
        .NeedsKeyboard(needsKeyboard);

    /// <summary>
    /// Sets the duration after which the element will be automatically disposed.
    /// </summary>
    /// <param name="seconds">The duration in seconds.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder Duration(float seconds)
    {
        _element.Options.Duration = seconds;
        return This;
    }

    /// <summary>
    /// Sets the delay before the element is displayed.
    /// </summary>
    /// <param name="seconds">The delay in seconds.</param>
    /// <remarks>
    /// <para>
    /// This delay is relative to the parent element. If the parent has a delay of 2 seconds,
    /// and the child has a delay of 1 second, the child will appear after 3 seconds.
    /// </para>
    /// <para>
    /// Additionally, if the parent container is destroyed before the delay period ends, 
    /// then the element will not be created.
    /// </para>
    /// </remarks>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder Delay(float seconds)
    {
        _element.Options.Delay = seconds;
        return This;
    }

    /// <summary>
    /// Configures the element as a transient popup that appears and disappears automatically.
    /// </summary>
    /// <param name="delay">The delay before the element appears, in seconds.</param>
    /// <param name="duration">The duration the element stays visible, in seconds.</param>
    /// <param name="fadeIn">The fade-in duration, in seconds.</param>
    /// <param name="fadeOut">The fade-out duration, in seconds.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder AsPopup(float delay, float duration, float fadeIn = 0.3f, float fadeOut = 0.3f) =>
        Delay(delay)
            .Duration(duration)
            .FadeIn(fadeIn)
            .FadeOut(fadeOut)
            .NeedsCursor()
            .NeedsKeyboard();

    /// <summary>
    /// Adds a panel element as a child of this element.
    /// </summary>
    /// <param name="setupAction">An action to configure the panel.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public TBuilder Panel(Action<IFluentPanelBuilder> setupAction) =>
        AddElement<FluentPanel, FluentPanelBuilder>(setupAction);

    /// <summary>
    /// Helper method to add a child element to this element.
    /// </summary>
    /// <typeparam name="TInnerElement">The type of element to add.</typeparam>
    /// <typeparam name="TInnerBuilder">The builder type for the element.</typeparam>
    /// <param name="setupAction">An action to configure the child element.</param>
    /// <returns>This builder instance for method chaining.</returns>
    private TBuilder AddElement<TInnerElement, TInnerBuilder>(Action<TInnerBuilder> setupAction)
        where TInnerElement : FluentElement<TInnerElement>, new()
        where TInnerBuilder : FluentElementBuilder<TInnerElement, TInnerBuilder>, new()
    {
        _element ??= FluentPool.Get<TElement>();

        var builder = FluentPool.Get<TInnerBuilder>();
        setupAction(builder);

        var child = builder.Build(_element.Options.Id);
        _element.AddElement(child);

        FluentPool.Free(ref builder);
        return This;
    }

    /// <summary>
    /// Prepares the builder for return to the object pool.
    /// Clears the element reference.
    /// </summary>
    public void EnterPool()
    {
        using var debug = FluentDebug.BeginScope();
        debug.Log($"{typeof(TBuilder).Name} is returning to pool");

        _element = null;
    }

    /// <summary>
    /// Initializes the builder when retrieved from the object pool.
    /// Creates a new element instance for building.
    /// </summary>
    public void LeavePool()
    {
        using var debug = FluentDebug.BeginScope();
        debug.Log($"Getting {typeof(TBuilder).Name} from pool");

        _element = FluentPool.Get<TElement>();
    }
}