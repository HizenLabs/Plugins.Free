using Facepunch;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Helper class for managing the plugin's configuration and language settings.
    /// </summary>
    private static class Helpers
    {
        private static List<string> _logs;

        /// <summary>
        /// Initializes the helper with the plugin instance and obtains resources from the pool.
        /// </summary>
        /// <param name="plugin"></param>
        public static void Init()
        {
            _logs = Pool.Get<List<string>>();
        }

        /// <summary>
        /// Frees the helper resources.
        /// </summary>
        public static void Unload()
        {
            Pool.FreeUnmanaged(ref _logs);
        }

        #region Logging

        /// <summary>
        /// Logs a localized message to the console and to the player if provided.
        /// </summary>
        /// <param name="langKey">The language key for the message.</param>
        /// <param name="player">The player to send the message to (optional).</param>
        /// <param name="args">The arguments to format the message with.</param>
        public static void Log(LangKeys langKey, BasePlayer player = null, params object[] args)
        {
            Log(Localizer.GetFormat(player, langKey), player, args);
        }

        /// <summary>
        /// Logs a message to the console and to the player if provided.
        /// </summary>
        /// <param name="format">The message to log.</param>
        /// <param name="player">The player to send the message to (optional).</param>
        public static void Log(string format, BasePlayer player = null, params object[] args)
        {
            if (_instance == null) return;

            if (args != null && args.Length > 0)
            {
                format = string.Format(format, args);
            }

            if (player != null)
            {
                _instance.SendReply(player, format);
                format = $"[Sent: {player.displayName}] {format}";
            }

            _instance.Puts(format);
            _logs.Add(format);
        }

        #endregion

        #region Stream

        /// <summary>
        /// Reads an integer value from a byte array at the specified offset and updates the offset.
        /// </summary>
        /// <param name="buffer">The byte array to read from.</param>
        /// <param name="offset">The offset in the byte array to start reading from.</param>
        /// <returns>The integer value read from the byte array.</returns>
        public static int ReadInt32(byte[] buffer, ref int offset)
        {
            int value;
            unsafe
            {
                fixed (byte* ptr = &buffer[offset])
                {
                    value = *(int*)ptr;
                }
            }
            offset += sizeof(int);
            return value;
        }

        /// <summary>
        /// Writes a string to a byte array at the specified offset and updates the offset.
        /// </summary>
        /// <param name="buffer">The byte array to write to.</param>
        /// <param name="value">The string value to write.</param>
        /// <param name="offset">The offset in the byte array to start writing at.</param>
        public static void WriteInt32(byte[] buffer, int value, ref int offset)
        {
            unsafe
            {
                fixed (byte* ptr = &buffer[offset])
                {
                    *(int*)ptr = value;
                }
            }
            offset += sizeof(int);
        }

        /// <summary>
        /// Reads a float value from a byte array at the specified offset and updates the offset.
        /// </summary>
        /// <param name="buffer">The byte array to read from.</param>
        /// <param name="offset">The offset in the byte array to start reading from.</param>
        /// <returns>The float value read from the byte array.</returns>
        public static float ReadSingle(byte[] buffer, ref int offset)
        {
            float value;
            unsafe
            {
                fixed (byte* ptr = &buffer[offset])
                {
                    value = *(float*)ptr;
                }
            }
            offset += sizeof(float);
            return value;
        }

        /// <summary>
        /// Writes a float value to a byte array at the specified offset and updates the offset.
        /// </summary>
        /// <param name="buffer">The byte array to write to.</param>
        /// <param name="value">The float value to write.</param>
        /// <param name="offset">The offset in the byte array to start writing at.</param>
        /// <returns>The number of bytes written.</returns>
        public static void WriteSingle(byte[] buffer, float value, ref int offset)
        {
            unsafe
            {
                fixed (byte* ptr = &buffer[offset])
                {
                    *(float*)ptr = value;
                }
            }
            offset += sizeof(float);
        }

        /// <summary>
        /// Reads a Vector3 value from a byte array at the specified offset and updates the offset.
        /// </summary>
        /// <param name="buffer">The byte array to read from.</param>
        /// <param name="offset">The offset in the byte array to start reading from.</param>
        /// <returns>The Vector3 value read from the byte array.</returns>
        public static Vector3 ReadVector3(byte[] buffer, ref int offset)
        {
            var x = ReadSingle(buffer, ref offset);
            var y = ReadSingle(buffer, ref offset);
            var z = ReadSingle(buffer, ref offset);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Writes a Vector3 value to a byte array at the specified offset and updates the offset.
        /// </summary>
        /// <param name="buffer">The byte array to write to.</param>
        /// <param name="value">The Vector3 value to write.</param>
        /// <param name="offset">The offset in the byte array to start writing at.</param>
        public static void WriteVector3(byte[] buffer, Vector3 value, ref int offset)
        {
            WriteSingle(buffer, value.x, ref offset);
            WriteSingle(buffer, value.y, ref offset);
            WriteSingle(buffer, value.z, ref offset);
        }

        #endregion

        #region Game Engine

        /// <summary>
        /// Tries to get the player target building.
        /// </summary>
        /// <param name="player">The player whose target building is being checked.</param>
        /// <param name="building">The building that was hit by the raycast.</param>
        /// <returns>True if the raycast hit a building; otherwise, false.</returns>
        public static bool TryGetTargetBuilding(BasePlayer player, out BuildingManager.Building building)
        {
            const float targetDistance = 20f;

            building = null;
            if (!TryGetTargetEntity(player, out var entity, targetDistance))
            {
                Localizer.ChatMessage(player, LangKeys.error_must_face_target, targetDistance);

                return false;
            }

            var priv = entity.GetBuildingPrivilege();
            if (!priv)
            {
                Localizer.ChatMessage(player, LangKeys.error_building_priv_missing);
                return false;
            }

            building = priv.GetBuilding();
            if (building == null)
            {
                Localizer.ChatMessage(player, LangKeys.error_building_null);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to get the target entity from the player's eyes.
        /// </summary>
        /// <param name="player">The player whose eyes are used for the raycast.</param>
        /// <param name="entity">The entity that was hit by the raycast.</param>
        /// <param name="distance">The maximum distance for the raycast.</param>
        /// <returns>True if the raycast hit an entity; otherwise, false.</returns>
        public static bool TryGetTargetEntity(BasePlayer player, out BaseEntity entity, float distance = float.PositiveInfinity)
        {
            if (TryGetPlayerEyesRaycast(player, out var hit, distance, Masks.BaseEntities))
            {
                entity = hit.GetEntity();
                return entity != null;
            }

            entity = null;
            return false;
        }

        /// <summary>
        /// Tries to get a hit from the player's eyes.
        /// </summary>
        /// <param name="player">The player whose eyes are used for the raycast.</param>
        /// <param name="hit">The RaycastHit object that contains information about the hit.</param>
        /// <param name="distance">The maximum distance for the raycast.</param>
        /// <param name="layerMask">The layer mask to use for the raycast.</param>
        /// <returns>True if the raycast hit an entity; otherwise, false.</returns>
        private static bool TryGetPlayerEyesRaycast(BasePlayer player, out RaycastHit hit, float distance = float.PositiveInfinity, int? layerMask = null)
        {
            var raycast = GetPlayerEyesRay(player);

            if (Physics.Raycast(raycast, out hit, distance, layerMask ?? Masks.Default))
            {
                return hit.collider != null;
            }

            return false;
        }

        /// <summary>
        /// Creates a ray from the player's eyes in the direction they are looking.
        /// </summary>
        /// <param name="player">The player whose eyes are used for the ray.</param>
        /// <returns>A Ray object representing the player's eyes ray.</returns>
        private static Ray GetPlayerEyesRay(BasePlayer player)
        {
            return new(player.eyes.position, player.eyes.HeadForward());
        }

        #endregion

        #region Misc

        /// <summary>
        /// Tries to parse a string into a specified type.
        /// </summary>
        /// <typeparam name="T">The type to parse the string into.</typeparam>
        /// <param name="input">The string to parse.</param>
        /// <param name="value">The parsed value of type T.</param>
        /// <returns>True if the parsing was successful; otherwise, false.</returns>
        public static bool TryParse<T>(string input, out T value)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null && converter.IsValid(input))
                {
                    value = (T)converter.ConvertFromString(input);
                    return true;
                }
            }
            catch { }

            value = default;
            return false;
        }

        #endregion
    }

    /// <summary>
    /// Helper class to manage layer masks.
    /// </summary>
    private static class Masks
    {
        /// <summary>
        /// Default layer mask.
        /// </summary>
        public static int Default { get; }

        /// <summary>
        /// Deployed layer mask.
        /// </summary>
        public static int Deployed { get; }

        /// <summary>
        /// Construction layer mask.
        /// </summary>
        public static int Construction { get; }

        /// <summary>
        /// Vehicle layer mask.
        /// </summary>
        public static int Vehicle { get; }

        /// <summary>
        /// Harvestable layer mask.
        /// </summary>
        public static int Harvestable { get; }

        /// <summary>
        /// Combined layer mask for all base entities (Default, Deployed, Construction, Vehicle, Harvestable).
        /// </summary>
        public static int BaseEntities { get; }

        /// <summary>
        /// Ground layer mask.
        /// </summary>
        public static int Ground { get; }

        /// <summary>
        /// Initializes the layer masks.
        /// </summary>
        static Masks()
        {
            Default = LayerMask.GetMask("Default");
            Ground = LayerMask.GetMask("Ground", "Terrain", "World");
            Deployed = LayerMask.GetMask("Deployed");
            Construction = LayerMask.GetMask("Construction");
            Vehicle = LayerMask.GetMask("Vehicle Detailed", "Vehicle World", "Vehicle Large");
            Harvestable = LayerMask.GetMask("Harvestable");

            BaseEntities = Default | Deployed | Construction | Vehicle | Harvestable;
        }
    }

    /// <summary>
    /// Helper class for managing pooled dictionaries.
    /// </summary>
    /// <typeparam name="TKey"> The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    private class PooledDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable, Pool.IPooled
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PooledDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="collection"> The existing collection to add from.</param>
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (var item in collection)
            {
                Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Clears the dictionary and prepares it for reuse.
        /// </summary>
        public void Dispose()
        {
            var obj = this;
            Pool.Free(ref obj);
        }

        /// <summary>
        /// Enters the pool, clearing the dictionary for reuse.
        /// </summary>
        void Pool.IPooled.EnterPool()
        {
            Clear();
        }

        /// <summary>
        /// Leaves the pool, allowing the dictionary to be used again.
        /// </summary>
        void Pool.IPooled.LeavePool()
        {
        }
    }

    /// <summary>
    /// Simply holds the length (in bytes) of the data types used in the plugin.
    /// </summary>
    private static class DataLength
    {
        /// <summary>
        /// Length of the int data type.
        /// </summary>
        public const int Int32 = sizeof(int);

        /// <summary>
        /// Length of the float data type.
        /// </summary>
        public const int Single = sizeof(float);

        /// <summary>
        /// Length of the Vector3 data type.
        /// </summary>
        public const int Vector3 = Single * 3;
    }
}
