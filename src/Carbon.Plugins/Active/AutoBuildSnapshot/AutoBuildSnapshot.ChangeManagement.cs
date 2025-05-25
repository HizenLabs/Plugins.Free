using Cysharp.Threading.Tasks;
using Facepunch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Handles recording bases for any changes.
    /// </summary>
    private static class ChangeManagement
    {
        /// <summary>
        /// The dictionary of recordings, where the key is the tool cupboard position and the value is the recording instance.
        /// </summary>
        public static IReadOnlyDictionary<RecordingId, BaseRecording> Recordings => _recordings;
        private static Dictionary<RecordingId, BaseRecording> _recordings;

        public static IReadOnlyDictionary<RecordingId, List<MetaInfo>> SnapshotsMeta => _snapshotsMeta;
        private static Dictionary<RecordingId, List<MetaInfo>> _snapshotsMeta;

        /// <summary>
        /// The dictionary of locks, where the key is the tool cupboard position and the value is the recording lock instance.
        /// </summary>
        private static Dictionary<RecordingId, RecordingLock> _locks;

        /// <summary>
        /// Initializes the change management system.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        public static void Init()
        {
            _recordings = Pool.Get<Dictionary<RecordingId, BaseRecording>>();
            _snapshotsMeta = Pool.Get<Dictionary<RecordingId, List<MetaInfo>>>();
            _locks = Pool.Get<Dictionary<RecordingId, RecordingLock>>();

            var buildings = BaseNetworkable.serverEntities.OfType<BuildingPrivlidge>();
            Helpers.Log(LangKeys.message_init_recordings, null, buildings.Count());

            foreach (var tc in buildings)
            {
                StartRecording(tc);
            }

            ChangeMonitor.Start();
        }

        /// <summary>
        /// Frees the change management resources.
        /// </summary>
        public static void Unload()
        {
            ChangeMonitor.Stop();

            Pool.Free(ref _locks, true);
            Pool.Free(ref _recordings, true);

            foreach (var kvp in _snapshotsMeta)
            {
                var list = kvp.Value;
                Pool.FreeUnmanaged(ref list);
            }

            Pool.FreeUnmanaged(ref _snapshotsMeta);
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
        /// Handles changes to an entity and records them if applicable.
        /// </summary>
        /// <param name="entity">The entity that was changed.</param>
        /// <param name="action">The action that was performed on the entity.</param>
        /// <param name="player">Optionally, the player that made the change.</param>
        public static void HandleChange(BaseEntity entity, ChangeAction action, BasePlayer player = null)
        {
            if ((action == ChangeAction.Kill || action == ChangeAction.Decay)
                && entity is BuildingPrivlidge priv)
            {
                StopRecording(priv);
                return;
            }

            var tc = entity.GetBuildingPrivilege();

            if (tc && TryGetRecording(tc, out var recording))
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
            if (!priv || TryGetRecording(priv, out _)) return;

            var recording = Pool.Get<BaseRecording>();

            if (!recording.TryInitialize(priv))
            {
                Pool.Free(ref recording);
                return;
            }

            _recordings.Add(recording.Id, recording);
        }

        /// <summary>
        /// Stops recording the changes for the given tool cupboard.
        /// </summary>
        /// <param name="priv">The tool cupboard to stop recording.</param>
        private static void StopRecording(BuildingPrivlidge priv)
        {
            if (priv && TryGetRecording(priv, out var recording))
            {
                _recordings.Remove(recording.Id);

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
        private static bool TryGetRecording(BuildingPrivlidge priv, out BaseRecording recording)
        {
            if (!priv)
            {
                recording = null;
                return false;
            }

            var recordingId = RecordingId.For(priv);

            return _recordings.TryGetValue(recordingId, out recording);
        }

        /// <summary>
        /// Gets the ID of the entity. This is not persistant as IDs are pooled and reused.
        /// </summary>
        /// <param name="entity">The entity to get the ID for.</param>
        /// <returns>The ID of the entity.</returns>
        private static ulong GetEntityId(BaseEntity entity)
        {
            return entity.net.ID.Value;
        }

        /// <summary>
        /// Represents a recording of a tool cupboard and its associated building.
        /// </summary>
        internal class BaseRecording : Pool.IPooled
        {
            /// <summary>
            /// The id of the base recording (tc net id).
            /// </summary>
            public RecordingId Id => RecordingId.For(BaseTC);

            /// <summary>
            /// A record might still exist even after the TC is destroyed.
            /// So, we need to have a flag to determine if it is active.
            /// </summary>
            public bool IsActive => BaseTC && !BaseTC.IsDestroyed && BaseTC.isSpawned;

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
            public IReadOnlyList<ChangeRecord> ChangeRecords => _changeRecords;
            private List<ChangeRecord> _changeRecords;

            /// <summary>
            /// The list of changes that have been made to this record since the last save.
            /// </summary>
            public IEnumerable<ChangeRecord> PendingRecords
            {
                get
                {
                    var lastSaveTime = LastSuccessfulSaveTime;
                    return ChangeRecords.Where(cr => cr.TimeStamp > lastSaveTime);
                }
            }

            /// <summary>
            /// The time of the last change made to this recording.
            /// </summary>
            public ChangeRecord LastChangeRecord => _changeRecords.Count > 0 ? _changeRecords[^1] : null;

            /// <summary>
            /// The list of save attempts for this recording.
            /// </summary>
            public IReadOnlyList<SaveAttempt> SaveAttempts => _saveAttempts;
            private List<SaveAttempt> _saveAttempts;

            /// <summary>
            /// The time of the last save attempt for this recording.
            /// </summary>
            public SaveAttempt LastSaveAttempt => _saveAttempts.Count > 0 ? _saveAttempts[^1] : null;

            /// <summary>
            /// The time of the last successful save attempt for this recording.
            /// </summary>
            public DateTime LastSuccessfulSaveTime => LastSaveAttempt?.EndTime ?? DateTime.MinValue;

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

                if (IsValid)
                {
                    HandleChange(priv, ChangeAction.Create);
                }

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
                var changeRecord = ChangeRecord.Create(entity, action, player);

                _changeRecords.Add(changeRecord);
            }

            /// <summary>
            /// Attempts to save the current state of the recording.
            /// </summary>
            /// <returns>A task representing the asynchronous save operation.</returns>
            public async UniTask<SaveAttempt> AttemptSaveAsync(BasePlayer player = null)
            {
                var saveAttempt = Pool.Get<SaveAttempt>();
                try
                {
                    saveAttempt.Init(this, player);

                    using var recordingLock = CreateLock(player);

                    await SaveManager.SaveAsync(this, player);

                    saveAttempt.UpdateResult(true);
                }
                catch (Exception ex)
                {
                    Helpers.Log(LangKeys.error_save_fail, player, Id, BaseTC.ServerPosition, ex.Message);

                    saveAttempt.UpdateResult(ex);
                }
                finally
                {
                    _saveAttempts.Add(saveAttempt);
                }

                return saveAttempt;
            }

            /// <summary>
            /// Creates a lock for the recording to prevent concurrent access.
            /// </summary>
            /// <param name="player">The player requesting the lock.</param>
            /// <returns>The created lock.</returns>
            /// <exception cref="LocalizedException">Thrown if the recording is already locked.</exception>
            private RecordingLock CreateLock(BasePlayer player)
            {
                if (!RecordingLock.TryCreate(this, out var recordingLock))
                {
                    throw new LocalizedException(LangKeys.error_recording_locked, player);
                }

                return recordingLock;
            }

            /// <summary>
            /// Enters the pool and frees up resources.
            /// </summary>
            public void EnterPool()
            {
                BaseTC = null;
                Building = null;

                Pool.Free(ref _changeRecords, true);
                Pool.Free(ref _saveAttempts, true);
            }

            /// <summary>
            /// Leaves the pool and sets up the recording.
            /// </summary>
            public void LeavePool()
            {
                _changeRecords = Pool.Get<List<ChangeRecord>>();
                _saveAttempts = Pool.Get<List<SaveAttempt>>();
            }
        }

        internal readonly record struct RecordingId
        (
            [JsonProperty]Vector3 Position,
            [JsonProperty]Quaternion Rotation
        )
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static RecordingId For(BuildingPrivlidge tc)
            {
                return new RecordingId(tc.ServerPosition, tc.ServerRotation);
            }
        }

        /// <summary>
        /// Represents a lock for a recording, used to prevent concurrent access to the recording.
        /// </summary>
        internal class RecordingLock : IDisposable, Pool.IPooled
        {
            private static readonly object _lock = new();

            /// <summary>
            /// The state of the lock.
            /// </summary>
            public bool IsLocked { get; private set; }

            /// <summary>
            /// The recording that this lock is associated with.
            /// </summary>
            public BaseRecording Recording { get; private set; }

            /// <summary>
            /// Creates a lock for the given recording and returns it.
            /// </summary>
            /// <param name="recording">The recording to lock.</param>
            /// <returns>The created lock.</returns>
            public static bool TryCreate(BaseRecording recording, out RecordingLock recordingLock)
            {
                recordingLock = null;

                lock (_lock)
                {
                    if (_locks.ContainsKey(recording.Id))
                    {
                        return false;
                    }

                    recordingLock = Pool.Get<RecordingLock>();
                    recordingLock.Recording = recording;

                    _locks.Add(recording.Id, recordingLock);

                    return true;
                }
            }

            /// <summary>
            /// Releases the lock on the recording.
            /// </summary>
            public void Dispose()
            {
                var obj = this;
                Pool.Free(ref obj);
            }

            /// <summary>
            /// Enters the pool and frees up resources.
            /// </summary>
            public void EnterPool()
            {
                if (Recording.IsValid)
                {
                    lock (_lock)
                    {
                        if (_locks.TryGetValue(Recording.Id, out var existing)
                            && existing == this)
                        {
                            _locks.Remove(Recording.Id);
                        }
                    }
                }

                IsLocked = false;
                Recording = null;
            }

            /// <summary>
            /// Leaves the pool and sets up the recording lock.
            /// </summary>
            public void LeavePool()
            {
                IsLocked = true;
            }
        }

        /// <summary>
        /// Represents a change record for an entity.
        /// </summary>
        internal class ChangeRecord : Pool.IPooled
        {
            /// <summary>
            /// The time that the change was made.
            /// </summary>
            public DateTime TimeStamp { get; private set; }

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
            /// Creates a change record with the given parameters.
            /// </summary>
            /// <param name="entity">The entity that was changed.</param>
            /// <param name="action">The action that was performed.</param>
            /// <param name="player">Optionally, the player that made the change.</param>
            /// <returns>True if the change record was successfully initialized, false otherwise.</returns>
            public static ChangeRecord Create(BaseEntity entity, ChangeAction action, BasePlayer player = null)
            {
                var changeRecord = Pool.Get<ChangeRecord>();
                changeRecord.Entity = entity;
                changeRecord.Action = action;
                changeRecord.Player = player;
                return changeRecord;
            }

            /// <summary>
            /// Enters the pool and frees up resources.
            /// </summary>
            public void EnterPool()
            {
                TimeStamp = default;
                Action = default;
                Entity = null;
                Player = null;
            }

            /// <summary>
            /// Leaves the pool and sets up the change record.
            /// </summary>
            public void LeavePool()
            {
                TimeStamp = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Represents a save attempt for a recording.
        /// </summary>
        internal class SaveAttempt : Pool.IPooled
        {
            /// <summary>
            /// The time that the save attempt started.
            /// </summary>
            public DateTime StartTime { get; private set; }

            /// <summary>
            /// The time that the save attempt ended.
            /// </summary>
            public DateTime EndTime { get; private set; }

            /// <summary>
            /// The duration of the save attempt.
            /// </summary>
            public TimeSpan Duration => EndTime - StartTime;

            /// <summary>
            /// The id of the base that was saved.
            /// </summary>
            public RecordingId RecordingId { get; private set; }

            /// <summary>
            /// The player that initiated the save attempt (if any).
            /// </summary>
            public BasePlayer Player { get; private set; }

            /// <summary>
            /// Whether the save attempt was successful or not.
            /// </summary>
            public bool Success { get; private set; }

            /// <summary>
            /// The exception that occurred during the save attempt, if any.
            /// </summary>
            public Exception Exception { get; private set; }

            /// <summary>
            /// Initializes the save attempt with the given player.
            /// </summary>
            /// <param name="player">The player that initiated the save attempt.</param>
            public void Init(BaseRecording recording, BasePlayer player = null)
            {
                if (player != null && !Settings.Commands.Backup.HasPermission(player))
                {
                    throw new LocalizedException(LangKeys.error_no_permission, player);
                }

                RecordingId = recording.Id;
                Player = player;
                StartTime = DateTime.UtcNow;
            }

            /// <summary>
            /// Updates the result of the save attempt with the given exception.
            /// </summary>
            /// <param name="ex">The exception that occurred during the save attempt.</param>
            public void UpdateResult(Exception ex)
            {
                Exception = ex;

                UpdateResult(false);
            }

            /// <summary>
            /// Updates the result of the save attempt with the given success status.
            /// </summary>
            /// <param name="success">Whether the save attempt was successful or not.</param>
            public void UpdateResult(bool success)
            {
                Success = success;
                EndTime = DateTime.UtcNow;
            }

            /// <summary>
            /// Enters the pool and frees up resources.
            /// </summary>
            public void EnterPool()
            {
                StartTime = default;
                EndTime = default;
                Success = false;
                Exception = null;
            }

            /// <summary>
            /// Leaves the pool and sets up the save attempt.
            /// </summary>
            public void LeavePool()
            {
            }
        }
    }

    /// <summary>
    /// Represents the action that was performed on the entity.
    /// </summary>
    private enum ChangeAction
    {
        /// <summary>
        /// The entity was created.
        /// </summary>
        Create,
        /// <summary>
        /// The entity was updated.
        /// </summary>
        Update,
        /// <summary>
        /// The entity was decayed.
        /// </summary>
        Decay,
        /// <summary>
        /// The entity was killed.
        /// </summary>
        Kill
    }
}
