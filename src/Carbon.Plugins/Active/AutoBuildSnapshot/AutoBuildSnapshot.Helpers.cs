using Facepunch;
using System;
using System.Collections.Generic;
using System.IO;
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

    private static class DataLength
    {
        public const int Int32 = sizeof(int);
        public const int Single = sizeof(float);
        public const int Vector3 = Single * 3;
    }
}
