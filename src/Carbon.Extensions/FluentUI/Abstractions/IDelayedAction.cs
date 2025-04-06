﻿using Facepunch;
using HizenLabs.FluentUI.Enums;
using HizenLabs.FluentUI.Internals;
using System;

namespace HizenLabs.FluentUI.Abstractions;

internal interface IDelayedAction : Pool.IPooled
{
    /// <summary>
    /// The current state of the delayed action.
    /// </summary>
    DelayedActionState State { get; }

    /// <summary>
    /// The time to wait before executing the action.
    /// </summary>
    float Delay { get; set; }

    /// <summary>
    /// The time that the execute was called, starting the timer.
    /// </summary>
    DateTime? DelayStartTimeUtc { get; }

    /// <summary>
    /// The time that the action started executing.
    /// </summary>
    DateTime? ProcessStartTimeUtc { get; }

    /// <summary>
    /// The time that the action finished executing.
    /// </summary>
    DateTime? ProcessEndTimeUtc { get; }

    /// <summary>
    /// The time that the action is allowed before being considered expired.
    /// This is used in disposal calculations.
    /// </summary>
    /// <remarks>
    /// Used for <see cref="BuilderPool.Shutdown"/> to determine if the action is still valid
    /// before force terminating the process and cleaning up its resources.
    /// </remarks>
    float ProcessTimeoutSeconds { get; }

    /// <summary>
    /// The calculated amount of seconds based on the start time, delay and timeout
    /// until the action is considered expired.
    /// </summary>
    float SecondsUntilTimeout { get; }
}
