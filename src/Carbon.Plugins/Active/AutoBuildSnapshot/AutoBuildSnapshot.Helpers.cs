using Facepunch;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private static class Shared
    {
        public static ArrayPool<LogMessage> LogMessageArrayPool = new(512);

        public static ArrayPool<object> ArgumentsPool = new(4);
    }

    private class LogMessage : Pool.IPooled
    {
        private LangKeys _langKey;
        private TempArguments _args;

        public DateTime Timestamp => _timestamp;
        private DateTime _timestamp;

        public string GetMessage(BasePlayer player)
        {
            var message = Localizer.GetFormat(player, _langKey);
            message = _args.StringFormat(message);

            return message;
        }

        public static LogMessage Create(LangKeys langKey, object arg1, object arg2, object arg3)
        {
            var instance = Pool.Get<LogMessage>();
            instance._langKey = langKey;
            instance._args = TempArguments.Create(arg1, arg2, arg3);
            return instance;
        }

        void Pool.IPooled.EnterPool()
        {
            _langKey = LangKeys.Default;
            _timestamp = default;

            _args?.Dispose();
        }

        void Pool.IPooled.LeavePool()
        {
            _timestamp = DateTime.UtcNow;
        }
    }

    private class TempArguments : IDisposable, Pool.IPooled
    {
        private object[] _args;

        public static TempArguments Create(object arg1)
        {
            var instance = Pool.Get<TempArguments>();
            if (arg1 == null)
            {
                return instance;
            }

            instance._args = Shared.ArgumentsPool.Rent(1);
            instance._args[0] = arg1;
            return instance;
        }

        public static TempArguments Create(object arg1, object arg2)
        {
            if (arg2 == null)
            {
                return Create(arg1);
            }

            var instance = Pool.Get<TempArguments>();
            instance._args = Shared.ArgumentsPool.Rent(2);
            instance._args[0] = arg1;
            instance._args[1] = arg2;
            return instance;
        }

        public static TempArguments Create(object arg1, object arg2, object arg3)
        {
            if (arg3 == null)
            {
                return Create(arg1, arg2);
            }

            var instance = Pool.Get<TempArguments>();
            instance._args = Shared.ArgumentsPool.Rent(3);
            instance._args[0] = arg1;
            instance._args[1] = arg2;
            instance._args[2] = arg3;
            return instance;
        }

        public string StringFormat(string format)
        {
            if (_args == null)
            {
                return format;
            }

            return string.Format(format, _args);
        }

        public void Dispose()
        {
            var obj = this;
            Pool.Free(ref obj);
        }

        public void EnterPool()
        {
            if (_args != null)
            {
                Shared.ArgumentsPool.Return(_args);
                _args = null;
            }
        }

        public void LeavePool()
        {
        }

        public static implicit operator object[](TempArguments instance)
        {
            return instance._args;
        }
    }

    /// <summary>
    /// Helper class for managing the plugin's configuration and language settings.
    /// </summary>
    private static class Helpers
    {
        private const int _logsLength = 500;

        private static LogMessage[] _logs;
        private static int _logIndex = 0;
        private static int _logCount = 0;
        private static readonly object syncRoot = new();

        /// <summary>
        /// Initializes the helper with the plugin instance and obtains resources from the pool.
        /// </summary>
        /// <param name="plugin"></param>
        public static void Init()
        {
            _logs = Shared.LogMessageArrayPool.Rent(_logsLength);
        }

        /// <summary>
        /// Frees the helper resources.
        /// </summary>
        public static void Unload()
        {
            Shared.LogMessageArrayPool.Return(_logs);
        }

        #region Logging

        /// <summary>
        /// Logs a localized message to the console and to the player if provided.
        /// </summary>
        /// <param name="langKey">The language key for the message.</param>
        /// <param name="player">The player to send the message to (optional).</param>
        public static void Log(
            LangKeys langKey,
            BasePlayer player = null,
            object arg1 = null,
            object arg2 = null,
            object arg3 = null)
        {
            var logMessage = LogMessage.Create(langKey, arg1, arg2, arg3);

            AddLog(logMessage);
            SendLog(logMessage, player);
        }

        /// <summary>
        /// Logs a message to the console and to the player if provided.
        /// </summary>
        /// <param name="format">The message to log.</param>
        /// <param name="player">The player to send the message to (optional).</param>
        private static void SendLog(LogMessage logMessage, BasePlayer player = null)
        {
            if (_instance == null) return;
            var message = logMessage.GetMessage(player);

            if (player != null)
            {
                _instance.SendReply(player, message);
                message = $"[Sent: {player.displayName}] {message}";
            }

            _instance.Puts(message);
        }

        private static void AddLog(LogMessage logMessage)
        {
            lock (syncRoot)
            {
                _logs[_logIndex] = logMessage;
                _logIndex = (_logIndex + 1) % _logsLength;
                _logCount = Math.Min(_logCount + 1, _logsLength);
            }
        }

        public static void ClearLogs()
        {
            lock (syncRoot)
            {
                _logCount = 0;
                _logIndex = 0;
            }
        }

        /// <summary>
        /// Gets the logs for a player in a specified format.
        /// </summary>
        /// <param name="player">The player to get the logs for.</param>
        /// <param name="format">The format string for the logs.</param>
        /// <returns>A list of log messages formatted for the player.</returns>
        /// <remarks>
        /// Format options:
        /// <list type="bullet">
        /// <item>
        /// <description>{0}</description>
        /// <description>The log message.</description>
        /// </item>
        /// <item>
        /// <description>{1}</description>
        /// <description>The timestamp of the log message.</description>
        /// </item>
        /// <item>
        /// <description>{2}</description>
        /// <description>The timestamp of the log message, localized to the server's local time.</description>
        /// </item>
        /// <item>
        /// <description>{3}</description>
        /// <description>The player's display name.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static PooledList<string> GetLogs(BasePlayer player, string format = _defaultLogFormat)
        {
            var logs = Pool.Get<PooledList<string>>();

            lock (syncRoot)
            {
                for (int i = 0; i < _logCount; i++)
                {
                    int index = (_logIndex - 1 - i + _logsLength) % _logsLength;
                    var logMessage = _logs[index];
                    var message = logMessage.GetMessage(player);

                    if (!string.IsNullOrEmpty(format))
                    {
                        var args = Shared.ArgumentsPool.Rent(4);
                        args[0] = message;
                        args[1] = logMessage.Timestamp;
                        args[2] = logMessage.Timestamp.ToLocalTime();
                        args[3] = player.displayName;

                        message = string.Format(format, args);
                        Shared.ArgumentsPool.Return(args);
                    }

                    logs.Add(message);

                }
            }

            return logs;
        }

        private const string _defaultLogFormat = "[{2:G}] {0}";

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
