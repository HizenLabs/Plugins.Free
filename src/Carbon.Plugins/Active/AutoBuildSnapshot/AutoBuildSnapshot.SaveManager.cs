using Cysharp.Threading.Tasks;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private static string BackupDirectory => Path.Combine(Interface.Oxide.DataDirectory, "abs_backups");
    private const string gzipExtension = ".gz";
    private const string metaExtension = "meta.json";
    private const string dataExtension = "data.bin";

    #region Save Manager

    /// <summary>
    /// Helper class for managing saving and loading of the base records.
    /// </summary>
    private static class SaveManager
    {
        #region Fields

        /// <summary>
        /// The number of entities to process before yielding to avoid blocking the main thread.
        /// </summary>
        private const int processEntitiesBeforeYield = 500;

        private static Dictionary<ChangeManagement.RecordingId, List<MetaInfo>> _recordingSaves;

        #endregion

        #region Lifecycle

        public static void Init()
        {
            _recordingSaves = Pool.Get<Dictionary<ChangeManagement.RecordingId, List<MetaInfo>>>();

            LoadSavesAsync().Forget();
        }

        private static async UniTaskVoid LoadSavesAsync()
        {
            var retentionHours = Settings.General.SnapshotRetentionPeriodHours;
            var retentionDateUtc = DateTime.UtcNow.AddHours(-retentionHours);

            if (!Directory.Exists(BackupDirectory))
            {
                Directory.CreateDirectory(BackupDirectory);

                // No need to continue if no backups exist
                return;
            }

            using var files = Pool.Get<PooledList<string>>();
            await UniTask.RunOnThreadPool(() =>
            {
                foreach (var dir in Directory.EnumerateDirectories(BackupDirectory))
                {
                    foreach (var file in Directory.EnumerateFiles(dir, "*.*"))
                    {
                        files.Add(file);
                    }
                }
            });

            foreach (var file in files)
            {
                try
                {
                    if (File.GetCreationTimeUtc(file) < retentionDateUtc)
                    {
                        try
                        {
                            Helpers.Log(LangKeys.save_retention_deletion, null, Path.GetFileName(file), retentionHours);

                            await UniTask.RunOnThreadPool(() =>
                            {
                                File.Delete(file);
                            });
                        }
                        catch
                        {
                            Helpers.Log(LangKeys.save_retention_deletion_error, null, Path.GetFileName(file));
                        }
                    }
                    else if (file.EndsWith(metaExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        MetaInfo meta = default;
                        await UniTask.RunOnThreadPool(() =>
                        {
                            var json = File.ReadAllText(file);
                            meta = JsonConvert.DeserializeObject<MetaInfo>(json);
                        });

                        IndexMetaInfo(meta);
                    }
                }
                catch { }
            }
        }

        public static void Unload()
        {
            Pool.FreeUnmanaged(ref _recordingSaves);
        }

        #endregion

        #region Indexing and Retrieval

        private static void IndexMetaInfo(MetaInfo meta)
        {
            if (!_recordingSaves.TryGetValue(meta.RecordId, out var saves))
            {
                Helpers.Log(LangKeys.save_loading, null, meta.RecordId.Position);

                saves = Pool.Get<List<MetaInfo>>();
                _recordingSaves[meta.RecordId] = saves;
            }

            if (!saves.Contains(meta))
            {
                saves.Add(meta);
            }
        }

        public static void GetRecordingsByDistanceTo(Vector3 location, List<ChangeManagement.RecordingId> results)
        {
            results.Clear();

            results.AddRange(_recordingSaves.Keys);

            results.Sort((a, b) =>
            {
                float aDistSq = (a.Position - location).sqrMagnitude;
                float bDistSq = (b.Position - location).sqrMagnitude;
                return aDistSq.CompareTo(bDistSq);
            });
        }

        public static void GetSaves(ChangeManagement.RecordingId recordingId, List<MetaInfo> results)
        {
            results.Clear();

            if (_recordingSaves.TryGetValue(recordingId, out var saves))
            {
                results.AddRange(saves);
            }

            results.Sort((a, b) =>
            {
                return b.TimeStamp.CompareTo(a.TimeStamp);
            });
        }

        public static MetaInfo GetLastSave(ChangeManagement.RecordingId recordingId)
        {
            if (!_recordingSaves.TryGetValue(recordingId, out var saves))
            {
                return default;
            }

            MetaInfo lastSave = default;
            foreach (var save in saves)
            {
                if (lastSave.TimeStamp < save.TimeStamp)
                {
                    lastSave = save;
                }
            }

            return lastSave;
        }
        
        #endregion

        #region Cleanup

        public static async UniTask CleanupAsync()
        {
            using var metaList = Pool.Get<PooledList<MetaInfo>>();
            foreach (var list in _recordingSaves.Values)
            {
                metaList.AddRange(list);
            }

            var retentionDate = DateTime.UtcNow.AddHours(-Settings.General.SnapshotRetentionPeriodHours);
            using var toRemove = Pool.Get<PooledList<MetaInfo>>();
            await UniTask.RunOnThreadPool(() =>
            {
                foreach (var meta in metaList)
                {
                    var metaFilePath = meta.GetFilePath();
                    var dataFilePath = meta.GetDataFilePath();

                    if (!File.Exists(metaFilePath))
                    {
                        Helpers.Log(LangKeys.save_cleanup_missing, null, meta.RecordId, metaFilePath);
                        toRemove.Add(meta);
                        continue;
                    }

                    if (!File.Exists(dataFilePath))
                    {
                        Helpers.Log(LangKeys.save_cleanup_missing, null, meta.RecordId, dataFilePath);
                        toRemove.Add(meta);
                        continue;
                    }

                    if (meta.TimeStamp < retentionDate)
                    {
                        Helpers.Log(LangKeys.save_cleanup_old, null, meta.RecordId, meta.TimeStamp);
                        toRemove.Add(meta);
                        continue;
                    }
                }
            });

            foreach (var meta in toRemove)
            {
                try
                {
                    var metaFilePath = meta.GetFilePath();
                    if (File.Exists(metaFilePath))
                    {
                        File.Delete(metaFilePath);
                    }

                    var dataFilePath = meta.GetDataFilePath();
                    if (File.Exists(dataFilePath))
                    {
                        File.Delete(dataFilePath);
                    }

                    if (_recordingSaves.TryGetValue(meta.RecordId, out var saves))
                    {
                        saves.Remove(meta);
                        if (saves.Count == 0)
                        {
                            _recordingSaves.Remove(meta.RecordId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helpers.Log(LangKeys.save_cleanup_error, null, meta.RecordId, ex.Message);
                }
            }
        }

        #endregion

        #region Saving

        /// <summary>
        /// Saves the current state of the base recording.
        /// </summary>
        /// <param name="recording">The recording to save.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public static async UniTask<int> SaveAsync(ChangeManagement.BaseRecording recording, BasePlayer player = null)
        {
            if (!recording.IsActive)
            {
                throw new LocalizedException(LangKeys.error_save_recording_inactive, player, recording.Id);
            }

            using var entities = Pool.Get<PooledList<BaseEntity>>();
            using var zones = Pool.Get<PooledList<Vector3>>();
            await FindEntitiesForSaveAsync(recording, entities, zones, player);

            if (entities.Count == 0)
            {
                throw new LocalizedException(LangKeys.error_save_no_entities_found, player);
            }

            var meta = await SaveMetaDataAsync(recording, entities, zones, player);
            await SaveEntitiesAsync(meta, entities, zones, player);

            return entities.Count;
        }

        /// <summary>
        /// Saves the metadata for the given recording and entities to disk.
        /// </summary>
        /// <param name="recording">The recording to save metadata for.</param>
        /// <param name="entities">The list of entities to save metadata for.</param>
        /// <param name="player">The player who initiated the save.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        private static async UniTask<MetaInfo> SaveMetaDataAsync(
            ChangeManagement.BaseRecording recording,
            List<BaseEntity> entities,
            List<Vector3> zones,
            BasePlayer player = null)
        {
            var meta = MetaInfo.Create(recording, entities, zones);
            var metaFilePath = meta.GetFilePath();

            EnsureValidSaveFile(metaFilePath, player);

            await UniTask.RunOnThreadPool(() =>
            {
                var serialized = JsonConvert.SerializeObject(meta, Formatting.Indented);
                File.WriteAllText(metaFilePath, serialized);
            });

            IndexMetaInfo(meta);

            return meta;
        }

        /// <summary>
        /// Saves the entity data to disk.
        /// </summary>
        /// <param name="meta">The metadata for the save.</param>
        /// <param name="entities">The list of entities to save.</param>
        /// <param name="zones">The list of zones to save.</param>
        /// <param name="player">The player who initiated the save (optional).</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        private static async UniTask SaveEntitiesAsync(
            MetaInfo meta,
            List<BaseEntity> entities,
            List<Vector3> zones,
            BasePlayer player = null)
        {
            var binFilePath = meta.GetDataFilePath();

            EnsureValidSaveFile(binFilePath, player);

            ConVar.CopyPaste.OrderEntitiesForSave(entities);
            using var copyPasteEntityInfo = Pool.Get<CopyPasteEntityInfo>();

            var alignObject = CreateAlignmentObject(meta);
            await SaveCopyPasteEntitiesAsync(copyPasteEntityInfo, entities, alignObject);
            UnityEngine.Object.Destroy(alignObject.gameObject);

            using Stream writer = GetDataFileStream(binFilePath, isReading: false);

            await WriteBlockZonesAsync(writer, zones);

            await WriteCopyPasteEntityInfoAsync(writer, copyPasteEntityInfo);
        }

        /// <summary>
        /// Ensures that the save file is valid and does not already exist. Also creates the directory if it doesn't exist.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <param name="player">The player who initiated the save (optional).</param>
        /// <exception cref="LocalizedException">Thrown if the file already exists.</exception>
        private static void EnsureValidSaveFile(string filePath, BasePlayer player = null)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else if (File.Exists(filePath))
            {
                throw new LocalizedException(LangKeys.error_save_file_exists, player, filePath);
            }
        }

        #endregion

        #region Rollback

        public static async UniTaskVoid BeginRollbackAttemptAsync(BasePlayer player, MetaInfo meta)
        {
            Helpers.Log(LangKeys.rollback_attempt_begin, player, meta.RecordId.Position, meta.TimeStamp);

            var sb = Pool.Get<StringBuilder>();
            try
            {
                // Check if recording exists and if so, create lock
                var privs = Pool.Get<List<BuildingPrivlidge>>();

                // Check for the data file
                var dataFilePath = meta.GetDataFilePath();
                if (!File.Exists(dataFilePath))
                {
                    throw new LocalizedException(LangKeys.error_rollback_data_file_missing, player, dataFilePath);
                }

                // Load the zones in the data file
                var zones = Pool.Get<List<Vector3>>();
                var stream = GetDataFileStream(dataFilePath, isReading: true);
                var radius = await ReadBlockZoneDataAsync(stream, zones);

                // Check each zone for any existing TCs and create backup for each linked recordid to the TCs found
                var entities = Pool.Get<List<BaseEntity>>();
                await FindEntitiesForSaveAsync(zones, entities, radius, player);

                // extract all the TCs from the entities found
                bool showConfirmation = false;
                foreach (var entity in entities)
                {
                    if (entity is BuildingPrivlidge tc && !privs.Contains(tc))
                    {
                        privs.Add(tc);

                        if (tc.AutoBuildSnapshot_BaseRecording is ChangeManagement.BaseRecording recording
                            && recording.Id != meta.RecordId)
                        {
                            if (showConfirmation)
                            {
                                sb.Append(", ");
                            }

                            sb.Append(recording.Id.Position.ToString());

                            // If the TC is not the current recording, we need to show confirmation
                            showConfirmation = true;
                        }
                    }
                }

                // if any TCs were found that != current record, show confirmation first
                if (showConfirmation)
                {
                    UserInterface.ShowConfirmation(
                        player: player,
                        langKey: LangKeys.rollback_confirm_overwrite,
                        arg1: sb.ToString(),
                        onConfirm: p =>
                        {
                            ProcessRollbackAsync(p, meta, stream, zones, entities, privs).Forget();
                        },
                        onCancel: p =>
                        {
                            try
                            {
                                stream?.Dispose();
                            }
                            finally
                            {
                                Pool.FreeUnmanaged(ref zones);
                                Pool.FreeUnmanaged(ref entities);
                                Pool.FreeUnmanaged(ref privs);
                            }
                        });
                }
                else
                {
                    // Run on separate thread so it follows the same flow as the confirmation
                    ProcessRollbackAsync(player, meta, stream, zones, entities, privs).Forget();
                }
            }
            catch (Exception ex)
            {
                // Log rollback failure
                Helpers.Log(LangKeys.error_rollback_attempt_fail, player, ex.Message);
            }
            finally
            {
                Pool.FreeUnmanaged(ref sb);
            }
        }

        private static async UniTask ProcessRollbackAsync(
            BasePlayer player,
            MetaInfo meta,
            Stream stream,
            List<Vector3> zones,
            List<BaseEntity> entities,
            List<BuildingPrivlidge> privs)
        {
            using var locks = Pool.Get<PooledList<ChangeManagement.RecordingLock>>();
            var stopwatch = Pool.Get<Stopwatch>();
            try
            {
                stopwatch.Start();

                // Find any existing recordings for the given TCs
                using var recordings = Pool.Get<PooledList<ChangeManagement.BaseRecording>>();
                foreach (var priv in privs)
                {
                    if (priv.AutoBuildSnapshot_BaseRecording is ChangeManagement.BaseRecording tcRecord)
                    {
                        recordings.Add(tcRecord);
                    }
                }

                // Create backups for each recording
                foreach (var recording in recordings)
                {
                    if (recording.IsActive)
                    {
                        await recording.AttemptSaveAsync(player);
                    }
                }

                // Create locks for each recording
                foreach (var recording in recordings)
                {
                    var recordLock = recording.CreateLock(player);
                    locks.Add(recordLock);
                }

                // Load the copypaste data from the file
                var copyPasteEntityInfo = await ReadCopyPasteEntityInfoAsync(stream);

                // Kill all entities
                int processed = 0;
                foreach (var entity in entities)
                {
                    // The mask should already exclude this, but just in case
                    if (entity is BasePlayer)
                    {
                        continue;
                    }

                    // Skip if already destroyed
                    if (!entity || entity.IsDestroyed)
                    {
                        continue;
                    }

                    entity.Kill();

                    if (++processed % processEntitiesBeforeYield == 0)
                    {
                        await UniTask.Yield();
                    }
                }

                // Paste the copypaste entities into the world
                var request = new PasteRequest
                {
                    resources = Settings.General.IncludeGroundResources,
                    origin = meta.OriginPosition,
                    playerRotation = meta.OriginRotation.eulerAngles,
                    deployables = true,
                    vehicles = true
                };
                ConVar.CopyPaste.PasteEntitiesInternal(copyPasteEntityInfo, new(request));

                // Get the building priv after the paste
                var updatedPriv = FindBuildingPrivlidge(meta.OriginPosition);
                if (!updatedPriv)
                {
                    throw new LocalizedException(LangKeys.error_rollback_priv_invalid, player);
                }

                // Start recording the priv
                ChangeManagement.StartRecording(updatedPriv);

                // Log rollback success
                Helpers.Log(LangKeys.rollback_attempt_success, player, Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2));
            }
            catch (Exception ex)
            {
                // Log rollback failure
                Helpers.Log(LangKeys.error_rollback_attempt_fail, player, ex.ToString());
            }
            finally
            {
                try
                {
                    stream?.Dispose();

                    foreach (var recordLock in locks)
                    {
                        recordLock?.Dispose();
                    }
                }
                finally
                {
                    Pool.FreeUnmanaged(ref stopwatch);
                    Pool.FreeUnmanaged(ref zones);
                    Pool.FreeUnmanaged(ref entities);
                    Pool.FreeUnmanaged(ref privs);
                }
            }
        }

        #endregion

        #region Find Entities

        /// <summary>
        /// Finds entities to save for the given recording.
        /// </summary>
        /// <param name="recording">The recording to find entities for.</param>
        /// <param name="entities">The list to store the found entities.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async UniTask FindEntitiesForSaveAsync(
            ChangeManagement.BaseRecording recording,
            List<BaseEntity> entities,
            List<Vector3> zones,
            BasePlayer player = null)
        {
            foreach (var block in recording.Building.buildingBlocks)
            {
                zones.Add(block.ServerPosition + block.bounds.center);
            }

            await FindEntitiesForSaveAsync(zones, entities, Settings.Advanced.FoundationZoneRadius, player);
        }

        /// <summary>
        /// Finds entities to save for the given recording.
        /// </summary>
        /// <param name="zones">The list of zones to search for entities.</param>
        /// <param name="entities">The list to store the found entities.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async UniTask FindEntitiesForSaveAsync(
            List<Vector3> zones,
            List<BaseEntity> entities,
            float radius,
            BasePlayer player = null)
        {
            int processed = 0;
            foreach (var zone in zones)
            {
                processed = await FindEntitiesForFoundation(entities, zone, radius, Masks.BaseEntities, processed);
            }
        }

        /// <summary>
        /// Finds entities for the given foundation block within a specified radius and layer mask.
        /// </summary>
        /// <param name="entities">The list to store the found entities.</param>
        /// <param name="block">The foundation block to check.</param>
        /// <param name="radius">The radius to search within.</param>
        /// <param name="layerMask">The layer mask to use for the search.</param>
        private static async UniTask<int> FindEntitiesForFoundation(
            List<BaseEntity> entities,
            Vector3 zone,
            float radius,
            int layerMask,
            int processed)
        {
            var colliders = Physics.OverlapSphere(zone, radius, layerMask);
            foreach (var collider in colliders)
            {
                var entity = collider.GetComponentInParent<BaseEntity>();
                if (entity != null && !entities.Contains(entity))
                {
                    entities.Add(entity);
                }

                if (++processed % processEntitiesBeforeYield == 0)
                {
                    await UniTask.Yield();
                }
            }

            return processed;
        }

        /// <summary>
        /// Finds the BuildingPrivlidge associated with the given position.
        /// </summary>
        /// <param name="position">The position to search for the BuildingPrivlidge.</param>
        /// <returns>The found BuildingPrivlidge, or null if none was found.</returns>
        private static BuildingPrivlidge FindBuildingPrivlidge(Vector3 position)
        {
            var colliders = Physics.OverlapSphere(position, 3f, Masks.BaseEntities);
            foreach (var collider in colliders)
            {
                var entity = collider.GetComponentInParent<BaseEntity>();
                if (entity is BuildingPrivlidge priv)
                {
                    return priv;
                }
            }

            return null;
        }

        #endregion

        #region Helpers

        #region File Operations

        /// <summary>
        /// Gets a file stream for reading or writing based on the specified mode.
        /// </summary>
        /// <param name="filePath">The file path to open.</param>
        /// <param name="isReading">True if the stream is for reading, false if it is for writing.</param>
        /// <returns>The opened file stream.</returns>
        private static Stream GetDataFileStream(string filePath, bool isReading)
        {
            return isReading
                ? OpenRead(filePath)
                : OpenWrite(filePath);
        }

        /// <summary>
        /// Opens a file stream for reading.
        /// </summary>
        /// <param name="filePath">The file path to open.</param>
        /// <returns>The opened file stream.</returns>
        private static Stream OpenRead(string filePath)
        {
            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return FormatStream(filePath, stream, isReading: true);
        }

        /// <summary>
        /// Opens a file stream for writing.
        /// </summary>
        /// <param name="filePath">The file path to open.</param>
        /// <returns>The opened file stream.</returns>
        private static Stream OpenWrite(string filePath)
        {
            var stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

            return FormatStream(filePath, stream, isReading: false);
        }

        /// <summary>
        /// Formats the stream based on the specified data save format.
        /// </summary>
        /// <param name="filePath">The name of the file being processed.</param>
        /// <param name="stream">The stream to format.</param>
        /// <param name="isReading">True if the stream is being read, false if it is being written.</param>
        /// <returns>The formatted stream.</returns>
        private static Stream FormatStream(string filePath, Stream stream, bool isReading)
        {
            if (filePath.EndsWith(gzipExtension, StringComparison.OrdinalIgnoreCase))
            {
                return CompressStream(stream, isReading);
            }

            return stream;
        }

        /// <summary>
        /// Compresses or decompresses the given stream based on the specified mode.
        /// </summary>
        /// <param name="stream">The stream to compress or decompress.</param>
        /// <param name="isReading">True if the stream is being read, false if it is being written.</param>
        /// <returns>The compressed or decompressed stream.</returns>
        private static Stream CompressStream(Stream stream, bool isReading)
        {
            var mode = isReading
                ? CompressionMode.Decompress
                : CompressionMode.Compress;

            return new GZipStream(stream, mode);
        }

        #endregion

        #region Block Zones Serialization

        /// <summary>
        /// Writes the block data to the file stream.
        /// </summary>
        /// <param name="stream">The file stream to write to.</param>
        /// <param name="zones">The list of building blocks to write.</param>
        private static async UniTask WriteBlockZonesAsync(Stream stream, List<Vector3> zones)
        {
            var blockBufferSize = sizeof(int) + sizeof(float) + (zones.Count * DataLength.Vector3); // payload size + radius + (zones * size)
            var blockBuffer = BufferStream.Shared.ArrayPool.Rent(blockBufferSize);
            try
            {
                int bufferOffset = 0;

                int payloadSize = blockBufferSize - sizeof(int);
                Helpers.WriteInt32(blockBuffer, payloadSize, ref bufferOffset);

                Helpers.WriteSingle(blockBuffer, Settings.Advanced.FoundationZoneRadius, ref bufferOffset);

                for (int i = 0; i < zones.Count; i++)
                {
                    Helpers.WriteVector3(blockBuffer, zones[i], ref bufferOffset);
                }

                await stream.WriteAsync(blockBuffer, 0, blockBufferSize);
            }
            finally
            {
                BufferStream.Shared.ArrayPool.Return(blockBuffer);
            }
        }

        /// <summary>
        /// Reads the block zones from the file stream.
        /// </summary>
        /// <param name="stream">The file stream to read from.</param>
        /// <param name="zones">The list to store the read zones.</param>
        /// <returns>The radius of the zones.</returns>
        private static async UniTask<float> ReadBlockZoneDataAsync(Stream stream, List<Vector3> zones)
        {
            var buffer = BufferStream.Shared.ArrayPool.Rent(sizeof(int));
            try
            {
                await stream.ReadAsync(buffer, 0, sizeof(int));

                int offset = 0;
                int payloadSize = Helpers.ReadInt32(buffer, ref offset);

                BufferStream.Shared.ArrayPool.Return(buffer);
                buffer = BufferStream.Shared.ArrayPool.Rent(payloadSize);

                await stream.ReadAsync(buffer, 0, payloadSize);

                offset = 0;
                var radius = Helpers.ReadSingle(buffer, ref offset);

                var count = (payloadSize - offset) / DataLength.Vector3;
                for (int i = 0; i < count; i++)
                {
                    var zone = Helpers.ReadVector3(buffer, ref offset);
                    zones.Add(zone);
                }

                return radius;
            }
            finally
            {
                BufferStream.Shared.ArrayPool.Return(buffer);
            }
        }

        #endregion

        #region CopyPasteEntityInfo Serialization

        /// <summary>
        /// Writes the CopyPasteEntityInfo to the given stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        /// <param name="copyPasteEntityInfo">The CopyPasteEntityInfo to write.</param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        private static async UniTask WriteCopyPasteEntityInfoAsync(
            Stream writer,
            CopyPasteEntityInfo copyPasteEntityInfo)
        {
            using var buffer = Pool.Get<BufferStream>().Initialize();
            copyPasteEntityInfo.ToProto(buffer);

            var segment = buffer.GetBuffer();

            var sizeBytes = BufferStream.Shared.ArrayPool.Rent(sizeof(int));
            try
            {
                int offset = 0;
                Helpers.WriteInt32(sizeBytes, segment.Count - segment.Offset, ref offset);
                await writer.WriteAsync(sizeBytes, 0, sizeof(int));
            }
            finally
            {
                BufferStream.Shared.ArrayPool.Return(sizeBytes);
            }

            await writer.WriteAsync(segment.Array, segment.Offset, segment.Count);
        }

        /// <summary>
        /// Reads the CopyPasteEntityInfo from the given stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>A task representing the asynchronous read operation, returning the CopyPasteEntityInfo.</returns>
        private static async UniTask<CopyPasteEntityInfo> ReadCopyPasteEntityInfoAsync(Stream stream)
        {
            var buffer = BufferStream.Shared.ArrayPool.Rent(sizeof(int));
            try
            {
                await stream.ReadAsync(buffer, 0, sizeof(int));

                int offset = 0;
                int payloadSize = Helpers.ReadInt32(buffer, ref offset);

                BufferStream.Shared.ArrayPool.Return(buffer);
                buffer = BufferStream.Shared.ArrayPool.Rent(payloadSize);

                await stream.ReadAsync(buffer, 0, payloadSize);

                using var bufferStream = Pool.Get<BufferStream>().Initialize(buffer, payloadSize);
                var copyPasteEntityInfo = Pool.Get<CopyPasteEntityInfo>();
                copyPasteEntityInfo.FromProto(bufferStream);

                return copyPasteEntityInfo;
            }
            finally
            {
                BufferStream.Shared.ArrayPool.Return(buffer);
            }
        }

        #endregion

        #region CopyPaste Entity Saving

        /// <summary>
        /// Saves the copy-paste entities to the given CopyPasteEntityInfo.
        /// </summary>
        /// <param name="copyPasteEntityInfo">The CopyPasteEntityInfo to save to.</param>
        /// <param name="entities">The list of entities to save.</param>
        /// <param name="alignObject">The alignment object for the entities.</param>
        private static async UniTask SaveCopyPasteEntitiesAsync(
            CopyPasteEntityInfo copyPasteEntityInfo,
            List<BaseEntity> entities,
            Transform alignObject)
        {
            copyPasteEntityInfo.entities = Pool.Get<List<Entity>>();

            foreach (BaseEntity entity in entities)
            {
                if (!entity.isClient && entity.enableSaving)
                {
                    BaseEntity baseEntity = entity.parentEntity.Get(serverside: true);

                    // Skip entities that are parented to entities not included in the copy
                    if (baseEntity != null && (!entities.Contains(baseEntity) || !baseEntity.enableSaving))
                        continue;

                    ConVar.CopyPaste.SaveEntity(entity, copyPasteEntityInfo, baseEntity, alignObject);
                }

                if (copyPasteEntityInfo.entities.Count % processEntitiesBeforeYield == 0)
                {
                    await UniTask.Yield();
                }
            }
        }

        /// <summary>
        /// Creates an alignment object for the given metadata.
        /// </summary>
        /// <param name="meta">The metadata to create the alignment object for.</param>
        /// <returns>The created alignment object.</returns>
        private static Transform CreateAlignmentObject(MetaInfo meta)
        {
            var transform = new GameObject("Align").transform;
            transform.position = meta.OriginPosition;
            transform.rotation = meta.OriginRotation;
            return transform;
        }

        #endregion

        #endregion
    }

    #endregion

    #region Meta Info

    /// <summary>
    /// Represents metadata information for saves.
    /// </summary>
    private readonly record struct MetaInfo(
        [JsonProperty] Guid Id,
        [JsonProperty] ChangeManagement.RecordingId RecordId,
        [JsonProperty] DateTime TimeStamp,
        [JsonProperty] Vector3 OriginPosition,
        [JsonProperty] Quaternion OriginRotation,
        [JsonProperty] int OriginalEntityCount,
        [JsonProperty] int ZoneCount,
        [JsonProperty] int AuthorizedCount,
        [JsonProperty] bool IsCompressed
    )
    {
        /// <summary>
        /// The name of the metadata file associated with the save.
        /// </summary>
        public string GetFilePath()
        {
            var directory = GetTimestampDirectory();
            var fileNamePrefix = GetFileNamePrefix();

            return Path.Combine(directory, $"{fileNamePrefix}.{metaExtension}");
        }

        /// <summary>
        /// The name of the data file associated with the save.
        /// </summary>
        public string GetDataFilePath()
        {
            var directory = GetTimestampDirectory();
            var fileNamePrefix = GetFileNamePrefix();

            return Path.Combine(directory, $"{fileNamePrefix}.{dataExtension}{(IsCompressed ? ".gz" : string.Empty)}");
        }

        /// <summary>
        /// The timestamp directory for the save.
        /// </summary>
        /// <returns>The directory path for the timestamp.</returns>
        private string GetTimestampDirectory()
        {
            return Path.Combine(BackupDirectory, TimeStamp.ToString("yyyyMMdd"));
        }

        /// <summary>
        /// Gets the file name prefix for the save.
        /// </summary>
        /// <param name="id">The prefix of the save file.</param>
        private string GetFileNamePrefix()
        {
            return $"{TimeStamp:HH-mm-ss}_{RecordId.ToFileSafeId()}";
        }

        /// <summary>
        /// Creates a new instance of MetaInfo based on the given recording.
        /// </summary>
        /// <param name="recording">The recording to create the metadata from.</param>
        /// <returns>A new instance of MetaInfo.</returns>
        public static MetaInfo Create(ChangeManagement.BaseRecording recording, List<BaseEntity> entities, List<Vector3> zones)
        {
            return new MetaInfo
            (
                Id: Guid.NewGuid(),
                RecordId: recording.Id,
                TimeStamp: DateTime.UtcNow,
                OriginPosition: recording.BaseTC.ServerPosition,
                OriginRotation: recording.BaseTC.ServerRotation,
                OriginalEntityCount: entities.Count,
                ZoneCount: zones.Count,
                AuthorizedCount: recording.BaseTC.authorizedPlayers.Count,
                IsCompressed: Settings.Advanced.DataSaveFormat == DataFormat.GZip
            );
        }
    }

    #endregion
}