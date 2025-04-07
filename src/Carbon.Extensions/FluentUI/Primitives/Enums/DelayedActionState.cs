namespace HizenLabs.FluentUI.Primitives.Enums;

/// <summary>
/// Represents the state of a delayed action.
/// </summary>
internal enum DelayedActionState
{
    /// <summary>
    /// The action is in the pool (null).
    /// </summary>
    InPool,

    /// <summary>
    /// The action is created, but not started.
    /// </summary>
    Created,

    /// <summary>
    /// The action is started and waiting for timer delay.
    /// </summary>
    WaitingForDelay,

    /// <summary>
    /// The action delay is complete and is actually running.
    /// </summary>
    Running,

    /// <summary>
    /// The action is finished running (not released to pool yet).
    /// </summary>
    Finished
}
