using Facepunch;
using System;
using System.Collections.Generic;

namespace Carbon.Plugins.Active.AutoBuildSnapshot;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Handles recording bases for any changes.
    /// </summary>
    private static class ChangeManagement
    {
        private static Dictionary<ulong, BaseRecording> _recordings;

        /// <summary>
        /// Initializes the change management system.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        public static void Init(AutoBuildSnapshot plugin)
        {
            _recordings = Pool.Get<Dictionary<ulong, BaseRecording>>();
        }

        /// <summary>
        /// Frees the change management resources.
        /// </summary>
        public static void Unload()
        {
            Pool.Free(ref _recordings, true);
        }

        /// <summary>
        /// Handles the spawning of an entity and starts recording if it's a tool cupboard.
        /// </summary>
        /// <param name="networkable">The BaseNetworkable that was spawned.</param>
        public static void HandleOnEntitySpawned(BaseNetworkable networkable)
        {
            if (networkable is BuildingPrivlidge priv)
            {
                StartRecording(priv);
            }
        }

        /// <summary>
        /// Handles the destruction of an entity and stops recording if it's a tool cupboard.
        /// </summary>
        /// <param name="networkable">The BaseNetworkable that was destroyed.</param>
        public static void HandleOnEntityKill(BaseNetworkable networkable)
        {
            if (networkable is BuildingPrivlidge priv)
            {
                StopRecording(priv);
            }
            else if (networkable is BaseEntity entity)
            {
                HandleChange(entity, ChangeAction.Kill);
            }
        }

        /// <summary>
        /// Handles changes to an entity and records them if applicable.
        /// </summary>
        /// <param name="entity">The entity that was changed.</param>
        /// <param name="action">The action that was performed on the entity.</param>
        /// <param name="player">Optionally, the player that made the change.</param>
        public static void HandleChange(BaseEntity entity, ChangeAction action, BasePlayer player = null)
        {
            var tc = entity.GetBuildingPrivilege();

            if (TryGetRecording(tc, out var recordingId, out var recording))
            {
                recording.HandleChange(entity, action, player);
            }
        }

        /// <summary>
        /// Starts recording the changes for the given tool cupboard.
        /// </summary>
        /// <param name="priv">The tool cupboard to record.</param>
        private static void StartRecording(BuildingPrivlidge priv)
        {
            if (TryGetRecording(priv, out var recordingId, out _)) return;

            var recording = Pool.Get<BaseRecording>();

            if (!recording.TryInitialize(priv))
            {
                Pool.Free(ref recording);
                return;
            }

            _recordings.Add(recordingId, recording);
        }

        /// <summary>
        /// Stops recording the changes for the given tool cupboard.
        /// </summary>
        /// <param name="priv">The tool cupboard to stop recording.</param>
        private static void StopRecording(BuildingPrivlidge priv)
        {
            if (TryGetRecording(priv, out var recordingId, out var recording))
            {
                _recordings.Remove(recordingId);

                Pool.Free(ref recording);
            }
        }

        /// <summary>
        /// Attempts to get the recording associated with the given tool cupboard.
        /// </summary>
        /// <param name="priv">The tool cupboard to check.</param>
        /// <param name="recordingId">The ID of the recording.</param>
        /// <param name="recording">The recording associated with the tool cupboard.</param>
        /// <returns>True if the recording was found, false otherwise.</returns>
        private static bool TryGetRecording(BuildingPrivlidge priv, out ulong recordingId, out BaseRecording recording)
        {
            recordingId = priv.net.ID.Value;

            return _recordings.TryGetValue(recordingId, out recording);
        }

        /// <summary>
        /// Represents a recording of a tool cupboard and its associated building.
        /// </summary>
        private class BaseRecording : Pool.IPooled
        {
            /// <summary>
            /// The base tool cupboard this recording is tracking.
            /// </summary>
            public BuildingPrivlidge BaseTC { get; private set; }

            /// <summary>
            /// The building associated with the <see cref="BaseTC"/>.
            /// </summary>
            public BuildingManager.Building Building { get; private set; }

            /// <summary>
            /// The list of changes that have been made to this record since the last save.
            /// </summary>
            public IReadOnlyList<ChangeRecord> ChangeLog => _changeLog;
            private List<ChangeRecord> _changeLog;

            /// <summary>
            /// Indicates whether the recording is valid and can be used.
            /// </summary>
            public bool IsValid => BaseTC && Building != null;

            /// <summary>
            /// Initializes the recording with the given tool cupboard.
            /// </summary>
            /// <param name="priv">The tool cupboard to record.</param>
            /// <returns>True if the recording was successfully initialized, false otherwise.</returns>
            public bool TryInitialize(BuildingPrivlidge priv)
            {
                BaseTC = priv;
                Building = priv.GetBuilding();

                return IsValid;
            }

            /// <summary>
            /// Handles the change of an entity and records it in the change log.
            /// </summary>
            /// <param name="entity">The entity that was changed.</param>
            /// <param name="action">The action that was performed on the entity.</param>
            /// <param name="player">Optionally, the player that made the change.</param>
            public void HandleChange(BaseEntity entity, ChangeAction action, BasePlayer player = null)
            {
                var changeRecord = Pool.Get<ChangeRecord>();
                changeRecord.Init(entity, action, player);

                _changeLog.Add(changeRecord);
            }

            /// <summary>
            /// Enters the pool and frees up resources.
            /// </summary>
            public void EnterPool()
            {
                BaseTC = null;
                Building = null;

                Pool.Free(ref _changeLog, true);
            }

            /// <summary>
            /// Leaves the pool and sets up the recording.
            /// </summary>
            public void LeavePool()
            {
                _changeLog = Pool.Get<List<ChangeRecord>>();
            }
        }

        /// <summary>
        /// Represents a change record for an entity.
        /// </summary>
        private class ChangeRecord : Pool.IPooled
        {
            /// <summary>
            /// The time that the change was made.
            /// </summary>
            public DateTime Time { get; private set; }

            /// <summary>
            /// The entity that was changed.
            /// </summary>
            public BaseEntity Entity { get; private set; }

            /// <summary>
            /// The action that was performed on the entity.
            /// </summary>
            public ChangeAction Action { get; private set; }

            /// <summary>
            /// The id of the player that made the change.
            /// </summary>
            public BasePlayer Player { get; private set; }

            /// <summary>
            /// Initializes the change record with the given parameters.
            /// </summary>
            /// <param name="entity">The entity that was changed.</param>
            /// <param name="action">The action that was performed.</param>
            /// <param name="player">Optionally, the player that made the change.</param>
            /// <returns>True if the change record was successfully initialized, false otherwise.</returns>
            public void Init(BaseEntity entity, ChangeAction action, BasePlayer player = null)
            {
                Action = action;
                Entity = entity;
                Player = player;
            }

            /// <summary>
            /// Enters the pool and frees up resources.
            /// </summary>
            public void EnterPool()
            {
                Time = default;
                Action = default;
                Entity = null;
                Player = null;
            }

            /// <summary>
            /// Leaves the pool and sets up the change record.
            /// </summary>
            public void LeavePool()
            {
                Time = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Represents the action that was performed on the entity.
    /// </summary>
    private enum ChangeAction
    {
        Create,
        Update,
        Decay,
        Kill
    }
}
