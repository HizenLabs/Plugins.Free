using Facepunch;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Represents all of the elements within set of zones.
    /// The zone set is comprised of the build area surrounding connected TCs.
    /// </summary>
    private class BuildSnapshot : Pool.IPooled
    {
        private AutoBuildSnapshot _plugin;
        private Action<bool, BuildSnapshot> _resultCallback;

        private Dictionary<BuildRecord, List<BaseEntity>> _buildingEntities;
        private List<PlayerMetaData> _authorizedPlayers;
        private List<BuildRecord> _linkedRecords;
        private Queue<BuildRecord> _stepRecordQueue;
        private BuildRecord _stepRecord;
        private Queue<BaseEntity> _stepEntityQueue;

        private Stopwatch _processWatch;
        private Stopwatch _stepWatch;

        /// <summary>
        /// The number of frames it took to process the snapshot.
        /// </summary>
        public int FrameCount { get; private set; }

        /// <summary>
        /// The maximum time that a step took during processing.
        /// </summary>
        public double LongestStepDuration { get; private set; }

        /// <summary>
        /// The exception, if any, that was thrown during processing.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// The list of records/tcs processed in this snapshot.
        /// </summary>
        public IReadOnlyList<BuildRecord> LinkedRecords => _linkedRecords;

        /// <summary>
        /// The time it took to process the snapshot, or the current running duration if not yet completed.
        /// </summary>
        public double Duration => _processWatch.Elapsed.TotalMilliseconds;

        /// <summary>
        /// The time it took to process the last step (frame), or the current step if not yet completed.
        /// </summary>
        public double StepDuration => _stepWatch.Elapsed.TotalMilliseconds;

        /// <summary>
        /// Controls whether the next step will be processed in a new frame.
        /// Duration is represented in fractional milliseconds.
        /// </summary>
        private bool NeedsFrame => StepDuration > _config.Advanced.MaxStepFrameDuration;

        /// <summary>
        /// Initializes a new build snapshot starting from the specified TC.
        /// This will gather all TC in radius as well.
        /// </summary>
        /// <param name="buildingPrivlidge"></param>
        public void Init(AutoBuildSnapshot plugin, BuildRecord record, Action<bool, BuildSnapshot> resultCallback)
        {
            _plugin = plugin;
            _resultCallback = resultCallback;

            AddLinkedRecord(record);
        }

        private void AddLinkedRecord(BuildRecord record)
        {
            if (_linkedRecords.Contains(record))
                return;

            _linkedRecords.Add(record);

            foreach (var user in record.BaseTC.authorizedPlayers)
            {
                if (_authorizedPlayers.Any(p => p.UserID == user.userid))
                    continue;

                _authorizedPlayers.Add(new PlayerMetaData
                {
                    UserID = user.userid,
                    UserName = user.username
                });
            }
        }

        /// <summary>
        /// Saves the current snapshot for the initialized record data, including all surrounding TCs/records.
        /// </summary>
        /// <param name="plugin">The base plugin reference.</param>
        /// <param name="resultCallback">The callback action when the save is complete, as this can span several frames.</param>
        public void BeginSave()
        {
            Action nextStep;
            BuildState nextState;

            if (_config.MultiTC.Enabled)
            {
                nextStep = BuildNetwork;
                nextState = BuildState.Save_BuildingNetwork;
            }
            else
            {
                nextStep = FindEntities;
                nextState = BuildState.Save_FindingEntities;
            }

            TryStep(() =>
            {
                if (_linkedRecords.Count == 0)
                {
                    throw new InvalidOperationException($"Snapshot can't be saved before initializing!");
                }

                if (++FrameCount > 1)
                {
                    throw new InvalidOperationException($"Cannot call save twice on snapshot!");
                }

                if (!ValidEntity(_linkedRecords[0].BaseTC))
                {
                    throw new InvalidOperationException("Base TC is invalid! Unable to perform snapshot.");
                }

                Update(nextState, _stepRecordQueue.Enqueue);
            },
            nextStep);
        }

        /// <summary>
        /// Recursively builds the network of linked TCs.
        /// </summary>
        private void BuildNetwork()
        {
            if (_stepRecordQueue.Count > 0)
            {
                TryStep(() =>
                {
                    var next = _stepRecordQueue.Dequeue();

                    foreach (var zone in next.LinkedZones)
                    {
                        var links = BuildingScanner.GetEntities<BuildingPrivlidge>(next.BaseTC, zone, _maskBaseEntities);
                        foreach (var link in links)
                        {
                            var recordId = link.net.ID.Value;

                            // if we're not tracking this tc, skip it
                            if (!_buildRecords.TryGetValue(recordId, out var record))
                                continue;

                            // if this record is already linked, skip it
                            if (_linkedRecords.Contains(record))
                                continue;

                            // if this record is already in the queue, skip it
                            if (_stepRecordQueue.Contains(record))
                                continue;

                            AddLinkedRecord(record);

                            _stepRecordQueue.Enqueue(record);
                        }
                    }
                },
                BuildNetwork);
            }
            else
            {
                // Network build complete, update states and begin finding entities
                TryStep(() =>
                {
                    Update(BuildState.Save_FindingEntities, _stepRecordQueue.Enqueue);
                },
                FindEntities);
            }
        }

        /// <summary>
        /// Finds all entities in the linked records.
        /// </summary>
        private void FindEntities()
        {
            if (_stepRecordQueue.Count > 0)
            {
                TryStep(() =>
                {
                    _stepRecord = _stepRecordQueue.Dequeue();
                    _buildingEntities.Add(_stepRecord, Pool.Get<List<BaseEntity>>());
                    foreach (var zone in _stepRecord.EntityZones)
                    {
                        var entities = BuildingScanner.GetEntities<BaseEntity>(_stepRecord.BaseTC, zone, _maskBaseEntities);
                        foreach (var entity in entities)
                        {
                            if (!_config.General.IncludeGroundResources && entity.OwnerID == 0 && entity is CollectibleEntity)
                                continue;

                            if (!_config.General.IncludeNonAuthorizedDeployables && !AuthorizedEntity(entity))
                                continue;

                            _stepEntityQueue.Enqueue(entity);
                        }
                    }
                },
                ProcessFoundEntities);
            }
            else
            {
                // Placeholder until final save step is implemented.
                TryStep(() =>
                {
                    Update(BuildState.Save_Writing);
                },
                FinishSave);
            }
        }

        private bool AuthorizedEntity(BaseEntity entity) =>
            _authorizedPlayers.Any(p => p.UserID == entity.OwnerID);

        /// <summary>
        /// Processes the found entities and adds them to the snapshot.
        /// </summary>
        private void ProcessFoundEntities()
        {
            if (_stepEntityQueue.Count > 0)
            {
                TryStep(() =>
                {
                    var next = _stepEntityQueue.Dequeue();
                    if (_buildingEntities[_stepRecord].Any(e => e.net.ID == next.net.ID))
                        return;

                    _buildingEntities[_stepRecord].Add(next);
                },
                ProcessFoundEntities);
            }
            else
            {
                FindEntities();
            }
        }

        /// <summary>
        /// Finishes the save process and writes the snapshot to disk (or other storage).
        /// </summary>
        private void FinishSave()
        {
            TryStep(() =>
            {
                var now = DateTime.UtcNow;
                var snapshotId = Guid.NewGuid();
                var snapshotPrefix = $"{now:hhmmss}_{snapshotId}";
                var dataFile = $"{_snapshotDataDirectory}/{snapshotPrefix}.{_snapshotDataExtension}";
                var metaFile = $"{_snapshotDataDirectory}/{snapshotPrefix}.{_snapshotMetaExtension}";

                WriteSnapshotMeta(now, snapshotId, dataFile, metaFile);
                WriteSnapshotData(now, snapshotId, dataFile, metaFile);

                Update(BuildState.Save_Success);
            },
            () => _resultCallback(true, this));
        }

        /// <summary>
        /// Writes the snapshot metadata to disk (or other storage).
        /// </summary>
        /// <param name="now">The current time.</param>
        /// <param name="snapshotId">The unique id of the snapshot.</param>
        /// <param name="dataFile">The file to write the data to.</param>
        /// <param name="metaFile">The file to write the metadata to.</param>
        private void WriteSnapshotMeta(DateTime now, Guid snapshotId, string dataFile, string metaFile)
        {
            var linkedBuildingsMeta = Pool.Get<Dictionary<string, BuildingMetaData>>();
            foreach (var kvp in _buildingEntities)
            {
                linkedBuildingsMeta.Add(kvp.Key.PersistentID, new BuildingMetaData
                {
                    Position = kvp.Key.BaseTC.ServerPosition,
                    Entities = kvp.Value.Count,
                    Zones = kvp.Key.EntityZones,
                });
            }

            var metaData = new BuildSnapshotMetaData
            {
                ID = snapshotId,
                DataFile = dataFile,
                TimestampUTC = now,
                Entities = _buildingEntities.Sum(be => be.Value.Count),
                LinkedBuildings = linkedBuildingsMeta,
                AuthorizedPlayers = _authorizedPlayers,
            };

            Interface.Oxide.DataFileSystem.WriteObject(metaFile, metaData);

            // add metadata to the resource collection and update indexes
            _plugin.SyncSnapshotMetaData(metaData);

            // note: do not free linkedBuildingsMeta here -- it's still being used in the snapshots menu
        }

        /// <summary>
        /// Writes the snapshot data to disk (or other storage).
        /// </summary>
        /// <param name="now">The current time.</param>
        /// <param name="snapshotId">The unique id of the snapshot.</param>
        /// <param name="dataFile">The file to write the data to.</param>
        /// <param name="metaFile">The file to write the metadata to.</param>
        private void WriteSnapshotData(DateTime now, Guid snapshotId, string dataFile, string metaFile)
        {
            var buildingEntities = Pool.Get<Dictionary<string, List<PersistantEntity>>>();
            var zones = Pool.Get<List<Vector4>>();
            try
            {
                foreach (var buildingId in _buildingEntities.Keys)
                {
                    var entities = Pool.Get<List<PersistantEntity>>();
                    entities.AddRange(_buildingEntities[buildingId]
                        .Select(e => (PersistantEntity)e)
                        .OrderBy(e => e.PrefabName)
                        .OrderBy(e => e.Type));
                    buildingEntities.Add(buildingId.PersistentID, entities);
                }

                zones.AddRange(_linkedRecords.SelectMany(record => record.EntityZones).Distinct());

                var snapshotData = new SnapshotData
                {
                    ID = snapshotId,
                    MetaDataFile = metaFile,
                    Timestamp = now,
                    Entities = buildingEntities,
                    Zones = zones,
                };

                Interface.Oxide.DataFileSystem.WriteObject(dataFile, snapshotData);
            }
            finally
            {
                FreeDictionaryList(ref buildingEntities);
                Pool.FreeUnmanaged(ref zones);
            }
        }

        /// <summary>
        /// Attempts to perform a step in the process.
        /// Decides to run the step immediately or on the next frame depending on <see cref="NeedsFrame"/>.
        /// This is calculated based off of a preset duration between frames.
        /// </summary>
        /// <param name="stepAction">The action to perform.</param>
        /// <param name="successCallback">The callback to invoke on success.</param>
        private void TryStep(
            Action stepAction,
            Action successCallback
        )
        {
            // Persist all entities
            try
            {
                _processWatch.Start();
                _stepWatch.Restart();

                stepAction();

                _stepWatch.Stop();
                _processWatch.Stop();

                // Track the longest step duration
                if (StepDuration > LongestStepDuration)
                {
                    LongestStepDuration = StepDuration;
                }

                if (NeedsFrame)
                {
                    FrameCount++;
                    _plugin.NextFrame(successCallback);
                }
                else
                {
                    successCallback();
                }
            }
            catch (Exception ex)
            {
                foreach (var record in _linkedRecords)
                {
                    record.Update(ex);
                }

                Exception = ex;

                // on failure, call the primary result handler immediately
                _resultCallback(false, this);
            }
        }

        private void Update(BuildState state, Action<BuildRecord> updateAction = null)
        {
            foreach (var record in _linkedRecords)
            {
                record.Update(state);

                updateAction?.Invoke(record);
            }
        }

        /// <summary>
        /// Enters the pool and frees any unmanaged resources.
        /// </summary>
        public void EnterPool()
        {
            foreach (var recordId in _buildingEntities.Keys)
            {
                var recordEntities = _buildingEntities[recordId];
                if (recordEntities != null)
                {
                    Pool.FreeUnmanaged(ref recordEntities);
                    _buildingEntities[recordId] = null;
                }
            }

            Pool.FreeUnmanaged(ref _buildingEntities);
            Pool.FreeUnmanaged(ref _authorizedPlayers);
            Pool.FreeUnmanaged(ref _linkedRecords);
            Pool.FreeUnmanaged(ref _stepRecordQueue);
            Pool.FreeUnmanaged(ref _stepEntityQueue);

            _processWatch.Reset();
            _stepWatch.Reset();

            FrameCount = 0;
            LongestStepDuration = 0;
            Exception = null;
        }

        /// <summary>
        /// Leaves the pool and allocates any unmanaged resources.
        /// </summary>
        public void LeavePool()
        {
            _buildingEntities = Pool.Get<Dictionary<BuildRecord, List<BaseEntity>>>();
            _authorizedPlayers = Pool.Get<List<PlayerMetaData>>();
            _linkedRecords = Pool.Get<List<BuildRecord>>();
            _stepRecordQueue = Pool.Get<Queue<BuildRecord>>();
            _stepEntityQueue = Pool.Get<Queue<BaseEntity>>();

            _processWatch ??= new();
            _stepWatch ??= new();
        }
    }

    /// <summary>
    /// The metadata that is created when a snapshot is saved.
    /// Primarily used to index current backups.
    /// </summary>
    private readonly struct BuildSnapshotMetaData
    {
        /// <summary>
        /// The unique id of the snapshot.
        /// </summary>
        public required Guid ID { get; init; }

        /// <summary>
        /// The namne of the matching data file.
        /// Should be the same as this file, but with a ".data" extension.
        /// </summary>
        public required string DataFile { get; init; }

        /// <summary>
        /// The time the snapshot was created.
        /// </summary>
        public required DateTime TimestampUTC { get; init; }

        /// <summary>
        /// The number of entities in the snapshot.
        /// </summary>
        public required int Entities { get; init; }

        /// <summary>
        /// The linked buildings in the snapshot.
        /// </summary>
        public required Dictionary<string, BuildingMetaData> LinkedBuildings { get; init; }

        /// <summary>
        /// The list of players that are authorized in any of the linked buildings in the snapshot.
        /// </summary>
        public required List<PlayerMetaData> AuthorizedPlayers { get; init; }

        /// <summary>
        /// The collection of zones within this snapshot.
        /// </summary>
        public IEnumerable<Vector4> Zones => LinkedBuildings.Values
            .SelectMany(b => b.Zones)
            .Distinct();

        public SnapshotData GetData() => Interface.Oxide.DataFileSystem.ReadObject<SnapshotData>(DataFile);
    }

    /// <summary>
    /// The metadata for a building in the snapshot.
    /// </summary>
    private readonly struct BuildingMetaData
    {
        /// <summary>
        /// The position of the building TC.
        /// </summary>
        public Vector3 Position { get; init; }

        /// <summary>
        /// The number of entities attached to this building.
        /// </summary>
        public int Entities { get; init; }

        /// <summary>
        /// The number of zones this building is comprised of.
        /// </summary>
        public List<Vector4> Zones { get; init; }
    }

    /// <summary>
    /// The simplified player metadata.
    /// </summary>
    private readonly struct PlayerMetaData
    {
        /// <summary>
        /// The id of the player.
        /// </summary>
        public ulong UserID { get; init; }

        /// <summary>
        /// The display name of the player.
        /// </summary>
        public string UserName { get; init; }
    }

    private readonly struct SnapshotData
    {
        /// <summary>
        /// The unique id of the snapshot.
        /// </summary>
        public required Guid ID { get; init; }

        /// <summary>
        /// The namne of the matching metadata file.
        /// Should be the same as this file, but with a ".meta" extension.
        /// </summary>
        public required string MetaDataFile { get; init; }

        /// <summary>
        /// The time the snapshot was created.
        /// </summary>
        public required DateTime Timestamp { get; init; }

        /// <summary>
        /// The entities in this base, grouped by building.
        /// </summary>
        public required Dictionary<string, List<PersistantEntity>> Entities { get; init; }

        /// <summary>
        /// The zones this base is comprised of.
        /// </summary>
        public required List<Vector4> Zones { get; init; }
    }
}
