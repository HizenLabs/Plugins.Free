using Carbon.Plugins;
using HizenLabs.FluentUI.API;
using HizenLabs.FluentUI.API.Interfaces;
using HizenLabs.FluentUI.Primitives.Enums;
using HizenLabs.FluentUI.Utils.Debug;
using HizenLabs.FluentUI.Utils.Delays;
using HizenLabs.FluentUI.Utils.Delays.Base;
using HizenLabs.FluentUI.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HizenLabs.FluentUI.Core.Services.Pooling;

/// <summary>
/// A custom pool used to manage <see cref="FluentBuilder"/> objects.
/// Handles disposal of timer objects that might run after a builder is disposed.
/// </summary>
internal class BuilderPool : IDisposable
{
    private readonly CarbonPlugin _plugin;
    private List<IDelayedAction> _pendingActions;
    private List<IFluentElement> _trackingElements;

    public BuilderPool(CarbonPlugin plugin)
    {
        _plugin = plugin;
        _pendingActions = FluentPool.Get<List<IDelayedAction>>();
        _trackingElements = FluentPool.Get<List<IFluentElement>>();
    }

    /// <summary>
    /// Begins tracking the specified delayed action.
    /// </summary>
    /// <param name="action">The delayed action to track.</param>
    public void BeginTracking(IDelayedAction action)
    {
        if (!_pendingActions.Contains(action))
        {
            _pendingActions.Add(action);
        }
        else
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Warning: Action already tracked?: {action.GetType().GetFriendlyTypeName()}");
        }
    }

    /// <summary>
    /// Begins tracking the specified element.
    /// </summary>
    /// <param name="element">The element to track.</param>
    public void BeginTracking(IFluentElement element)
    {
        if (!_trackingElements.Contains(element))
        {
            _trackingElements.Add(element);
        }
        else
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Warning: Action already tracked?: {element.GetType().GetFriendlyTypeName()}");
        }
    }

    /// <summary>
    /// Creates a new delayed action and tracks it, returning the newly created instance.
    /// </summary>
    /// <param name="delaySeconds">The delay in seconds before the action is executed.</param>
    /// <param name="action">The action to be executed after the delay.</param>
    /// <returns>The newly created delayed action.</returns>
    public DelayedAction CreateDelayedAction(float delaySeconds, Action action)
    {
        var delayedAction = FluentPool.Get<DelayedAction>();
        delayedAction.Delay = delaySeconds;
        delayedAction.Action = action;

        BeginTracking(delayedAction);

        return delayedAction;
    }

    public DelayedAction CreateDelayedAction<TBaseAction>(TBaseAction baseAction, Action<TBaseAction> extendedAction)
        where TBaseAction : IDelayedAction
    {
        var delayedAction = FluentPool.Get<DelayedAction>();
        delayedAction.Delay = baseAction.Delay;
        delayedAction.Action = () => extendedAction(baseAction);

        BeginTracking(baseAction);
        BeginTracking(delayedAction);

        return delayedAction;
    }

    /// <summary>
    /// Attempts to shutdown the pool and free all pending actions.
    /// If any actions are still delayed/processing, waits for completion.
    /// </summary>
    public void Shutdown()
    {
        if (_pendingActions != null)
        {
            var pending = _pendingActions.Where(x => x.State != DelayedActionState.Finished);
            if (pending.Any())
            {
                var timeout = pending.Max(x => x.SecondsUntilTimeout);
                if (timeout > 0)
                {
                    // valid actions still running with a timeout, lets wait for the longest one to complete.
                    _plugin.timer.In(timeout + 1, Shutdown);
                    return;
                }

            }

            // no valid actions remaining, return them to the pool
            FluentPool.FreeCustom(ref _pendingActions);
        }

        // now that all actions are free and complete, we can free any remaining elements
        if (_trackingElements != null)
        {
            FluentPool.FreeCustom(ref _trackingElements);
        }
    }

    public void Dispose()
    {
        if (_pendingActions != null)
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Warning: Dispose was called with {_pendingActions.Count} pending action(s). Expected list to be null.");
        }

        if (_trackingElements != null)
        {
            using var debug = FluentDebug.BeginScope();
            debug.Log($"!! Warning: Dispose was called with {_trackingElements.Count} element(s). Expected list to be null.");
        }
    }
}
