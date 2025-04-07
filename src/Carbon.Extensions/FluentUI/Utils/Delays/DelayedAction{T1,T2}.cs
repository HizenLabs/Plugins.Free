using Carbon.Plugins;
using HizenLabs.FluentUI.Abstractions;
using System;

namespace HizenLabs.FluentUI.Utils.Delays;

internal class DelayedAction<T1, T2> : DelayedActionBase
{
    /// <summary>
    /// The action to be executed after the delay. Meant to be used as a setter only. 
    /// Use Execute(...) when ready to process so that time metrics are calculated properly.
    /// </summary>
    public Action<T1, T2> Action { private get; set; }

    /// <summary>
    /// Prepares the timer with the delay and action specified.
    /// </summary>
    /// <param name="plugin">The plugin to run the timer on.</param>
    /// <param name="obj1">The first object to pass to the action.</param>
    /// <param name="obj2">The second object to pass to the action.</param>
    public void ExecuteTimer(CarbonPlugin plugin, T1 obj1, T2 obj2) =>
        ExecuteTimerInternal(plugin, () => Action?.Invoke(obj1, obj2));

    /// <inheritdoc/>
    protected override void CleanupAction()
    {
        Action = null;
    }
}
