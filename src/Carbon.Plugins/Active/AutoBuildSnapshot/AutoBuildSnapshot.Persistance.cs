using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using System;
using System.Collections;
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

        public static PersistantEntity Load(string dataFile)
        {
            using var fs = File.Open(dataFile, FileMode.Open, FileAccess.Read, FileShare.None);
            using var reader = new BinaryReader(fs);

            return Load(reader);
        }

        public static PersistantEntity Load(BinaryReader reader)
        {
            var entity = new PersistantEntity
            {
                Type = reader.ReadString(),
                PrefabName = reader.ReadString(),
                PrefabID = reader.ReadUInt32(),
                OwnerID = reader.ReadUInt64(),
                Position = SerializationHelper.ReadVector3(reader),
                Rotation = SerializationHelper.ReadQuaternion(reader),
                Properties = SerializationHelper.ReadDictionary(reader)
            };

            return entity;
        }

        public void Save(string dataFile)
        {
            using var fs = File.Open(dataFile, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new BinaryWriter(fs);

            Save(writer);
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Type);
            writer.Write(PrefabName);
            writer.Write(PrefabID);
            writer.Write(OwnerID);
            SerializationHelper.Write(writer, Position);
            SerializationHelper.Write(writer, Rotation);
            SerializationHelper.Write(writer, Properties);
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

        public static PersistantItem Load(string dataFile)
        {
            using var fs = File.Open(dataFile, FileMode.Open, FileAccess.Read, FileShare.None);
            using var reader = new BinaryReader(fs);

            return Load(reader);
        }

        public static PersistantItem Load(BinaryReader reader)
        {
            var item = new PersistantItem
            {
                UID = reader.ReadUInt64(),
                ItemID = reader.ReadInt32(),
                Tag = reader.ReadString(),
                Amount = reader.ReadInt32(),
                Flags = (Item.Flag)reader.ReadInt32(),
                Properties = SerializationHelper.ReadDictionary(reader)
            };

            return item;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(UID);
            writer.Write(ItemID);
            writer.Write(Tag);
            writer.Write(Amount);
            writer.Write((int)Flags);

            SerializationHelper.Write(writer, Properties);
        }
    }

    private static class SerializationHelper
    {
        #region Type Constants

        private enum TypeMarker : byte
        {
            // Primitive types (0-19)
            Null = 0,
            Bool = 1,
            Byte = 2,
            Char = 3,
            Short = 4,
            Int = 5,
            Long = 6,
            Float = 7,
            Double = 8,
            String = 9,

            // Common types (20-49)
            Object = 20,
            Array = 21,
            Dictionary = 22,

            // Custom types (50+)
            Entity = 50,
            Item = 51,
        }

        #endregion

        #region Write Methods for Basic Types

        public static void Write(BinaryWriter writer, VersionNumber version)
        {
            writer.Write(version.Major);
            writer.Write(version.Minor);
            writer.Write(version.Patch);
        }

        public static void Write(BinaryWriter writer, Vector3 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
        }

        public static void Write(BinaryWriter writer, Vector4 vector)
        {
            Write(writer, (Vector3)vector);
            writer.Write(vector.w);
        }

        public static void Write(BinaryWriter writer, Quaternion rotation)
        {
            writer.Write(rotation.x);
            writer.Write(rotation.y);
            writer.Write(rotation.z);
            writer.Write(rotation.w);
        }

        #endregion

        #region Dictionary Serialization

        public static void Write(BinaryWriter writer, Dictionary<string, object> properties)
        {
            // Write dictionary count
            writer.Write(properties.Count);

            // Write each key-value pair
            foreach (var kvp in properties)
            {
                // Write the key
                writer.Write(kvp.Key);

                // Write the value with its type marker
                WriteTypedValue(writer, kvp.Value);
            }
        }

        // Method to write a typed value with its marker
        private static void WriteTypedValue(BinaryWriter writer, object value)
        {
            // Handle null values
            if (value == null)
            {
                writer.Write((byte)TypeMarker.Null);
                return;
            }

            // Handle value based on type
            switch (value)
            {
                case bool boolean:
                    writer.Write((byte)TypeMarker.Bool);
                    writer.Write(boolean);
                    break;

                case byte b:
                    writer.Write((byte)TypeMarker.Byte);
                    writer.Write(b);
                    break;

                case char c:
                    writer.Write((byte)TypeMarker.Char);
                    writer.Write(c);
                    break;

                case short s:
                    writer.Write((byte)TypeMarker.Short);
                    writer.Write(s);
                    break;

                case int integer:
                    writer.Write((byte)TypeMarker.Int);
                    writer.Write(integer);
                    break;

                case long l:
                    writer.Write((byte)TypeMarker.Long);
                    writer.Write(l);
                    break;

                case float f:
                    writer.Write((byte)TypeMarker.Float);
                    writer.Write(f);
                    break;

                case double d:
                    writer.Write((byte)TypeMarker.Double);
                    writer.Write(d);
                    break;

                case string str:
                    writer.Write((byte)TypeMarker.String);
                    writer.Write(str);
                    break;

                case Array array:
                    writer.Write((byte)TypeMarker.Array);
                    WriteArray(writer, array);
                    break;

                case IDictionary dictionary:
                    writer.Write((byte)TypeMarker.Dictionary);
                    WriteDictionary(writer, dictionary);
                    break;

                case PersistantEntity entity:
                    writer.Write((byte)TypeMarker.Entity);
                    entity.Save(writer);
                    break;

                case PersistantItem item:
                    writer.Write((byte)TypeMarker.Item);
                    item.Save(writer);
                    break;

                default:
                    writer.Write((byte)TypeMarker.Object);
                    writer.Write(value.ToString());
                    break;
            }
        }

        private static void WriteArray(BinaryWriter writer, Array array)
        {
            // Write array length
            writer.Write(array.Length);

            // Write array element type
            Type elementType = array.GetType().GetElementType();
            WriteTypeInfo(writer, elementType);

            // Write each element
            foreach (var element in array)
            {
                if (element == null)
                {
                    writer.Write(false); // Not present
                }
                else
                {
                    writer.Write(true); // Present
                    WriteValue(writer, element);
                }
            }
        }

        private static void WriteDictionary(BinaryWriter writer, IDictionary dictionary)
        {
            // Write dictionary count
            writer.Write(dictionary.Count);

            // Write key-value pairs
            foreach (DictionaryEntry entry in dictionary)
            {
                WriteValue(writer, entry.Key);
                WriteValue(writer, entry.Value);
            }
        }

        private static void WriteTypeInfo(BinaryWriter writer, Type type)
        {
            // Write the type name as a string
            writer.Write(type.AssemblyQualifiedName);
        }

        private static void WriteValue(BinaryWriter writer, object value)
        {
            if (value == null)
            {
                writer.Write((byte)TypeMarker.Null);
                return;
            }

            // Use ToString for simplicity - in a complete implementation
            // this would call back to WriteTypedValue
            writer.Write(value.ToString());
        }

        #endregion

        #region Dictionary Deserialization

        // Read method for dictionaries
        public static Dictionary<string, object> ReadDictionary(BinaryReader reader)
        {
            // Read dictionary count
            int count = reader.ReadInt32();
            var properties = new Dictionary<string, object>(count);

            // Read each key-value pair
            for (int i = 0; i < count; i++)
            {
                // Read key
                string key = reader.ReadString();

                // Read typed value
                object value = ReadTypedValue(reader);

                properties[key] = value;
            }

            return properties;
        }

        // Read a typed value based on its marker
        private static object ReadTypedValue(BinaryReader reader)
        {
            // Read type marker
            TypeMarker typeMarker = (TypeMarker)reader.ReadByte();

            // Read value based on type marker
            return typeMarker switch
            {
                TypeMarker.Null => null,
                TypeMarker.Bool => reader.ReadBoolean(),
                TypeMarker.Byte => reader.ReadByte(),
                TypeMarker.Char => reader.ReadChar(),
                TypeMarker.Short => reader.ReadInt16(),
                TypeMarker.Int => reader.ReadInt32(),
                TypeMarker.Long => reader.ReadInt64(),
                TypeMarker.Float => reader.ReadSingle(),
                TypeMarker.Double => reader.ReadDouble(),
                TypeMarker.String => reader.ReadString(),
                TypeMarker.Array => ReadArray(reader),
                TypeMarker.Dictionary => ReadDictionary(reader),
                TypeMarker.Entity => PersistantEntity.Load(reader),
                TypeMarker.Item => PersistantItem.Load(reader),
                TypeMarker.Object => reader.ReadString(),
                _ => throw new InvalidOperationException($"Unknown type marker: {typeMarker}"),
            };
        }

        // Helper method for reading arrays
        private static Array ReadArray(BinaryReader reader)
        {
            // Read array length
            int length = reader.ReadInt32();

            // Read element type
            Type elementType = ReadTypeInfo(reader);

            // Create array
            Array array = Array.CreateInstance(elementType, length);

            // Read elements
            for (int i = 0; i < length; i++)
            {
                bool isPresent = reader.ReadBoolean();
                if (isPresent)
                {
                    object element = ReadValue(reader);
                    array.SetValue(element, i);
                }
            }

            return array;
        }

        // Helper method for reading type info
        private static Type ReadTypeInfo(BinaryReader reader)
        {
            string typeName = reader.ReadString();
            return Type.GetType(typeName);
        }

        // Helper method for reading a value of any type
        private static object ReadValue(BinaryReader reader)
        {
            // This is simplified - in a complete implementation
            // this would call back to ReadTypedValue
            return reader.ReadString();
        }
        #endregion

        #region Read Methods for Basic Types

        public static VersionNumber ReadVersionNumber(BinaryReader reader)
        {
            int major = reader.ReadInt32();
            int minor = reader.ReadInt32();
            int patch = reader.ReadInt32();
            return new VersionNumber(major, minor, patch);
        }

        public static Vector3 ReadVector3(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            return new Vector3(x, y, z);
        }

        public static Vector4 ReadVector4(BinaryReader reader)
        {
            Vector3 v3 = ReadVector3(reader);
            float w = reader.ReadSingle();
            return new Vector4(v3.x, v3.y, v3.z, w);
        }

        public static Quaternion ReadQuaternion(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();
            return new Quaternion(x, y, z, w);
        }

        #endregion
    }
}
