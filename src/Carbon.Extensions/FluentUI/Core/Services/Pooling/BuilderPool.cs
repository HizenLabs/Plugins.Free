using Carbon.Plugins;
using HizenLabs.FluentUI.API;
using HizenLabs.FluentUI.API.Interfaces;
using HizenLabs.FluentUI.Primitives.Enums;
using HizenLabs.FluentUI.Utils.Debug;
using HizenLabs.FluentUI.Utils.Delays;
using HizenLabs.FluentUI.Utils.Delays.Base;
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
        using var _ = FluentDebug.BeginScope();

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
        using var debug = FluentDebug.BeginScope();

        if (!_pendingActions.Contains(action))
        {
            debug.Log($"Begin tracking action: {action.GetType()}");
            _pendingActions.Add(action);
        }
        else
        {
            debug.Log($"Action already tracked: {action.GetType()}");
        }
    }

    /// <summary>
    /// Begins tracking the specified element.
    /// </summary>
    /// <param name="element">The element to track.</param>
    public void BeginTracking(IFluentElement element)
    {
        using var debug = FluentDebug.BeginScope();

        if (!_trackingElements.Contains(element))
        {
            debug.Log($"Begin tracking element: {element.GetType()}");
            _trackingElements.Add(element);
        }
        else
        {
            debug.Log($"Element already tracked: {element.GetType()}");
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
        using var debug = FluentDebug.BeginScope();

        if (_pendingActions != null)
        {
            var pending = _pendingActions.Where(x => x.State != DelayedActionState.Finished);
            if (pending.Any())
            {
                var timeout = pending.Max(x => x.SecondsUntilTimeout);
                if (timeout > 0)
                {
                    // valid actions still running with a timeout, lets wait for the longest one to complete.
                    debug.Log($"Waiting for {pending.Count()} pending actions to complete. Timeout in {timeout} seconds.");
                    _plugin.timer.In(timeout + 1, Shutdown);
                    return;
                }

                debug.Log($"Found {pending.Count()} pending actions that did not finish before timeout. Forcing shutdown.");

            }
            else
            {
                debug.Log("Found no pending actions.");
            }

            // no valid actions remaining, return them to the pool
            debug.Log($"Freeing {_pendingActions.Count} action(s).");
            FluentPool.FreeCustom(ref _pendingActions);
        }

        // now that all actions are free and complete, we can free any remaining elements
        if (_trackingElements != null)
        {
            debug.Log($"Freeing {_trackingElements.Count} element(s).");
            FluentPool.FreeCustom(ref _trackingElements);
        }
    }

    public void Dispose()
    {
        using var debug = FluentDebug.BeginScope();

        if (_pendingActions != null)
        {
            debug.Log($"!! Dispose was called with {_pendingActions.Count} pending action(s). Expected list to be null.");
        }

        if (_trackingElements != null)
        {
            debug.Log($"!! Dispose was called with {_trackingElements.Count} element(s). Expected list to be null.");
        }
    }
}
