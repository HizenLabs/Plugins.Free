using Facepunch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Represents a persistent entity, which just uses the prefabID and coords.
    /// </summary>
    private readonly struct PersistantEntity
    {
        /// <summary>
        /// A null entity that can be used to represent an invalid or non-existent entity.
        /// </summary>
        private static PersistantEntity Null { get; } = new()
        {
            PrefabID = 0,
            Position = Vector3.zero
        };

        /// <summary>
        /// Creates a new persistent entity from the specified entity.
        /// </summary>
        /// <param name="entity">The entity to create the persistent entity from.</param>
        public PersistantEntity(BaseEntity entity)
        {
            Type = entity.GetType().Name;
            PrefabName = entity.PrefabName;
            PrefabID = entity.prefabID;
            OwnerID = entity.OwnerID;
            Position = entity.ServerPosition;
            Rotation = entity.ServerRotation;

            Properties = new();

            if (entity is BuildingBlock block)
            {
                Properties["Grade"] = block.grade;
            }

            if (entity is StorageContainer storage)
            {
                var itemContainers = Pool.Get<List<ItemContainer>>();

                storage.GetAllInventories(itemContainers);
                Properties["Items"] = itemContainers
                    .SelectMany(container => container.itemList
                    .Select(item => new PersistantItem(item)))
                    .ToArray();

                Pool.Free(ref itemContainers);
            }

            if (entity is DecayEntity decay)
            {
                Properties["Health"] = decay.health;
                Properties["HealthFraction"] = decay.healthFraction;
            }

            if (entity.HasAnySlot())
            {

            }

            if (entity.skinID > 0)
            {
                Properties["SkinID"] = entity.skinID;
            }
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
        /// The type of this entity.
        /// </summary>
        public string Type { get; init; }

        /// <summary>
        /// The prefab name of the entity.
        /// </summary>
        public string PrefabName { get; init; }

        /// <summary>
        /// The prefab ID of the entity.
        /// </summary>
        public uint PrefabID { get; init; }

        /// <summary>
        /// The ID of the entity's owner.
        /// </summary>
        public ulong OwnerID { get; init; }

        /// <summary>
        /// The position of the entity.
        /// </summary>
        public Vector3 Position { get; init; }

        /// <summary>
        /// The rotation of the entity.
        /// </summary>
        public Quaternion Rotation { get; init; }

        /// <summary>
        /// The object properties.
        /// </summary>
        public Dictionary<string, object> Properties { get; init; }

        /// <summary>
        /// Implicitly converts a <see cref="BaseEntity"/> to a <see cref="PersistantEntity"/>.
        /// </summary>
        /// <param name="entity"></param>
        public static implicit operator PersistantEntity(BaseEntity entity)
        {
            if (!ValidEntity(entity))
            {
                return Null;
            }

            return new(entity);
        }
    }

    /// <summary>
    /// Represents a persistent item, which is a simplified version of the Base Item.
    /// </summary>
    private readonly struct PersistantItem
    {
        public PersistantItem(Item item)
        {
            UID = item.uid.Value;
            ItemID = item.info.itemid;
            Tag = item.info.tag;
            Amount = item.amount;
            Flags = item.flags;

            Properties = new();

            if (item.contents != null)
            {
                Properties["Items"] = item.contents.itemList?
                    .Select(i => new PersistantItem(i))
                    .ToArray()
                    ?? Array.Empty<PersistantItem>();
            }

            if (item.fuel > 0)
            {
                Properties["Fuel"] = item.fuel;
            }

            if (item.skin > 0)
            {
                Properties["SkinID"] = item.skin;
            }
        }

        public ulong UID { get; init; }

        public int ItemID { get; init; }

        public string Tag { get; init; }

        public int Amount { get; init; }

        public Item.Flag Flags { get; init; }

        public Dictionary<string, object> Properties { get; init; }
    }
}
