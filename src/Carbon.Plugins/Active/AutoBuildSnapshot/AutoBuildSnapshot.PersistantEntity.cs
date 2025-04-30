using Facepunch;
using Newtonsoft.Json;
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
    internal class PersistantEntity : PersistantBase<BaseEntity>
    {
        private static readonly IPropertyMapping[] _mappings = new IPropertyMapping[]
        {
            // BaseEntity
            new PropertyMapping<BaseEntity, BaseEntity.Flags>(e => e.flags),
            new PropertyMapping<BaseEntity, uint>(e => e.parentBone, e => e.HasParent()),
            new PropertyMapping<BaseEntity, ulong>(e => e.skinID),
            // new PropertyMapping<BaseEntity, List<EntityComponentBase>>(e => e.Components),
            new PropertyMapping<BaseEntity, ulong>(e => e.OwnerID),
            new PropertyMapping<BaseEntity, BaseEntity.TraitFlag>(e => e.Traits),

            // BuildingBlock
            new PropertyMapping<BuildingBlock, BuildingGrade.Enum>(e => e.grade),

            // BuildingPrivlidge
            new PropertyMapping<BuildingPrivlidge, PooledList<PlayerMetaData>>(
                $"{nameof(BuildingPrivlidge)}.{nameof(BuildingPrivlidge.authorizedPlayers)}",
                e =>
                {
                    var list = Pool.Get<PooledList<PlayerMetaData>>();
                    list.AddRange(e.authorizedPlayers.Select(PlayerMetaData.CreateFrom));
                    return list;
                },
                (e, v) =>
                {
                    foreach(var data in v)
                    {
                        e.authorizedPlayers.Add(data.ToPlayerNameID());
                    }
                },
                e => e.authorizedPlayers.Count > 0
            ),

            // DecayEntity
            new PropertyMapping<DecayEntity, float>(e => e.health),
        };

        public PersistantEntity() : base(_mappings) { }

        /// <summary>
        /// Generates a unique ID for the entity based on its prefab and position.
        /// </summary>
        [JsonIgnore]
        public string ID => GetPersistanceID(Type, Position);

        /// <summary>
        /// The type of this entity.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// The prefab name of the entity.
        /// </summary>
        public string PrefabName { get; private set; }

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

        public override void EnterPool()
        {
            base.EnterPool();

            Type = null;
            PrefabName = null;
            Position = default;
            Rotation = default;
            CenterPosition = default;
            CollisionRadius = 0;
        }

        public override void LeavePool()
        {
            base.LeavePool();
        }

        /// <summary>
        /// Creates a new persistent entity from the specified entity.
        /// </summary>
        /// <param name="entity">The entity to create the persistent entity from.</param>
        public static PersistantEntity CreateFrom(BaseEntity entity)
        {
            var result = Pool.Get<PersistantEntity>();
            result.Read(entity);
            return result;
        }

        public static PersistantEntity Load(string dataFile)
        {
            using var fs = File.Open(dataFile, FileMode.Open, FileAccess.Read, FileShare.None);
            using var reader = new BinaryReader(fs);

            var result = Pool.Get<PersistantEntity>();
            result.Read(reader);
            return result;
        }

        public void Save(string dataFile)
        {
            using var fs = File.Open(dataFile, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new BinaryWriter(fs);

            Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);

            Type = reader.ReadString();
            PrefabName = reader.ReadString();
            Position = SerializationHelper.ReadVector3(reader);
            Rotation = SerializationHelper.ReadQuaternion(reader);
            CenterPosition = SerializationHelper.ReadVector3(reader);
            CollisionRadius = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);

            writer.Write(Type);
            writer.Write(PrefabName);
            SerializationHelper.Write(writer, Position);
            SerializationHelper.Write(writer, Rotation);
            SerializationHelper.Write(writer, CenterPosition);
            writer.Write(CollisionRadius);
        }

        public override void Read(BaseEntity obj)
        {
            if (!ValidEntity(obj))
            {
                throw new InvalidOperationException("Failed to read entity: Invalid entity.");
            }

            base.Read(obj);

            Type = obj.GetType().Name;
            PrefabName = obj.PrefabName;
            Position = obj.ServerPosition;
            Rotation = obj.ServerRotation;
            CenterPosition = Position + obj.bounds.center;

            var size = obj.bounds.size;
            CollisionRadius = Mathf.Max(size.x, size.y, size.z) + 1;

            if (obj is StorageContainer container && container.inventory.itemList.Count > 0)
            {
                var subItems = Pool.Get<List<PersistantItem>>();
                subItems.AddRange(container.inventory.itemList.Select(PersistantItem.CreateFrom));
                Properties[nameof(SubItems)] = subItems;
            }
        }
    }
}