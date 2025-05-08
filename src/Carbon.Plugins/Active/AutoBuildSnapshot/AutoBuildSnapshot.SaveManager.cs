using Cysharp.Threading.Tasks;
using Facepunch;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Helper class for managing saving and loading of the base records.
    /// </summary>
    private static class SaveManager
    {
        /// <summary>
        /// The number of entities to process before yielding to avoid blocking the main thread.
        /// </summary>
        const int processEntitiesBeforeYield = 500;

        /// <summary>
        /// Saves the current state of the base recording.
        /// </summary>
        /// <param name="recording">The recording to save.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public static async UniTask SaveAsync(ChangeManagement.BaseRecording recording, BasePlayer player = null)
        {
            Helpers.Log(LangKeys.message_save_begin, player, recording.Id, recording.BaseTC.ServerPosition);

            using var entities = Pool.Get<PooledList<BaseEntity>>();
            using var zones = Pool.Get<PooledList<Vector3>>();
            await FindEntitiesForSaveAsync(recording, entities, zones, player);

            if (entities.Count == 0)
            {
                throw new LocalizedException(LangKeys.error_save_no_entities_found, player);
            }

            var meta = await SaveMetaDataAsync(recording, entities, zones, player);
            await SaveEntitiesAsync(meta, entities, zones, player);
        }

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
                zones.Add(block.ServerPosition);
            }

            int processed = 0;
            foreach (var zone in zones)
            {
                processed = await FindEntitiesForFoundation(entities, zone, Settings.Advanced.FoundationPrivilegeRadius, Masks.BaseEntities, processed);
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

            return meta;
        }

        /// <summary>
        /// Saves the entity data to disk.
        /// </summary>
        /// <param name="meta">The metadata for the save.</param>
        /// <param name="entities">The list of entities to save.</param>
        /// <param name="outputFile">The output file path.</param>
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

            using var buffer = Pool.Get<BufferStream>().Initialize();  // Ensure proper initialization
            copyPasteEntityInfo.ToProto(buffer);

            using var writer = File.Create(binFilePath);

            await WriteBlockZonesAsync(writer, zones);

            var segment = buffer.GetBuffer();
            await writer.WriteAsync(segment.Array, segment.Offset, segment.Count);
        }

        /// <summary>
        /// Writes the block data to the file stream.
        /// </summary>
        /// <param name="writer">The file stream to write to.</param>
        /// <param name="zones">The list of building blocks to write.</param>
        private static async UniTask WriteBlockZonesAsync(FileStream writer, List<Vector3> zones)
        {
            var blockBufferSize = sizeof(int) + sizeof(float) + (zones.Count * DataLength.Vector3); // payload size + radius + (zones * size)
            var blockBuffer = BufferStream.Shared.ArrayPool.Rent(blockBufferSize);
            try
            {
                int bufferOffset = 0;

                int payloadSize = blockBufferSize - sizeof(int);
                Helpers.WriteInt32(blockBuffer, payloadSize, ref bufferOffset);

                Helpers.WriteSingle(blockBuffer, Settings.Advanced.FoundationPrivilegeRadius, ref bufferOffset);

                for (int i = 0; i < zones.Count; i++)
                {
                    Helpers.WriteVector3(blockBuffer, zones[i], ref bufferOffset);
                }

                await writer.WriteAsync(blockBuffer, 0, blockBufferSize);
            }
            finally
            {
                BufferStream.Shared.ArrayPool.Return(blockBuffer);
            }
        }

        /// <summary>
        /// Reads the block zones from the file stream.
        /// </summary>
        /// <param name="reader">The file stream to read from.</param>
        /// <param name="zones">The list to store the read zones.</param>
        /// <returns>The radius of the zones.</returns>
        private static async UniTask<float> ReadBlockZoneData(FileStream reader, List<Vector3> zones)
        {
            var buffer = BufferStream.Shared.ArrayPool.Rent(sizeof(int));
            try
            {
                await reader.ReadAsync(buffer, 0, sizeof(int));

                int offset = 0;
                int payloadSize = Helpers.ReadInt32(buffer, ref offset);

                BufferStream.Shared.ArrayPool.Return(buffer);
                buffer = BufferStream.Shared.ArrayPool.Rent(payloadSize);

                await reader.ReadAsync(buffer, 0, payloadSize);

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
    }

    /// <summary>
    /// Represents metadata information for saves.
    /// </summary>
    private readonly struct MetaInfo
    {
        /// <summary>
        /// The ID of the save.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// The time stamp of the save.
        /// </summary>
        public DateTime TimeStamp { get; init; }

        /// <summary>
        /// The position of the origin point of the save.
        /// </summary>
        public Vector3 OriginPosition { get; init; }

        /// <summary>
        /// The rotation of the origin point of the save.
        /// </summary>
        public Quaternion OriginRotation { get; init; }

        /// <summary>
        /// The number of entities originally prepared for the save.
        /// </summary>
        /// <remarks>
        /// This count is not necessarily the same as the number of entities that made it in the save.
        /// Some entities will be dropped if they are invalid or their parent is not included in the save.
        /// </remarks>
        public int OriginalEntityCount { get; init; }

        /// <summary>
        /// The number of zones (priv radius spheres surrounding foundations) in the save.
        /// </summary>
        public int ZoneCount { get; init; }

        /// <summary>
        /// The name of the metadata file associated with the save.
        /// </summary>
        public string GetFilePath()
        {
            var directory = GetTimestampDirectory();
            var fileNamePrefix = GetFileNamePrefix();

            return Path.Combine(directory, $"{fileNamePrefix}.meta.json");
        }

        /// <summary>
        /// The name of the data file associated with the save.
        /// </summary>
        public string GetDataFilePath()
        {
            var directory = GetTimestampDirectory();
            var fileNamePrefix = GetFileNamePrefix();

            return Path.Combine(directory, $"{fileNamePrefix}.data.bin");
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
            return $"{TimeStamp:HH-mm-ss}-{Id}";
        }

        /// <summary>
        /// Creates a new instance of MetaInfo based on the given recording.
        /// </summary>
        /// <param name="recording">The recording to create the metadata from.</param>
        /// <returns>A new instance of MetaInfo.</returns>
        public static MetaInfo Create(ChangeManagement.BaseRecording recording, List<BaseEntity> entities, List<Vector3> zones)
        {
            return new MetaInfo
            {
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow,
                OriginPosition = recording.BaseTC.ServerPosition,
                OriginRotation = recording.BaseTC.ServerRotation,
                OriginalEntityCount = entities.Count,
                ZoneCount = zones.Count
            };
        }
    }
}