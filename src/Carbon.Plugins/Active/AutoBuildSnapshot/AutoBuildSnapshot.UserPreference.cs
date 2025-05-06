using Newtonsoft.Json;
using System.Collections.Generic;

namespace Carbon.Plugins.Active.AutoBuildSnapshot;

public partial class AutoBuildSnapshot
{
    #region Settings

    /// <summary>
    /// Handles the AutoBuildSnapshot settings and defaults.
    /// </summary>
    private static class Settings
    {
        /// <summary>
        /// The default settings for the AutoBuildSnapshot plugin.
        /// </summary>
        public static class Defaults
        {
            /// <inheritdoc cref="AutoBuildSnapshotConfig.General"/>
            public static class General
            {
                /// <inheritdoc cref="AutoBuildSnapshotConfig.GeneralSettings.BuildMonitorInterval"/>
                public const int BuildMonitorInterval = 600;

                /// <inheritdoc cref="AutoBuildSnapshotConfig.GeneralSettings.DelayBetweenSaves"/>
                public const int DelayBetweenSaves = 3600;

                /// <inheritdoc cref="AutoBuildSnapshotConfig.GeneralSettings.SnapshotRetentionPeriodHours"/>
                public const int SnapshotRetentionPeriodHours = 720;

                /// <inheritdoc cref="AutoBuildSnapshotConfig.GeneralSettings.IncludeGroundResources"/>
                public const bool IncludeGroundResources = true;

                /// <inheritdoc cref="AutoBuildSnapshotConfig.GeneralSettings.IncludeNonAuthorizedDeployables"/>
                public const bool IncludeNonAuthorizedDeployables = true;
            }

            /// <inheritdoc cref="AutoBuildSnapshotConfig.MultiTC"/>
            public static class MultiTC
            {
                /// <inheritdoc cref="AutoBuildSnapshotConfig.MultiTCSettings.Mode"/>
                public const MultiTCMode Mode = MultiTCMode.Disabled;

                /// <inheritdoc cref="AutoBuildSnapshotConfig.MultiTCSettings.ScanRadius"/>
                public const float ScanRadius = 80;
            }

            /// <inheritdoc cref="AutoBuildSnapshotConfig.Commands"/>
            public static class Commands
            {
                /// <inheritdoc cref="AutoBuildSnapshotConfig.CommandSettingCollection.AdminPermission"/>
                public const string AdminPermission = "abs.admin";

                /// <summary>
                /// The default name and permission for the <see cref="AutoBuildSnapshotConfig.CommandSettingCollection.ToggleMenu"/> command.
                /// </summary>
                public const string ToggleMenu = "abs.menu";

                /// <summary>
                /// The default name and permission for the <see cref="AutoBuildSnapshotConfig.CommandSettingCollection.Backup"/> command.
                /// </summary>
                public const string Backup = "abs.backup";

                /// <summary>
                /// The default name and permission for the <see cref="AutoBuildSnapshotConfig.CommandSettingCollection.Rollback"/> command.
                /// </summary>
                public const string Rollback = "abs.rollback";
            }

            /// <inheritdoc cref="AutoBuildSnapshotConfig.Advanced"/>
            public static class Advanced
            {
                /// <inheritdoc cref="AutoBuildSnapshotConfig.AdvancedSettings.FoundationPrivilegeRadius"/>
                public const float FoundationPrivRadius = 40;

                /// <inheritdoc cref="AutoBuildSnapshotConfig.AdvancedSettings.MaxSaveFailRetries"/>
                public const int MaxSaveFailRetries = 3;

                /// <inheritdoc cref="AutoBuildSnapshotConfig.AdvancedSettings.MaxStepDuration"/>
                public const int MaxStepDuration = 50;

                /// <inheritdoc cref="AutoBuildSnapshotConfig.AdvancedSettings.MaxScanZoneRadius"/>
                public const float MaxScanZoneRadius = 100;
            }
        }
    }

    /// <summary>
    /// Represents the configuration.
    /// </summary>
    private class AutoBuildSnapshotConfig
    {
        /// <summary>
        /// The general settings category.
        /// </summary>
        [JsonProperty("General Settings")]
        public GeneralSettings General { get; init; } = new();
        /// <summary>
        /// The settings for multi-TC buildings.
        /// </summary>
        [JsonProperty("Multi-TC Settings")]
        public MultiTCSettings MultiTC { get; init; } = new();

        [JsonProperty("Command Settings")]
        public CommandSettingCollection Commands { get; set; } = new();

        /// <summary>
        /// Fine tune settings for performance.
        /// </summary>
        [JsonProperty("Advanced Settings (Caution: Changes may cause lag)")]
        public AdvancedSettings Advanced { get; init; } = new();

        /// <summary>
        /// The general settings category.
        /// </summary>
        public class GeneralSettings
        {
            /// <summary>
            /// The interval, in seconds, to check if snapshots are needed.
            /// </summary>
            [JsonProperty("Update Check Interval (seconds)")]
            public float BuildMonitorInterval { get; set; } = Settings.Defaults.General.BuildMonitorInterval;

            /// <summary>
            /// The minimum delay between snapshots.
            /// </summary>
            [JsonProperty("Delay Between Snapshots (seconds)")]
            public float DelayBetweenSaves { get; set; } = Settings.Defaults.General.DelayBetweenSaves;

            /// <summary>
            /// The number of hours to keep snapshots before they are deleted.
            /// </summary>
            [JsonProperty("Snapshot Retention Period (hours)")]
            public int SnapshotRetentionPeriodHours { get; set; } = Settings.Defaults.General.SnapshotRetentionPeriodHours;

            /// <summary>
            /// Whether to include ground resources in the backup that are not player-owned.
            /// </summary>
            [JsonProperty("Include Ground Resources")]
            public bool IncludeGroundResources { get; set; } = Settings.Defaults.General.IncludeGroundResources;

            /// <summary>
            /// Whether to include non-authorized deployables in the backup.
            /// </summary>
            [JsonProperty("Include Non-Authorized Deployables")]
            public bool IncludeNonAuthorizedDeployables { get; set; } = Settings.Defaults.General.IncludeNonAuthorizedDeployables;
        }

        /// <summary>
        /// The settings for multi-TC buildings.
        /// </summary>
        public class MultiTCSettings
        {
            /// <summary>
            /// Whether to try to link multiple TCs together when they are in the same area.
            /// </summary>
            [JsonProperty("Multi-TC Snapshots ModeLegend")]
            public Dictionary<string, MultiTCMode> ModeLegend => _modeLegend;
            private static readonly Dictionary<string, MultiTCMode> _modeLegend = new()
            {
                ["Disabled"] = MultiTCMode.Disabled
            };

            /// <summary>
            /// Whether to try to link multiple TCs together when they are in the same area.
            /// </summary>
            [JsonProperty("Multi-TC Snapshots Mode")]
            public MultiTCMode Mode { get; set; } = Settings.Defaults.MultiTC.Mode;

            /// <summary>
            /// The radius to scan for other TCs when trying to link multiple TCs together.
            /// </summary>
            [JsonProperty("Multi-TC Scan Radius (Automatic Mode)")]
            public float ScanRadius { get; set; } = Settings.Defaults.MultiTC.ScanRadius;
        }

        /// <summary>
        /// Allows for customization of command names and permissions.
        /// </summary>
        public class CommandSettingCollection
        {
            /// <summary>
            /// Name of the admin permission to grant permission to all commands.
            /// </summary>
            [JsonProperty("Admin Permission")]
            public string AdminPermission { get; set; } = Settings.Defaults.Commands.AdminPermission;

            /// <summary>
            /// Settings for the toggle menu command.
            /// </summary>
            [JsonProperty("Toggle Menu")]
            public CommandSetting ToggleMenu { get; set; } = new(Settings.Defaults.Commands.ToggleMenu);

            /// <summary>
            /// Settings for the manual backup command.
            /// </summary>
            [JsonProperty("Backup (manual)")]
            public CommandSetting Backup { get; set; } = new(Settings.Defaults.Commands.Backup);

            /// <summary>
            /// Settings for the rollback command.
            /// </summary>
            [JsonProperty("Rollback")]
            public CommandSetting Rollback { get; set; } = new(Settings.Defaults.Commands.Rollback);

            /// <summary>
            /// Checks if the user has permission to run the command.
            /// </summary>
            /// <param name="player">The player to check.</param>
            /// <param name="command">The command to check.</param>
            /// <param name="plugin">The plugin instance.</param>
            /// <returns>True if the user has permission, false otherwise.</returns>
            public bool UserHasPermission(BasePlayer player, CommandSetting command, AutoBuildSnapshot_old plugin) =>
                plugin.permission.UserHasPermission(player.UserIDString, command.Permission)
                || plugin.permission.UserHasPermission(player.UserIDString, AdminPermission);
        }

        /// <summary>
        /// Represents a command setting.
        /// </summary>
        public class CommandSetting
        {
            public CommandSetting() { }

            public CommandSetting(string name)
            {
                Alias = name;
                Permission = name;
            }

            /// <summary>
            /// The name of the command to type into chat.
            /// </summary>
            [JsonProperty("Alias")]
            public string Alias { get; set; }

            /// <summary>
            /// The permission required to run the command.
            /// </summary>
            [JsonProperty("Permission")]
            public string Permission { get; set; }
        }

        /// <summary>
        /// Fine tune settings for performance.
        /// </summary>
        public class AdvancedSettings
        {
            /// <summary>
            /// The default radius is something close to 37, but we use 40 to catch a broader range.
            /// We also want to allow users to change in-case they have plugins or something using a different radius.
            /// </summary>
            [JsonProperty("Foundation Privilege Radius")]
            public float FoundationPrivilegeRadius { get; set; } = Settings.Defaults.Advanced.FoundationPrivRadius;

            /// <summary>
            /// The maximum number of retries to save a snapshot before giving up.
            /// </summary>
            [JsonProperty("Max Retry Attempts on Failure")]
            public int MaxSaveFailRetries { get; set; } = Settings.Defaults.Advanced.MaxSaveFailRetries;

            /// <summary>
            /// The maximum duration, in milliseconds, that we can process a step 
            /// before yielding it to the game loop (may step into the next frame).
            /// </summary>
            [JsonProperty("Max Step Duration (ms)")]
            public int MaxStepDuration { get; set; } = Settings.Defaults.Advanced.MaxStepDuration;

            /// <summary>
            /// The maximum radius to create zones before splitting them up.
            /// This is used when scanning entities for snapshots.
            /// </summary>
            [JsonProperty("Max Zone Scan Radius")]
            public float MaxScanZoneRadius { get; set; } = Settings.Defaults.Advanced.MaxScanZoneRadius;

            /// <summary>
            /// List of data formats that can be used to save the snapshot data.
            /// </summary>
            [JsonProperty("Data Save FormatLegend")]
            public Dictionary<string, DataFormat> DataSaveFormatLegend => _dataFormatLegend;
            private static readonly Dictionary<string, DataFormat> _dataFormatLegend = new()
            {
                ["Binary"] = DataFormat.Binary,
                ["Binary (GZip Compressed)"] = DataFormat.GZip,
                /*
                ["Json"] = DataFormat.Json,
                ["Json (Expanded)"] = DataFormat.JsonExpanded
                */
            };

            /// <summary>
            /// The type to use when saving the snapshot data.
            /// </summary>
            [JsonProperty("Data Save Format")]
            public DataFormat DataSaveFormat { get; set; } = DataFormat.GZip;
        }
    }

    /// <summary>
    /// Represents the mode for multi-TC buildings.
    /// </summary>
    /// <remarks>
    /// This feature is currently disabled.
    /// </remarks>
    private enum MultiTCMode
    {
        Disabled = 0,
        // Manual = 1,
        // Automatic = 2
    }

    /// <summary>
    /// Represents the format of the data when saving the snapshot.
    /// </summary>
    private enum DataFormat
    {
        Binary = 0,
        GZip = 1,
    }

    #endregion
}
