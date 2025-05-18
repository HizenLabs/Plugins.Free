using Facepunch;
using System;

namespace HizenLabs.Extensions.UserPreference.Pooling;

internal interface ITrackedPooled : Pool.IPooled
{
    Guid TrackingId { get; set; }
}
