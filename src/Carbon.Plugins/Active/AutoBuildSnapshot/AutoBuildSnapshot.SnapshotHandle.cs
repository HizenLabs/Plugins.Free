using Epic.OnlineServices;
using Facepunch;
using Oxide.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private class SnapshotHandle : Pool.IPooled
    {
        public Guid ID => Meta.ID;

        public BuildSnapshotMetaData Meta { get; private set; }

        public BasePlayer Player { get; private set; }

        public ulong PlayerUserID => Player.userID;

        public SnapshotState State { get; private set; }

        public DateTime LastModified { get; private set; }

        public DateTime Expiration => LastModified.AddSeconds(_maxSnapshotHandleDuration);

        public TimeSpan TimeSinceModified => DateTime.UtcNow - LastModified;

        public TimeSpan Remaining => Expiration - DateTime.UtcNow;

        public Timer PreviewTimer { get; private set; }

        public void PreviewZones()
        {
            // DISABLED until further notice
            // We could make server entities and expand the radius of the sphere that way,
            // but it's just ugly and I don't think it makes sense.
            // We'll just keep the preview rollback option and call it a day
        }

        public void PreviewRollback()
        {
            // DISABLED until further notice
            // ClientEntity just doesn't work properly for this
            // We have to do some other major refactoring to make this work
        }

        private void PreviewStop(SnapshotState stopState, ulong playerId)
        {
            if (playerId != PlayerUserID)
                return;

            if (State == stopState)
            {
                KillTempEntities(PlayerUserID);
                PreviewTimer?.Destroy();
                PreviewTimer?.Dispose();
            }
            else
            {
                // if it's already idle, processing a rollback, or locked, leave it alone
                return;
            }

            Update(SnapshotState.Idle);
        }

        private void Update(SnapshotState state)
        {
            State = state;
            LastModified = DateTime.UtcNow;
        }

        public static void Release(BasePlayer player)
        {
            if (_playerSnapshotHandles.TryGetValue(player.userID, out var handleId))
            {
                if (_snapshotHandles.TryGetValue(handleId, out var handle))
                {
                    Pool.Free(ref handle);

                    _snapshotHandles.Remove(handleId);
                }

                _playerSnapshotHandles.Remove(player.userID);
            }
        }

        public static bool TryTake(BuildSnapshotMetaData meta, BasePlayer player, out SnapshotHandle handle)
        {
            if (_playerSnapshotHandles.TryGetValue(player.userID, out var existingHandleId)
                && existingHandleId != meta.ID)
            {
                Release(player);
            }

            if (_snapshotHandles.TryGetValue(meta.ID, out handle))
            {
                if (handle.Expiration < DateTime.UtcNow)
                {
                    handle.Player = player;
                }
            }
            else
            {
                handle = Pool.Get<SnapshotHandle>();
                handle.Meta = meta;
                handle.Player = player;

                _snapshotHandles[meta.ID] = handle;
            }

            if (handle.PlayerUserID == player.userID)
            {
                handle.LastModified = DateTime.UtcNow;

                _playerSnapshotHandles[player.userID] = handle.ID;

                return true;
            }

            return false;
        }

        public void EnterPool()
        {
            Meta = default;
            Player = null;
            LastModified = DateTime.MinValue;
        }

        public void LeavePool()
        {
            State = SnapshotState.Idle;
        }
    }

    /// <summary>
    /// Represents the state of a snapshot.
    /// </summary>
    private enum SnapshotState
    {
        /// <summary>
        /// The snapshot is idle and not processing.
        /// </summary>
        Idle,

        /// <summary>
        /// The snapshot zones previewing is enabled.
        /// </summary>
        PreviewZones,

        /// <summary>
        /// The snapshot rollback preview is enabled.
        /// </summary>
        PreviewRollback,

        /// <summary>
        /// The snapshot rollback is in progress.
        /// </summary>
        ProcessRollback,

        /// <summary>
        /// The snapshot is locked and cannot be modified.
        /// </summary>
        Locked,
    }
}
