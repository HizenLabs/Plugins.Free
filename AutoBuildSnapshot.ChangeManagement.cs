using Facepunch;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // fully qualifying method params due to issues with codegen

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Initializes the timer to monitor builds.
    /// This method will clean up any existing timer.
    /// </summary>
    /// <param name="interval">The interval, in seconds, to check for build updates.</param>
    private void InitBuildMonitor(float interval)
    {
        _buildMonitor?.Destroy();

        _buildMonitor = timer.Every(_config.General.BuildMonitorInterval, BuildMonitorCycle);
    }

    #region Processing

    /// <summary>
    /// Cycles through the build monitor, checking for changes and processing them.
    /// </summary>
    private void BuildMonitorCycle()
    {
        if (_buildRecords is null or { Count: 0 })
            return;

        foreach (var record in _buildRecords.Values)
        {
            // process retry attempts
            if (record.State == BuildState.Save_Failure)
            {
                if (record.RetryCount >= _config.Advanced.MaxSaveFailRetries)
                {
                    AddLogMessage($"Build save had {record.RetryCount} retry attempt(s), terminating");
                    record.Update(BuildState.RetryLimitExceeded);
                }
                else
                {
                    AddLogMessage($"Queue build save for retry attempt {record.RetryCount + 1}/{_config.Advanced.MaxSaveFailRetries}");
                    record.Update(BuildState.Queued);
                }
            }
            // process anything that is currently modified (including those just flagged above)
            else if (record.State == BuildState.Modified)
            {
                // Update anything set to 'modified' to 'queued' if applicable (> save delay span)
                if (DateTime.Now - record.LastSaveSuccess > TimeSpan.FromSeconds(_config.General.DelayBetweenSaves))
                {
                    record.Update(BuildState.Queued);
                }
            }
        }

        // trigger process queue
        ProcessNextSave(recursive: true);
    }

    /// <summary>
    /// Processes the next save in the queue.
    /// </summary>
    /// <param name="record">The record to process. If null, will find the next queued record.</param>
    /// <param name="callback">The callback to invoke when the save is complete.</param>
    /// <param name="recursive">Whether to continue to process queued saves (only meant to be called from the build monitor process).</param>
    private void ProcessNextSave(BuildRecord record = null, System.Action<bool, BuildSnapshot> callback = null, bool recursive = false)
    {
        if (record == null)
        {
            // Check if we have anything to process
            var queue = _buildRecords.Where(r => r.Value.State == BuildState.Queued);
            if (!queue.Any())
            {
                return;
            }

            // Get the next process item and flag that it is currently processing
            var next = queue.First();
            record = next.Value;
        }

        record.Update(BuildState.Processing);

        var snapshot = Pool.Get<BuildSnapshot>();
        snapshot.Init(this, record, (success, record) =>
        {
            if (success)
            {
                AddLogMessage($"Saved {record.LinkedRecords.Count} building(s) in {record.FrameCount} frames (total: {record.Duration} ms | longest: {record.LongestStepDuration} ms)");
            }
            else
            {
                if (record.Exception != null)
                {
                    AddLogMessage($"Failed to save {record.LinkedRecords.Count} building(s), reason: {record.Exception}");
                }
                else
                {
                    AddLogMessage($"Failed to save {record.LinkedRecords.Count} building(s), reason unknown");
                }
            }

            callback?.Invoke(success, record);

            if (recursive)
            {
                NextFrame(() => ProcessNextSave());
            }
        });

        snapshot.BeginSave();
    }

    #endregion

    #region Recording

    /// <summary>
    /// Attempts to begin recording/tracking the specified tc.
    /// </summary>
    /// <param name="tc">The tc to record.</param>
    private void StartRecording(BuildingPrivlidge tc)
    {
        if (!ValidEntity(tc))
        {
            AddLogMessage($"Failed to record entity (invalid): {tc.net.ID}");
            return;
        }

        var recordId = tc.net.ID.Value;
        if (_buildRecords.TryGetValue(recordId, out var existing))
        {
            // Try to catch all edge cases when we have an existing tc
            // Realistically, we should never be here, but just in case

            if (tc == existing.BaseTC)
            {
                return;
            }
            else if (!ValidEntity(existing.BaseTC))
            {
                AddLogMessage($"Warning: Found existing entity with id '{recordId}' that appears invalid and will stop recording.");

            }
            else if (existing.NetworkID != recordId)
            {
                AddLogMessage($"Warning: Found existing entity with id '{existing.NetworkID}' in bucket '{recordId}', reassigning.");
                NextFrame(() => StartRecording(existing.BaseTC));
            }
            else
            {
                var persistantID = GetPersistanceID(tc);
                if (existing.PersistentID == persistantID)
                {
                    return;
                }
                else
                {
                    AddLogMessage($"Warning: Found existing entity with id '{existing.NetworkID}' in location '{existing.BaseTC.ServerPosition}' that will stop recording.");
                }
            }

            // Regardless of where we ended up, if existing isn't the current tc, we need to get replace it
            Pool.Free(ref existing);
        }

        // By this point, our bucket should be ready to assign (warnings printed or method returned)
        var record = Pool.Get<BuildRecord>()
            .Init(tc);

        AddLogMessage($"Begin recording entity '{recordId}' at coordinates '{record.BaseTC.ServerPosition}'");
        _buildRecords[recordId] = record;
    }

    private enum BuildState
    {
        /// <summary>
        /// The build record is being initialized and needs to be set up.
        /// </summary>
        NeedsInit,

        /// <summary>
        /// Idle, nothing needs to be done
        /// </summary>
        Idle,

        /// <summary>
        /// Changes were made. Build will be added to process queue when ready.
        /// </summary>
        Modified,

        /// <summary>
        /// In processing queue. Only should be here when the build is modified
        /// and the time since last save exceeds the delay between saves.
        /// </summary>
        Queued,

        /// <summary>
        /// When the build has been picked up by an agent for processing.
        /// </summary>
        Processing,

        /// <summary>
        /// Saving: Finding all TCs in radius and linking them together in snapshot.
        /// </summary>
        Save_BuildingNetwork,

        /// <summary>
        /// Saving: Finding all entities to save in linked network.
        /// </summary>
        Save_FindingEntities,

        /// <summary>
        /// Saving: Writing the output snapshot to the save location (by default, saves are on disk).
        /// </summary>
        Save_Writing,

        /// <summary>
        /// Save successfully completed.
        /// </summary>
        Save_Success,

        /// <summary>
        /// Save process failed. This needs to be logged and retries incremented.
        /// After so many retries, this will be removed from the backup queue.
        /// </summary>
        Save_Failure,

        /// <summary>
        /// The build has attempted to snapshot and failed more times than allowed.
        /// </summary>
        RetryLimitExceeded
    }

    /// <summary>
    /// Represents a record of a build, including the zones and changes made to it.
    /// </summary>
    private class BuildRecord : Pool.IPooled
    {
        private List<Vector4> _entityZones;
        private List<Vector4> _linkedZones;
        private bool _zonesDirty;
        private List<ChangeRecord> _changeLog;
        private Dictionary<DateTime, bool> _saveAttempts;
        private DateTime _lastUpdate;

        /// <summary>
        /// Represents a unique id for this world entity that can persist between server restarts.
        /// Built from <see cref="GetPersistanceID(BaseEntity)"/> and includes prefab and position data.
        /// </summary>
        public string PersistentID { get; private set; }

        /// <summary>
        /// The network id of the entity. This id is pooled/recycled and can change between restarts.
        /// Do not use for persistence.
        /// </summary>
        public ulong NetworkID => BaseTC.net.ID.Value;

        /// <summary>
        /// The base TC that this record is tracking.
        /// </summary>
        public BuildingPrivlidge BaseTC { get; private set; }

        /// <summary>
        /// The list of entity zones that this record is tracking. This is a cached list and must be built with <see cref="RefreshZoneCache"/> first.
        /// </summary>
        public List<Vector4> EntityZones
        {
            get
            {
                RefreshZoneCache();
                return _entityZones;
            }
        }

        /// <summary>
        /// The list of linked tc zones that this record is tracking. This is a cached list and must be built with <see cref="RefreshZoneCache"/> first.
        /// </summary>
        public List<Vector4> LinkedZones
        {
            get
            {
                RefreshZoneCache();
                return _linkedZones;
            }
        }

        /// <summary>
        /// The list of changes that have been made to this record since the last save.
        /// </summary>
        public List<ChangeRecord> ChangeLog => _changeLog;

        /// <summary>
        /// The current state of the build record.
        /// </summary>
        public BuildState State { get; private set; }

        /// <summary>
        /// The last time this record was modified.
        /// </summary>
        public DateTime LastModified => ChangeLog.Count > 0
            ? ChangeLog.Last().Time
            : DateTime.MinValue;

        /// <summary>
        /// The last time a save was attempted.
        /// </summary>
        public DateTime LastSaveAttempt => SaveAttempts.Count > 0
            ? SaveAttempts.Last().Key
            : DateTime.MinValue;

        /// <summary>
        /// The last time a save was successful.
        /// </summary>
        public DateTime LastSaveSuccess => SaveAttempts.Any(x => x.Value)
            ? SaveAttempts.Where(x => x.Value).Last().Key
            : DateTime.MinValue;

        /// <summary>
        /// The last time this record was updated.
        /// </summary>
        public DateTime LastUpdate => _lastUpdate;

        /// <summary>
        /// The list of save attempts and their success status.
        /// </summary>
        public Dictionary<DateTime, bool> SaveAttempts => _saveAttempts;

        /// <summary>
        /// The number of retries that have been attempted for this record.
        /// </summary>
        public int RetryCount => SaveAttempts.Values.Reverse().TakeWhile(x => !x).Count() - 1;

        /// <summary>
        /// The last exception that was thrown during processing.
        /// </summary>
        public Exception LastException { get; private set; }

        /// <summary>
        /// Initializes the build record with the specified base TC.
        /// </summary>
        /// <param name="baseTC">The base TC to initialize the record with.</param>
        /// <returns>The initialized build record.</returns>
        public BuildRecord Init(BuildingPrivlidge baseTC)
        {
            BaseTC = baseTC;
            PersistentID = GetPersistanceID(baseTC);
            State = BuildState.Idle;
            return this;
        }

        /// <summary>
        /// Updates the entity and linked TC zone areas for this record's base TC.
        /// </summary>
        public void RefreshZoneCache()
        {
            if (_zonesDirty)
            {
                BuildingScanner.GetZones(_entityZones, BaseTC, _config.Advanced.FoundationPrivilegeRadius, _config.Advanced.MaxScanZoneRadius);

                if (_config.MultiTC.Enabled)
                {
                    BuildingScanner.GetZones(_linkedZones, BaseTC, _config.MultiTC.ScanRadius, _config.Advanced.MaxScanZoneRadius);
                }
            }

            _zonesDirty = false;
        }

        /// <summary>
        /// Adds a change to the change log.
        /// </summary>
        /// <param name="entity">The entity that was changed.</param>
        /// <param name="action">The action that was performed.</param>
        /// <param name="player">The player that made the change.</param>
        public void AddChange(BaseEntity entity, ChangeAction action, BasePlayer player = null)
        {
            ChangeLog.Add(new()
            {
                Time = DateTime.UtcNow,
                Action = action,
                PlayerID = player?.UserIDString,
                PlayerDisplayName = player?.displayName,
            });

            if (State == BuildState.Save_Success || State == BuildState.Idle)
            {
                Update(BuildState.Modified);
            }
            else if (State != BuildState.RetryLimitExceeded)
            {
                Update(State);
            }

            if (entity is BuildingBlock)
            {
                _zonesDirty = true;
            }
        }

        /// <summary>
        /// Scans 2x building priv area for any other TCs, and if they have 
        /// any overlapping owners, they will be linked.
        /// Planning to make this configurable in the future.
        /// </summary>
        public IEnumerable<BuildRecord> FindLinkedRecords()
        {
            return Array.Empty<BuildRecord>();
        }

        /// <summary>
        /// Updates the state of the build record.
        /// </summary>
        /// <param name="state">The new state to set.</param>
        public void Update(BuildState state)
        {
            _lastUpdate = DateTime.Now;

            State = state;

            if (state == BuildState.Save_Success)
            {
                SaveAttempts.Add(_lastUpdate, true);
            }
            else if (state == BuildState.Save_Failure)
            {
                SaveAttempts.Add(_lastUpdate, false);
            }
        }

        /// <summary>
        /// Updates the state of the build record to indicate that a save attempt failed.
        /// </summary>
        /// <param name="ex">The exception that was thrown during the save attempt.</param>
        public void Update(Exception ex)
        {
            LastException = ex;

            Update(BuildState.Save_Failure);
        }

        /// <summary>
        /// Enters the pool and frees any unmanaged resources.
        /// </summary>
        public void EnterPool()
        {
            State = BuildState.NeedsInit;

            BaseTC = null;
            PersistentID = null;
            LastException = null;

            Pool.FreeUnmanaged(ref _entityZones);
            Pool.FreeUnmanaged(ref _linkedZones);
            Pool.FreeUnmanaged(ref _changeLog);
            Pool.FreeUnmanaged(ref _saveAttempts);
        }

        /// <summary>
        /// Leaves the pool and allocates any unmanaged resources.
        /// </summary>
        public void LeavePool()
        {
            _entityZones = Pool.Get<List<Vector4>>();
            _linkedZones = Pool.Get<List<Vector4>>();
            _changeLog = Pool.Get<List<ChangeRecord>>();
            _saveAttempts = Pool.Get<Dictionary<DateTime, bool>>();

            _lastUpdate = DateTime.Now;
            _zonesDirty = true;
        }

        public PersistantEntity GetPersistantEntity() => BaseTC;
    }

    /// <summary>
    /// Represents a change that was made to an entity.
    /// </summary>
    private readonly struct ChangeRecord
    {
        /// <summary>
        /// The time that the change was made.
        /// </summary>
        public DateTime Time { get; init; }

        /// <summary>
        /// The action that was performed on the entity.
        /// </summary>
        public ChangeAction Action { get; init; }

        /// <summary>
        /// The entity that was changed.
        /// </summary>
        public PersistantEntity Entity { get; init; }

        /// <summary>
        /// The id of the player that made the change.
        /// </summary>
        public string PlayerID { get; init; }

        /// <summary>
        /// The display name of the player that made the change.
        /// </summary>
        public string PlayerDisplayName { get; init; }
    }

    /// <summary>
    /// Represents the action that was performed on the entity.
    /// </summary>
    private enum ChangeAction
    {
        Create,
        Update,
        Decay
    }

    #endregion
}
