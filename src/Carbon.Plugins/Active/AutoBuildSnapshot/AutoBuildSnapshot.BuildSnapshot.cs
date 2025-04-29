using Cysharp.Threading.Tasks;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Represents all of the elements within set of zones.
    /// The zone set is comprised of the build area surrounding connected TCs.
    /// </summary>
    private class BuildSnapshot : ExecutionBase
    {
        private AutoBuildSnapshot _plugin;
        private Action<bool, BuildSnapshot> _resultCallback;

        private Dictionary<BuildRecord, List<BaseEntity>> _buildingEntities;
        private List<PlayerMetaData> _authorizedPlayers;
        private List<BuildRecord> _linkedRecords;

        /// <summary>
        /// The number of entities in the snapshot.
        /// </summary>
        public int EntityCount => _buildingEntities.Sum(kvp => kvp.Value.Count);

        /// <summary>
        /// The list of records/tcs processed in this snapshot.
        /// </summary>
        public IReadOnlyList<BuildRecord> LinkedRecords => _linkedRecords;

        /// <summary>
        /// Enters the pool and frees any unmanaged resources.
        /// </summary>
        public override void EnterPool()
        {
            base.EnterPool();

            FreeDictionaryList(ref _buildingEntities);
            Pool.FreeUnmanaged(ref _authorizedPlayers);
            Pool.FreeUnmanaged(ref _linkedRecords);
        }

        /// <summary>
        /// Leaves the pool and allocates any unmanaged resources.
        /// </summary>
        public override void LeavePool()
        {
            base.LeavePool();

            _buildingEntities = Pool.Get<Dictionary<BuildRecord, List<BaseEntity>>>();
            _authorizedPlayers = Pool.Get<List<PlayerMetaData>>();
            _linkedRecords = Pool.Get<List<BuildRecord>>();
        }

        public static BuildSnapshot Create(AutoBuildSnapshot plugin, BuildRecord record, Action<bool, BuildSnapshot> resultCallback)
        {
            var snapshot = Pool.Get<BuildSnapshot>();
            snapshot.Init(plugin, record, resultCallback);
            return snapshot;
        }

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

        /// <summary>
        /// Saves the current snapshot for the initialized record data, including all surrounding TCs/records.
        /// </summary>
        protected override async UniTask ProcessAsync()
        {
            try
            {
                // update _linkedRecords
                await BuildNetworkAsync();
                await YieldStep();

                // update _buildingEntities
                await LoadEntitiesAsync();
                await YieldStep();

                // serialize and save the snapshot
                await FinishSaveAsync();
                await YieldStep();

                // update records with success state
                Update(BuildState.Save_Success);

                // callback success
                _resultCallback(true, this);
            }
            catch (Exception ex)
            {
                // update records with failed state
                Update(ex);

                // callback failure
                _resultCallback(false, this);

                // re-throw upstream
                throw;
            }
        }

        /// <summary>
        /// Recursively builds the network of linked TCs.
        /// </summary>
        private async UniTask BuildNetworkAsync()
        {
            if (_config.MultiTC.Mode == MultiTCMode.Disabled)
                return;

            using var processed = Pool.Get<PooledList<BuildRecord>>();
            var processQueue = Pool.Get<Queue<BuildRecord>>();
            processQueue.Enqueue(_linkedRecords[0]);

            while (processQueue.Count > 0)
            {
                var record = processQueue.Dequeue();
                processed.Add(record);

                if (_linkedRecords.Contains(record))
                    continue;

                var collidingRecords = await _instance.GetCollidingRecordsAsync(record.LinkedZones);
                foreach(var collision in collidingRecords)
                {
                    if (processed.Contains(record))
                        continue;

                    if (IsLinkedRecord(collision))
                    {
                        AddLinkedRecord(collision);
                        processQueue.Enqueue(collision);
                    }

                    await YieldStep();
                }

                Pool.FreeUnmanaged(ref collidingRecords);
            }

            Pool.FreeUnmanaged(ref processQueue);
        }

        /// <summary>
        /// Finds all entities in the linked records.
        /// </summary>
        private async UniTask LoadEntitiesAsync()
        {
            using var baseTCs = Pool.Get<PooledList<BuildingPrivlidge>>();
            foreach (var record in _linkedRecords)
            {
                if (!_buildingEntities.TryGetValue(record, out var entityList))
                {
                    entityList = Pool.Get<List<BaseEntity>>();
                    _buildingEntities.Add(record, entityList);
                }

                entityList.Clear();
                foreach (var zone in record.EntityZones)
                {
                    using var entities = BuildingScanner.GetEntities<BaseEntity>(record.BaseTC, zone, _maskBaseEntities);
                    foreach (var entity in entities)
                    {
                        // skip if already processed
                        if (entityList.Contains(entity))
                            continue;

                        // only perform next checks if entity does not belong to authorized player
                        if (!_authorizedPlayers.Select(p => p.UserID).Contains(entity.OwnerID))
                        {
                            // check if entity is owned by one of the authorized players
                            if (!_config.General.IncludeNonAuthorizedDeployables)
                                continue;

                            // check if entity is a tracked type
                            if (!_config.General.IncludeGroundResources && entity is ResourceEntity)
                                continue;
                        }

                        // get building priv
                        var entityTC = entity.GetBuildingPrivilege();

                        // ignore if has no building priv
                        if (!entityTC)
                            continue;

                        // should be picked up by another record
                        if (entityTC != record.BaseTC && baseTCs.Contains(entityTC))
                            continue;

                        // add to the list
                        entityList.Add(entity);

                        await YieldStep();
                    }
                }
            }
        }

        /// <summary>
        /// Finishes the save process and writes the snapshot to disk (or other storage).
        /// </summary>
        private async UniTask FinishSaveAsync()
        {
            var now = DateTime.UtcNow;
            var snapshotId = Guid.NewGuid();

            var dataFile = $"{_snapshotDataDirectory}/{now:yyyyMMdd}/{now:hhmmss}_{snapshotId}.{_snapshotDataExtension}";
            var metaFile = $"{_snapshotDataDirectory}/{now:yyyyMMdd}/{now:hhmmss}_{snapshotId}.{_snapshotMetaExtension}";

            await WriteSnapshotMetaAsync(now, snapshotId, dataFile, metaFile);
            await WriteSnapshotDataAsync(now, snapshotId, dataFile, metaFile);
        }

        /// <summary>
        /// Writes the snapshot metadata to disk (or other storage).
        /// </summary>
        /// <param name="now">The current time.</param>
        /// <param name="snapshotId">The unique id of the snapshot.</param>
        /// <param name="dataFile">The file to write the data to.</param>
        /// <param name="metaFile">The file to write the metadata to.</param>
        private async UniTask WriteSnapshotMetaAsync(DateTime now, Guid snapshotId, string dataFile, string metaFile)
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
                Version = _plugin.Version,
                ID = snapshotId,
                DataFile = dataFile,
                TimestampUTC = now,
                Entities = _buildingEntities.Sum(be => be.Value.Count),
                LinkedBuildings = linkedBuildingsMeta,
                AuthorizedPlayers = _authorizedPlayers,
            };

            await UniTask.RunOnThreadPool(() =>
            {
                Interface.Oxide.DataFileSystem.WriteObject(metaFile, metaData);
            });

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
        private async UniTask WriteSnapshotDataAsync(DateTime now, Guid snapshotId, string dataFile, string metaFile)
        {
            var buildingEntities = Pool.Get<Dictionary<string, List<PersistantEntity>>>();
            var zones = Pool.Get<List<Vector4>>();
            try
            {
                foreach (var buildingId in _buildingEntities.Keys)
                {
                    var entities = Pool.Get<List<PersistantEntity>>();
                    entities.AddRange(_buildingEntities[buildingId]
                        .Select(PersistantEntity.CreateFrom)
                        .OrderBy(e => e.PrefabName)
                        .OrderBy(e => e.Type));
                    buildingEntities.Add(buildingId.PersistentID, entities);
                }

                zones.AddRange(_linkedRecords.SelectMany(record => record.EntityZones).Distinct());

                var snapshotData = new SnapshotData
                {
                    Version = _plugin.Version,
                    ID = snapshotId,
                    MetaDataFile = metaFile,
                    Timestamp = now,
                    Entities = buildingEntities,
                    Zones = zones,
                };

                await snapshotData.SaveAsync(Path.Combine(Interface.Oxide.DataDirectory, dataFile), _config.Advanced.DataSaveFormat);
            }
            finally
            {
                foreach(var kvp in buildingEntities)
                {
                    var list = buildingEntities[kvp.Key];
                    Pool.Free(ref list, true);
                }

                FreeDictionaryList(ref buildingEntities);
                Pool.FreeUnmanaged(ref zones);
            }
        }

        private void AddLinkedRecord(BuildRecord record)
        {
            // if it's the first record, we will always initialize it
            if (_linkedRecords.Count > 0)
            {
                if (_linkedRecords.Contains(record))
                    return;

                if (!IsLinkedRecord(record))
                    return;
            }

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
        /// Checks if the given record is linked to this snapshot.
        /// </summary>
        /// <param name="record">The record to check.</param>
        /// <returns>True if the record is linked, false otherwise.</returns>
        private bool IsLinkedRecord(BuildRecord record)
        {
            switch (_config.MultiTC.Mode)
            {
                case MultiTCMode.Manual:
                    if (_instance._manualLinks.TryGetValue(record.PersistentID, out var links))
                    {
                        return _linkedRecords
                            .Select(r => r.PersistentID)
                            .Any(links.Contains);
                    }
                    return false;

                case MultiTCMode.Automatic:
                    var authorized = _linkedRecords[0].BaseTC.authorizedPlayers;
                    if (record.BaseTC.authorizedPlayers.Any(authorized.Contains))
                    {
                        return _linkedRecords
                            .SelectMany(r => r.LinkedZones)
                            .Any(zone => ZoneContains(zone, record.BaseTC.ServerPosition));
                    }
                    return false;

            case MultiTCMode.Disabled:
            default:
                    return false;
            }
        }

        private void Update(Exception ex)
        {
            foreach (var record in _linkedRecords)
            {
                record.Update(ex);
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
    }

    /// <summary>
    /// The metadata that is created when a snapshot is saved.
    /// Primarily used to index current backups.
    /// </summary>
    private readonly struct BuildSnapshotMetaData
    {
        /// <summary>
        /// The version of the plugin when this snapshot was created.
        /// </summary>
        public required VersionNumber Version { get; init; }

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

        /// <summary>
        /// Gets the associated snapshot data from the file.
        /// </summary>
        /// <returns>The snapshot data.</returns>
        public UniTask<SnapshotData> GetDataAsync() => SnapshotData.LoadAsync(DataFile);
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
        /// The version of the plugin when this snapshot was created.
        /// </summary>
        public required VersionNumber Version { get; init; }

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

        /// <summary>
        /// Loads the snapshot data from the specified file.
        /// </summary>
        /// <param name="file">The file to load from.</param>
        public static async UniTask<SnapshotData> LoadAsync(string file)
        {
            await UniTask.SwitchToThreadPool();
            try
            {
                file = FindExistingFile(file);

                if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    var data = File.ReadAllText(file);
                    return JsonConvert.DeserializeObject<SnapshotData>(data);
                }

                bool compressed = file.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
                using var stream = File.Open(file, FileMode.Open);
                using var gzip = compressed ? new GZipStream(stream, CompressionMode.Decompress) : null;
                using var reader = new BinaryReader(compressed ? gzip : stream);

                return new SnapshotData
                {
                    Version = SerializationHelper.ReadVersionNumber(reader),
                    ID = SerializationHelper.ReadGuid(reader),
                    MetaDataFile = reader.ReadString(),
                    Timestamp = SerializationHelper.ReadDateTime(reader),
                    Zones = SerializationHelper.ReadList<Vector4>(reader),
                    Entities = SerializationHelper.ReadDictionary<string, List<PersistantEntity>>(reader)
                };
            }
            finally
            {
                await UniTask.SwitchToMainThread();
            }
        }

        /// <summary>
        /// Saves the snapshot data to the specified file.
        /// </summary>
        /// <param name="file">The file to save to.</param>
        /// <param name="compress">Whether to compress the data.</param>
        public async UniTask SaveAsync(string file, DataFormat saveFormat = DataFormat.Binary)
        {
            var format = _config.Advanced.DataSaveFormat;
            var version = _instance.Version;

            await UniTask.SwitchToThreadPool();
            try
            {
                if (format == DataFormat.Json || format == DataFormat.JsonExpanded)
                {
                    var jsonFormat = format == DataFormat.JsonExpanded
                        ? Formatting.Indented
                        : Formatting.None;

                    var data = JsonConvert.SerializeObject(this, jsonFormat);
                    File.WriteAllText(file + ".json", data);
                }
                else if (format == DataFormat.Binary || format == DataFormat.GZip)
                {
                    file += ".bin";

                    bool compress = format == DataFormat.GZip;
                    if (compress)
                    {
                        file += ".gz";
                    }

                    using var stream = File.Open(file, FileMode.Create);
                    using var gzip = compress ? new GZipStream(stream, CompressionMode.Compress) : null;
                    using var writer = new BinaryWriter(compress ? gzip : stream);

                    SerializationHelper.Write(writer, version);
                    SerializationHelper.Write(writer, ID);
                    writer.Write(MetaDataFile);
                    SerializationHelper.Write(writer, Timestamp);
                    SerializationHelper.Write(writer, Zones);
                    SerializationHelper.Write(writer, Entities);
                }
                else
                {
                    throw new NotSupportedException($"Unsupported data format: {format}");
                }
            }
            finally
            {
                await UniTask.SwitchToMainThread();
            }
        }

        private static readonly string[] dataEextensions = { "", ".bin", ".bin.gz", ".json" };

        /// <summary>
        /// Finds an existing file, checking variants and data directory.
        /// </summary>
        /// <param name="filePath">The initial file path to check.</param>
        /// <returns>The path to the existing file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when no file is found.</exception>
        private static string FindExistingFile(string filePath)
        {
            // Try original directory
            foreach (string ext in dataEextensions)
            {
                string path = filePath + ext;
                if (File.Exists(path))
                    return path;
            }

            // Try data directory
            string dataFilePath = Path.Combine(Interface.Oxide.DataDirectory, filePath);
            foreach (string ext in dataEextensions)
            {
                string path = dataFilePath + ext;
                if (File.Exists(path))
                    return path;
            }

            throw new FileNotFoundException($"Snapshot data file not found: {filePath}");
        }
    }
}
