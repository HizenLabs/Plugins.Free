using Carbon.Components;
using Carbon.Modules;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    #region Localization

    /// <summary>
    /// Loads the default localized messages.
    /// </summary>
    protected override void LoadDefaultMessages()
    {
        base.LoadDefaultMessages();

        Localizer.RegisterMessages(this);
    }

    /// <summary>
    /// Handles localization resources for the plugin.
    /// </summary>
    private static class Localizer
    {
        /// <summary>
        /// Gets the localized message format for the specified key and player.
        /// </summary>
        /// <param name="player">The player to get the message for.</param>
        /// <param name="langKey">The key for the localized message.</param>
        /// <returns>The localized message format.</returns>
        public static string GetFormat(BasePlayer player, LangKeys langKey)
        {
            var key = langKey.ToString();
            var userId = player?.UserIDString;

            return _instance.lang.GetMessage(key, _instance, userId);
        }

        /// <summary>
        /// Sends a reply to the player with the localized message.
        /// </summary>
        /// <param name="player">The player to send the message to.</param>
        /// <param name="langKey">The key for the localized message.</param>
        /// <param name="args">The arguments to format the message with.</param>
        public static void ChatMessage(BasePlayer player, LangKeys langKey, params object[] args)
        {
            var msg = Text(langKey, player, args);

            player.ChatMessage(msg);
        }

        /// <summary>
        /// Gets the localized message format for the specified key and player, and formats it with the provided arguments.
        /// </summary>
        /// <param name="player">The player to get the message for.</param>
        /// <param name="langKey">The key for the localized message.</param>
        /// <param name="args">The arguments to format the message with.</param>
        /// <returns>The formatted localized message.</returns>
        public static string Text(LangKeys langKey, BasePlayer player = null, params object[] args)
        {
            var format = GetFormat(player, langKey);

            return string.Format(format, args);
        }

        /// <summary>
        /// Registers the default localized messages.
        /// </summary>
        public static void RegisterMessages(CarbonPlugin plugin)
        {
            plugin.lang.RegisterMessages(new()
            {
                [nameof(LangKeys.error_no_permission)] = "You do not have permission to use this command.",
                [nameof(LangKeys.error_save_fail)] = "Failed to save base {0} at position {1}: {2}",
                [nameof(LangKeys.error_save_baserecording_invalid)] = "BaseRecording is not valid.",
                [nameof(LangKeys.error_save_no_entities_found)] = "No entities found for saving.",
                [nameof(LangKeys.message_init_recordings)] = "Initialize found {0} building(s) to track",
                [nameof(LangKeys.message_save_begin)] = "Begin saving base {0} at position {1}...",
                [nameof(LangKeys.error_save_file_exists)] = "The save file at '{0}' already exists.",
                [nameof(LangKeys.error_recording_locked)] = "The base is currently locked for processing. Please try again later.",
                [nameof(LangKeys.error_must_face_target)] = "Must be facing building and be within {0} meters.",
                [nameof(LangKeys.error_building_priv_missing)] = "Entity is not part of building privilege.",
                [nameof(LangKeys.error_building_null)] = "Could not find building (check returned null).",
                [nameof(LangKeys.error_command_args_length)] = "Expected {0} argument(s), but got {1} arguments.",
                [nameof(LangKeys.error_command_arg_parse_fail)] = "The argument at index '{0}' could not be parsed as '{1}', value: '{2}'.",
                [nameof(LangKeys.error_recording_not_found)] = "Could not find recording with id '{0}'.",
                [nameof(LangKeys.error_backup_failed)] = "Backup command for base {0} failed, reason unknown.",
                [nameof(LangKeys.error_backup_failed_exception)] = "Backup command for base {0} failed, reason: {1}",
                [nameof(LangKeys.error_load_config_failed)] = "Failed to load config: {0}, creating from default.",
                [nameof(LangKeys.message_backup_success)] = "Backup command for base {0} completed in: {1}",
                [nameof(LangKeys.menu_title)] = "Auto Build Snapshot",
                [nameof(LangKeys.menu_close)] = "Close",
                [nameof(LangKeys.menu_options)] = "Options",
                [nameof(LangKeys.menu_options_title)] = "Menu Options",
                [nameof(LangKeys.menu_options_theme)] = "Theme",
                [nameof(LangKeys.menu_options_mode)] = "Mode",
                [nameof(LangKeys.menu_options_contrast)] = "Contrast",
                [nameof(LangKeys.menu_options_header_fonttype)] = "Header Font Type",
                [nameof(LangKeys.menu_options_header_fontsize)] = "Header Font Size",
                [nameof(LangKeys.menu_options_body_fonttype)] = "Body Font Type",
                [nameof(LangKeys.menu_options_body_fontsize)] = "Body Font Size",
                [nameof(LangKeys.menu_options_background)] = "Background",
                [nameof(LangKeys.menu_tab_home)] = "Home",
                [nameof(LangKeys.menu_tab_logs)] = "Logs",
                [nameof(LangKeys.menu_content_back)] = "Back",
                [nameof(LangKeys.menu_content_clear)] = "Clear",
                [nameof(LangKeys.menu_logs_empty)] = "The logs are empty.",
            }, plugin, "en");
        }
    }

    private enum LangKeys
    {
        /// <summary>
        /// You do not have permission to use this command.
        /// </summary>
        error_no_permission,

        /// <summary>
        /// Failed to save base {0} at position {1}: {2}
        /// </summary>
        error_save_fail,

        /// <summary>
        /// BaseRecording is not valid.
        /// </summary>
        error_save_baserecording_invalid,

        /// <summary>
        /// No entities found for saving.
        /// </summary>
        error_save_no_entities_found,

        /// <summary>
        /// The save file at '{0}' already exists.
        /// </summary>
        error_save_file_exists,

        /// <summary>
        /// Must be facing building and be within {0} meters.
        /// </summary>
        error_must_face_target,

        /// <summary>
        /// Entity is not part of building privilege.
        /// </summary>
        error_building_priv_missing,

        /// <summary>
        /// Could not find building (check returned null).
        /// </summary>
        error_building_null,

        /// <summary>
        /// The base is currently locked for processing. Please try again later.
        /// </summary>
        error_recording_locked,

        /// <summary>
        /// Expected {0} argument(s), but got {1} arguments.
        /// </summary>
        error_command_args_length,

        /// <summary>
        /// The argument at index '{0}' could not be parsed as '{1}', value: '{2}'
        /// </summary>
        error_command_arg_parse_fail,

        /// <summary>
        /// Could not find recording with id '{0}'.
        /// </summary>
        error_recording_not_found,

        /// <summary>
        /// Backup command for base {0} failed, reason unknown.
        /// </summary>
        error_backup_failed,

        /// <summary>
        /// Backup command for base {0} failed, reason: {1}
        /// </summary>
        error_backup_failed_exception,

        /// <summary>
        /// Failed to load config: {0}, creating from default.
        /// </summary>
        error_load_config_failed,

        /// <summary>
        /// Backup command for base {0} completed in: {1}
        /// </summary>
        message_backup_success,

        /// <summary>
        /// Initialize found {0} building(s) to track
        /// </summary>
        message_init_recordings,

        /// <summary>
        /// Begin saving base {0} at position {1}...
        /// </summary>
        message_save_begin,

        /// <summary>
        /// Auto Build Snapshot
        /// </summary>
        menu_title,

        /// <summary>
        /// Close
        /// </summary>
        menu_close,

        /// <summary>
        /// Options
        /// </summary>
        menu_options,

        /// <summary>
        /// Menu Options
        /// </summary>
        menu_options_title,

        /// <summary>
        /// Theme
        /// </summary>
        menu_options_theme,

        /// <summary>
        /// Mode
        /// </summary>
        menu_options_mode,

        /// <summary>
        /// Contrast
        /// </summary>
        menu_options_contrast,

        /// <summary>
        /// Background Transparency
        /// </summary>
        menu_options_background,

        /// <summary>
        /// Header Font Type
        /// </summary>
        menu_options_header_fonttype,

        /// <summary>
        /// Header Font Size
        /// </summary>
        menu_options_header_fontsize,

        /// <summary>
        /// Body Font Type
        /// </summary>
        menu_options_body_fonttype,

        /// <summary>
        /// Body Font Size
        /// </summary>
        menu_options_body_fontsize,

        /// <summary>
        /// Home
        /// </summary>
        menu_tab_home,

        /// <summary>
        /// Logs
        /// </summary>
        menu_tab_logs,

        /// <summary>
        /// Back
        /// </summary>
        menu_content_back,

        /// <summary>
        /// Clear
        /// </summary>
        menu_content_clear,

        /// <summary>
        /// The logs are empty.
        /// </summary>
        menu_logs_empty,
    }

    /// <summary>
    /// Represents a localized exception.
    /// </summary>
    private class LocalizedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedException"/> class with the specified language key and player.
        /// </summary>
        /// <param name="langKey">The language key for the exception message.</param>
        /// <param name="player">The player to send the message to (optional).</param>
        /// <param name="args">The arguments to format the message with.</param>
        public LocalizedException(LangKeys langKey, BasePlayer player = null, params object[] args)
            : base(Localizer.Text(langKey, player, args))
        {
            LangKey = langKey;
        }

        /// <summary>
        /// Gets the language key used for the localized exception message.
        /// </summary>
        public LangKeys LangKey { get; }
    }

    #endregion

    #region Settings

    #region Config Overrides

    /// <summary>
    /// Loads the configuration.
    /// </summary>
    protected override void LoadConfig()
    {
        base.LoadConfig();

        Settings.Init(this);
    }

    /// <summary>
    /// Loads the default configuration.
    /// </summary>
    protected override void LoadDefaultConfig()
    {
        base.LoadDefaultConfig();

        Settings.InitDefault(this);
    }

    /// <summary>
    /// Saves the configuration.
    /// </summary>
    protected override void SaveConfig()
    {
        base.SaveConfig();

        Settings.Save(this);
    }

    #endregion

    /// <summary>
    /// Handles the AutoBuildSnapshot settings and defaults.
    /// </summary>
    /// <summary>
    /// Handles the AutoBuildSnapshot settings and defaults.
    /// </summary>
    private static class Settings
    {
        private static AutoBuildSnapshotConfig _config;

        /// <inheritdoc cref="AutoBuildSnapshotConfig.GeneralSettings"/>
        public static AutoBuildSnapshotConfig.GeneralSettings General => _config.General;

        /// <inheritdoc cref="AutoBuildSnapshotConfig.MultiTCSettings"/>
        public static AutoBuildSnapshotConfig.MultiTCSettings MultiTC => _config.MultiTC;

        /// <inheritdoc cref="AutoBuildSnapshotConfig.AdvancedSettings"/>
        public static AutoBuildSnapshotConfig.AdvancedSettings Advanced => _config.Advanced;

        /// <inheritdoc cref="AutoBuildSnapshotConfig.Commands"/>
        public static AutoBuildSnapshotConfig.CommandSettingCollection Commands => _config.Commands;

        /// <summary>
        /// Initializes the settings by reading the configuration from the file or creating a default one if it fails.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        public static void Init(AutoBuildSnapshot plugin)
        {
            _config = ReadConfigOrCreateDefault(plugin);

            plugin.SaveConfig();
        }

        /// <summary>
        /// Initializes the settings with default values and saves them to the file.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        public static void InitDefault(AutoBuildSnapshot plugin)
        {
            _config = CreateDefault();

            plugin.SaveConfig();
        }

        /// <summary>
        /// Reads the configuration from the file or creates a default one if it fails.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        /// <returns>The configuration object.</returns>
        private static AutoBuildSnapshotConfig ReadConfigOrCreateDefault(AutoBuildSnapshot plugin)
        {
            try
            {
                return plugin.Config.ReadObject<AutoBuildSnapshotConfig>()
                    ?? throw new Exception("Config is null");
            }
            catch (Exception ex)
            {
                Helpers.Log(LangKeys.error_load_config_failed, null, ex.Message);

                return CreateDefault();
            }
        }

        /// <summary>
        /// Creates a default <see cref="AutoBuildSnapshotConfig"/> instance.
        /// </summary>
        /// <returns>The default instance.</returns>
        private static AutoBuildSnapshotConfig CreateDefault()
        {
            return new();
        }

        /// <summary>
        /// Saves the configuration to the file.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        public static void Save(AutoBuildSnapshot plugin)
        {
            plugin.Config.WriteObject(_config, true);

            plugin.AddCovalenceCommand(Commands.ToggleMenu.Alias, nameof(CommandToggleMenu), Commands.ToggleMenu.Permission);
            plugin.AddCovalenceCommand(Commands.Backup.Alias, nameof(CommandBackup), Commands.Backup.Permission);
            plugin.AddCovalenceCommand(Commands.Rollback.Alias, nameof(CommandRollback), Commands.Rollback.Permission);
        }
    }

    /// <summary>
    /// The default settings for the AutoBuildSnapshot plugin.
    /// </summary>
    private static class SettingDefaults
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

            /// <inheritdoc cref="AutoBuildSnapshotConfig.AdvancedSettings.DataSaveFormat"/>
            public const DataFormat DataSaveFormat = DataFormat.GZip;
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

        /// <summary>
        /// The alias and permission settings for each of the commands.
        /// </summary>
        [JsonProperty("Command Settings")]
        public CommandSettingCollection Commands { get; set; } = new();

        /// <summary>
        /// Fine tuned settings for performance.
        /// </summary>
        [JsonProperty("Advanced Settings (Caution: Changes may cause lag)")]
        public AdvancedSettings Advanced { get; init; } = new();

        /// <inheritdoc cref="General"/>
        public class GeneralSettings
        {
            /// <summary>
            /// The interval, in seconds, to check if snapshots are needed.
            /// </summary>
            [JsonProperty("Update Check Interval (seconds)")]
            public float BuildMonitorInterval { get; set; } = SettingDefaults.General.BuildMonitorInterval;

            /// <summary>
            /// The minimum delay between snapshots.
            /// </summary>
            [JsonProperty("Delay Between Snapshots (seconds)")]
            public float DelayBetweenSaves { get; set; } = SettingDefaults.General.DelayBetweenSaves;

            /// <summary>
            /// The number of hours to keep snapshots before they are deleted.
            /// </summary>
            [JsonProperty("Snapshot Retention Period (hours)")]
            public int SnapshotRetentionPeriodHours { get; set; } = SettingDefaults.General.SnapshotRetentionPeriodHours;

            /// <summary>
            /// Whether to include ground resources in the backup that are not player-owned.
            /// </summary>
            [JsonProperty("Include Ground Resources")]
            public bool IncludeGroundResources { get; set; } = SettingDefaults.General.IncludeGroundResources;

            /// <summary>
            /// Whether to include non-authorized deployables in the backup.
            /// </summary>
            [JsonProperty("Include Non-Authorized Deployables")]
            public bool IncludeNonAuthorizedDeployables { get; set; } = SettingDefaults.General.IncludeNonAuthorizedDeployables;
        }

        /// <inheritdoc cref="MultiTC"/>
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

            /// <inheritdoc cref="MultiTCMode"/>
            [JsonProperty("Multi-TC Snapshots Mode")]
            public MultiTCMode Mode { get; set; } = SettingDefaults.MultiTC.Mode;

            /// <summary>
            /// The radius to scan for other TCs when trying to link multiple TCs together.
            /// </summary>
            [JsonProperty("Multi-TC Scan Radius (Automatic Mode)")]
            public float ScanRadius { get; set; } = SettingDefaults.MultiTC.ScanRadius;
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
            public string AdminPermission { get; set; } = SettingDefaults.Commands.AdminPermission;

            /// <summary>
            /// Settings for the toggle menu command.
            /// </summary>
            [JsonProperty("Toggle Menu")]
            public CommandSetting ToggleMenu { get; set; } = new(SettingDefaults.Commands.ToggleMenu);

            /// <summary>
            /// Settings for the manual backup command.
            /// </summary>
            [JsonProperty("Backup (manual)")]
            public CommandSetting Backup { get; set; } = new(SettingDefaults.Commands.Backup);

            /// <summary>
            /// Settings for the rollback command.
            /// </summary>
            [JsonProperty("Rollback")]
            public CommandSetting Rollback { get; set; } = new(SettingDefaults.Commands.Rollback);

            /// <summary>
            /// Checks if the player has the admin permission, allowing them to run all commands.
            /// </summary>
            /// <param name="player">The player to check.</param>
            /// <returns>True if the player has permission, false otherwise.</returns>
            public bool HasAdminPermission(BasePlayer player)
            {
                return _instance.permission.UserHasPermission(player.UserIDString, AdminPermission);
            }
        }

        /// <summary>
        /// Represents a command setting.
        /// </summary>
        public class CommandSetting
        {
            public CommandSetting() { }

            /// <summary>
            /// Creates a new command setting with the specified name and permission.
            /// </summary>
            /// <param name="name"></param>
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

            /// <summary>
            /// Checks if the player has permission to run the command.
            /// </summary>
            /// <param name="player">The player to check.</param>
            /// <returns>True if the player has permission, false otherwise.</returns>
            public bool HasPermission(BasePlayer player)
            {
                return _instance.permission.UserHasPermission(player.UserIDString, Permission)
                    || Settings.Commands.HasAdminPermission(player);
            }
        }

        /// <inheritdoc cref="Advanced"/>
        public class AdvancedSettings
        {
            /// <summary>
            /// The default radius is something close to 37, but we use 40 to catch a broader range.
            /// We also want to allow users to change in-case they have plugins or something using a different radius.
            /// </summary>
            [JsonProperty("Foundation Privilege Radius")]
            public float FoundationPrivilegeRadius { get; set; } = SettingDefaults.Advanced.FoundationPrivRadius;

            /// <summary>
            /// The maximum number of retries to save a snapshot before giving up.
            /// </summary>
            [JsonProperty("Max Retry Attempts on Failure")]
            public int MaxSaveFailRetries { get; set; } = SettingDefaults.Advanced.MaxSaveFailRetries;

            /// <summary>
            /// List of data formats that can be used to save the snapshot data.
            /// </summary>
            [JsonProperty("Data Save FormatLegend")]
            public Dictionary<string, DataFormat> DataSaveFormatLegend => _dataFormatLegend;
            private static readonly Dictionary<string, DataFormat> _dataFormatLegend = new()
            {
                ["Binary"] = DataFormat.Binary,
                ["Binary (GZip Compressed)"] = DataFormat.GZip,
            };

            /// <inheritdoc cref="DataFormat"/>
            [JsonProperty("Data Save Format")]
            public DataFormat DataSaveFormat { get; set; } = SettingDefaults.Advanced.DataSaveFormat;
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
        /// <summary>
        /// Multi-TC mode is disabled.
        /// </summary>
        Disabled = 0,
        // Manual = 1,
        // Automatic = 2
    }

    /// <summary>
    /// Represents the format of the data when saving the snapshot.
    /// </summary>
    private enum DataFormat
    {
        /// <summary>
        /// Binary format.
        /// </summary>
        Binary = 0,
        /// <summary>
        /// GZip compressed binary format.
        /// </summary>
        GZip = 1,
    }

    #endregion
}
