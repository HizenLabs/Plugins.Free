using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Represents a persistent entity, which just uses the prefabID and coords.
    /// </summary>
    private partial class PersistantEntity : Pool.IPooled
    {
        public PersistantEntity() { }

        /// <summary>
        /// Creates a new persistent entity from the specified entity.
        /// </summary>
        /// <param name="entity">The entity to create the persistent entity from.</param>
        public static PersistantEntity Load(BaseEntity entity)
        {
            var result = Pool.Get<PersistantEntity>();
            if (!ValidEntity(entity))
            {
                return result;
            }

            result.Version = _instance.Version;
            result.Type = entity.GetType().Name;
            result.PrefabName = entity.PrefabName;
            result.PrefabID = entity.prefabID;
            result.OwnerID = entity.OwnerID;
            result.Position = entity.ServerPosition;
            result.Rotation = entity.ServerRotation;
            result.CenterPosition = result.Position + entity.bounds.center;

            var size = entity.bounds.size;
            result.CollisionRadius = Mathf.Max(size.x, size.y, size.z) + 1;

            result.LoadProperties(entity);

            return result;
        }

        /// <summary>
        /// Generates a unique ID for the entity based on its prefab and position.
        /// </summary>
        [JsonIgnore]
        public string ID => IsNull ? "null" : GetPersistanceID(Type, Position);

        /// <summary>
        /// Checks if the entity is null (i.e. has a prefab ID of 0).
        /// </summary>
        [JsonIgnore]
        public bool IsNull => PrefabID == 0;

        /// <summary>
        /// The plugin version when this entity was created.
        /// </summary>
        public VersionNumber Version { get; private set; }

        /// <summary>
        /// The type of this entity.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// The prefab name of the entity.
        /// </summary>
        public string PrefabName { get; private set; }

        /// <summary>
        /// The prefab ID of the entity.
        /// </summary>
        public uint PrefabID { get; private set; }

        /// <summary>
        /// The ID of the entity's owner.
        /// </summary>
        public ulong OwnerID { get; private set; }

        /// <summary>
        /// The position of the entity.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// The rotation of the entity.
        /// </summary>
        public Quaternion Rotation { get; private set; }

        /// <summary>
        /// The center position of the entity bounds.
        /// </summary>
        public Vector3 CenterPosition { get; private set; }

        /// <summary>
        /// The radius from the center to scan for collisions.
        /// </summary>
        public float CollisionRadius { get; private set; }

        /// <summary>
        /// The object properties.
        /// </summary>
        public Dictionary<string, object> Properties => _properties;
        private Dictionary<string, object> _properties;

        public void EnterPool()
        {
            Version = default;
            Type = null;
            PrefabName = null;
            PrefabID = 0;
            OwnerID = 0;
            Position = default;
            Rotation = default;
            CenterPosition = default;
            CollisionRadius = 0;

            Pool.FreeUnmanaged(ref _properties);
        }

        public void LeavePool()
        {
            _properties = Pool.Get<Dictionary<string, object>>();
        }

        public static PersistantEntity Load(string dataFile)
        {
            using var fs = File.Open(dataFile, FileMode.Open, FileAccess.Read, FileShare.None);
            using var reader = new BinaryReader(fs);
            return ReadFrom(reader);
        }

        public static PersistantEntity ReadFrom(BinaryReader reader)
        {
            var entity = Pool.Get<PersistantEntity>();
            entity.Version = SerializationHelper.ReadVersionNumber(reader);
            entity.Type = reader.ReadString();
            entity.PrefabName = reader.ReadString();
            entity.PrefabID = reader.ReadUInt32();
            entity.OwnerID = reader.ReadUInt64();
            entity.Position = SerializationHelper.ReadVector3(reader);
            entity.Rotation = SerializationHelper.ReadQuaternion(reader);
            entity.CenterPosition = SerializationHelper.ReadVector3(reader);
            entity.CollisionRadius = reader.ReadSingle();
            entity._properties = SerializationHelper.ReadDictionary<string, object>(reader);
            return entity;
        }

        public void Save(string dataFile)
        {
            using var fs = File.Open(dataFile, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new BinaryWriter(fs);

            Write(writer);
        }

        public void Write(BinaryWriter writer)
        {
            SerializationHelper.Write(writer, Version);
            writer.Write(Type);
            writer.Write(PrefabName);
            writer.Write(PrefabID);
            writer.Write(OwnerID);
            SerializationHelper.Write(writer, Position);
            SerializationHelper.Write(writer, Rotation);
            SerializationHelper.Write(writer, CenterPosition);
            writer.Write(CollisionRadius);
            SerializationHelper.Write(writer, Properties);
        }
    }

}
