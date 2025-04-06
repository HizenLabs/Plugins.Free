using Carbon;
using Carbon.Plugins;
using Facepunch;
using HizenLabs.FluentUI.Abstractions;
using HizenLabs.FluentUI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HizenLabs.FluentUI.Internals;

/// <summary>
/// A custom pool used to manage <see cref="FluentBuilder"/> objects.
/// Handles disposal of timer objects that might run after a builder is disposed.
/// </summary>
internal class BuilderPool
{
    private readonly CarbonPlugin _plugin;
    private List<IDelayedAction> _pendingActions;

    public BuilderPool(CarbonPlugin plugin)
    {
        _plugin = plugin;
        _pendingActions = Pool.Get<List<IDelayedAction>>();
    }

    /// <summary>
    /// Begins tracking the specified delayed action.
    /// </summary>
    /// <param name="action">The delayed action to track.</param>
    public void BeginTracking(IDelayedAction action)
    {
        Logger.Debug("test");

        if (!_pendingActions.Contains(action))
        {
            _pendingActions.Add(action);
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
        var delayedAction = Pool.Get<DelayedAction>();
        delayedAction.Delay = delaySeconds;
        delayedAction.Action = action;

        BeginTracking(delayedAction);

        return delayedAction;
    }

    /// <summary>
    /// Attempts to shutdown the pool and free all pending actions.
    /// If any actions are still delayed/processing, waits for completion.
    /// </summary>
    public void Shutdown()
    {
        if (_pendingActions == null)
        {
            return;
        }

        if (_pendingActions.Count == 0)
        {
            Pool.FreeUnmanaged(ref _pendingActions);
            return;
        }

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
        PoolHelper.FreeActions(ref _pendingActions);
    }
}
