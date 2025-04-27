using Facepunch;
using System;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private class SnapshotHandle : Pool.IPooled
    {
        private Guid _confirmationCode = Guid.Empty;

        public Guid ID => Meta.ID;

        public BuildSnapshotMetaData Meta { get; private set; }

        public BasePlayer Player { get; private set; }

        public ulong PlayerUserID => Player.userID;

        public SnapshotState State { get; private set; }

        public DateTime LastModified { get; private set; }

        public DateTime Expiration => LastModified.AddSeconds(_maxSnapshotHandleDuration);

        public TimeSpan TimeSinceModified => DateTime.UtcNow - LastModified;

        public TimeSpan Remaining => Expiration - DateTime.UtcNow;

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
            }
            else
            {
                // if it's already idle, processing a rollback, or locked, leave it alone
                return;
            }

            Update(SnapshotState.Idle);
        }

        /// <summary>
        /// Attempts to lock the snapshot for a rollback operation.
        /// </summary>
        /// <param name="player">The player initiating the rollback.</param>
        /// <param name="confirmationCode">The confirmation code created for the lock.</param>
        /// <returns>The confirmation code to pass when beginning the rollback.</returns>
        public bool TryConfirmationLock(BasePlayer player, out Guid confirmationCode)
        {
            confirmationCode = Guid.NewGuid();

            if (player.userID != PlayerUserID)
            {
                player.ChatMessage($"Snapshot handle currently belongs to another player: '{Player.displayName}'");
                return false;
            }

            if (State != SnapshotState.Idle)
            {
                player.ChatMessage($"Snapshot state is not idle (State: {State})");
                return false;
            }

            Update(SnapshotState.Locked);

            _confirmationCode = confirmationCode;

            return true;
        }

        public bool TryConfirmationCancel(BasePlayer player)
        {
            if (player.userID != PlayerUserID)
            {
                player.ChatMessage($"Snapshot handle currently belongs to another player: '{Player.displayName}'");
                return false;
            }

            if (State != SnapshotState.Locked)
            {
                player.ChatMessage($"Snapshot state is not locked (State: {State})");
                return false;
            }

            Update(SnapshotState.Idle);

            _confirmationCode = Guid.Empty;

            return true;
        }

        /// <summary>
        /// Attempts to start a rollback operation.
        /// </summary>
        /// <param name="player">The player initiating the rollback.</param>
        /// <param name="confirmationCode">The confirmation code for the rollback.</param>
        /// <returns>True if the rollback was started successfully; otherwise, false.</returns>
        public bool TryBeginRollback(BasePlayer player, Guid confirmationCode)
        {
            if (confirmationCode != _confirmationCode)
                return false;

            if (State != SnapshotState.Locked)
                return false;

            if (player.userID != PlayerUserID)
                return false;

            _instance.BeginRollback(this);

            return true;
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
