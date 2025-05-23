using Facepunch;
#if DEBUG
using System;
#endif

namespace HizenLabs.Extensions.UserPreference.Pooling;

internal interface ITrackedPooled : Pool.IPooled
{
#if DEBUG
    Guid TrackingId { get; set; }
#endif
}
