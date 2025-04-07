using Carbon.Plugins;
using HizenLabs.FluentUI.Abstractions;
using System;

namespace HizenLabs.FluentUI.Utils.Delays;

internal class DelayedAction : DelayedActionBase
{
    /// <summary>
    /// The action to be executed after the delay. Meant to be used as a setter only. 
    /// Use Execute(...) when ready to process so that time metrics are calculated properly.
    /// </summary>
    public Action Action { private get; set; }

    /// <summary>
    /// Prepares the timer with the delay and action specified.
    /// </summary>
    /// <param name="plugin">The plugin to run the timer on.</param>
    public void ExecuteTimer(CarbonPlugin plugin) =>
        ExecuteTimerInternal(plugin, Action);

    /// <inheritdoc/>
    protected override void CleanupAction()
    {
        Action = null;
    }
}
