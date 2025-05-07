using Facepunch;
using System;
using System.Collections.Generic;
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
}
