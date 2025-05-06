using Facepunch;
using System.Collections.Generic;
using UnityEngine;

namespace Carbon.Plugins.Active.AutoBuildSnapshot;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Helper class for managing the plugin's configuration and language settings.
    /// </summary>
    private static class Helpers
    {
        private static AutoBuildSnapshot _plugin;
        private static List<string> _logs;

        /// <summary>
        /// Initializes the helper with the plugin instance and obtains resources from the pool.
        /// </summary>
        /// <param name="plugin"></param>
        public static void Init(AutoBuildSnapshot plugin)
        {
            _plugin = plugin;
            _logs = Pool.Get<List<string>>();
        }

        /// <summary>
        /// Frees the helper resources.
        /// </summary>
        public static void Unload()
        {
            _plugin = null;
            Pool.FreeUnmanaged(ref _logs);
        }

        /// <summary>
        /// Logs a message to the console and to the player if provided.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="player">The player to send the message to (optional).</param>
        public static void Log(string message, BasePlayer player = null)
        {
            if (_plugin == null) return;

            if (player != null)
            {
                _plugin.SendReply(player, message);
                message = $"[Sent: {player.displayName}] {message}";
            }

            _plugin.Puts(message);
            _logs.Add(message);
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
}
