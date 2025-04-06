using Carbon.Plugins;
using HizenLabs.FluentUI.Enums;
using Oxide.Plugins;
using System;

namespace HizenLabs.FluentUI.Abstractions;

internal abstract class DelayedActionBase : IDelayedAction
{
    /// <inheritdoc/>
    public DelayedActionState State { get; private set; }

    /// <inheritdoc/>
    public float Delay { get; set; }

    /// <inheritdoc/>
    public DateTime? DelayStartTimeUtc { get; private set; }

    /// <inheritdoc/>
    public DateTime? ProcessStartTimeUtc { get; private set; }

    /// <inheritdoc/>
    public DateTime? ProcessEndTimeUtc { get; private set; }

    /// <inheritdoc/>
    protected Timer Timer { get; private set; }

    /// <inheritdoc/>
    public float SecondsUntilTimeout
    {
        get
        {
            if (DelayStartTimeUtc.HasValue)
            {
                var procStart = ProcessStartTimeUtc ?? DelayStartTimeUtc.Value.AddSeconds(Delay);
                return (float)procStart
                    .AddSeconds(ProcessTimeoutSeconds)
                    .Subtract(DateTime.UtcNow).TotalSeconds;
            }
            
            // the process is invalid, return expiration
            return 0f;
        }
    }

    /// <summary>
    /// The time that the action is allowed before being considered expired.
    /// This is used in disposal calculations.
    /// </summary>
    /// <remarks>
    /// Used for <see cref="BuilderPool.Shutdown"/> to determine if the action is still valid
    /// before force terminating the process and cleaning up its resources.
    /// </remarks>
    public float ProcessTimeoutSeconds { get; private set; }

    /// <summary>
    /// Executes the timer with the specified action.
    /// This is meant to be used by implementing classes.
    /// </summary>
    /// <param name="plugin">The plugin to run the timer on.</param>
    protected void ExecuteTimerInternal(CarbonPlugin plugin, Action action)
    {
        State = DelayedActionState.WaitingForDelay;

        DelayStartTimeUtc = DateTime.UtcNow;
        Timer = plugin.timer.In(Delay, () =>
        {
            State = DelayedActionState.Running;
            ProcessStartTimeUtc = DateTime.UtcNow;

            action?.Invoke();

            ProcessEndTimeUtc = DateTime.UtcNow;
            State = DelayedActionState.Finished;
        });
    }

    /// <summary>
    /// For implementing classes to null out their action 
    /// and prepare it to enter the pool again.
    /// </summary>
    protected abstract void CleanupAction();

    /// <inheritdoc/>
    public virtual void EnterPool()
    {
        State = DelayedActionState.InPool;

        CleanupAction();

        Delay = 0f;
        DelayStartTimeUtc = null;
        ProcessStartTimeUtc = null;
        ProcessEndTimeUtc = null;
        ProcessTimeoutSeconds = 5f;

        Timer = null;
    }

    /// <inheritdoc/>
    public virtual void LeavePool()
    {
        State = DelayedActionState.Created;
    }
}
