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

#pragma warning disable IDE1006 // Naming Styles
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
                [nameof(LangKeys.message_backup_success)] = "Backup command for base {0} completed in: {1}",
                [nameof(LangKeys.menu_title)] = "Auto Build Snapshot",
                [nameof(LangKeys.menu_close)] = "Close",
                [nameof(LangKeys.menu_options)] = "Options",
                [nameof(LangKeys.menu_options_title)] = "Menu Options",
                [nameof(LangKeys.menu_options_mode)] = "Mode",
                [nameof(LangKeys.menu_options_contrast)] = "Contrast",
                [nameof(LangKeys.menu_options_fontsize)] = "Font Size",
                [nameof(LangKeys.menu_options_fonttype)] = "Font Type",
                [nameof(LangKeys.menu_options_background)] = "Background",
            },
            plugin, "en");
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
        /// Font Size
        /// </summary>
        menu_options_fontsize,

        /// <summary>
        /// Font Type
        /// </summary>
        menu_options_fonttype
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
                Helpers.Log($"Failed to load config: {ex.Message}, creating from default.");

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

    #region User Preference (ui settings)

    private static class UserPreference
    {
        private static string _userPreferenceDir;
        private static Dictionary<ulong, UserPreferenceData> _userPreferences;

        public static void Init()
        {
            _userPreferenceDir = Path.Combine(Interface.Oxide.DataDirectory, "abs_userprefs");

            if (!Directory.Exists(_userPreferenceDir))
            {
                Directory.CreateDirectory(_userPreferenceDir);
            }

            _userPreferences = Pool.Get<Dictionary<ulong, UserPreferenceData>>();
        }

        public static void Unload()
        {
            Pool.FreeUnmanaged(ref _userPreferences);
        }

        public static UserPreferenceData For(BasePlayer player)
        {
            if (_userPreferences.TryGetValue(player.userID, out var data))
            {
                return data;
            }

            var userPreferenceFile = GetUserPreferenceFile(player);
            if (File.Exists(userPreferenceFile))
            {
                try
                {
                    var json = File.ReadAllText(userPreferenceFile);
                    data = JsonConvert.DeserializeObject<UserPreferenceData>(json);
                    _userPreferences[player.userID] = data;

                    if (data == null)
                    {
                        throw new Exception("Failed to deserialize user preference data.");
                    }
                }
                catch (Exception ex)
                {
                    Helpers.Log($"Failed to load user preference data for {player.displayName}: {ex.Message}");
                }
            }

            data ??= CreateDefault(player);

            return data;
        }

        /// <summary>
        /// Creates a default user preference data object for the specified player.
        /// </summary>
        /// <param name="player">The player to create the preference data for.</param>
        /// <returns>The default user preference data object.</returns>
        private static UserPreferenceData CreateDefault(BasePlayer player)
        {
            var data = new UserPreferenceData();
            Save(player, data);
            return data;
        }

        /// <summary>
        /// Saves the user preference data for the specified player.
        /// </summary>
        /// <param name="player">The player to save the preference data for.</param>
        /// <param name="data">The user preference data to save.</param>
        private static void Save(BasePlayer player, UserPreferenceData data)
        {
            var filePath = GetUserPreferenceFile(player);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);

            _userPreferences[player.userID] = data;
        }

        /// <summary>
        /// Gets the path to the user preference file for the specified player.
        /// </summary>
        /// <param name="player">The player to get the preference file for.</param>
        /// <returns>The path to the user preference file.</returns>
        private static string GetUserPreferenceFile(BasePlayer player)
        {
            var filename = $"{player.UserIDString}.json";
            return Path.Combine(_userPreferenceDir, filename);
        }

        /// <summary>
        /// Updates the user preference data for the specified player.
        /// </summary>
        /// <param name="player">The player to update the preference data for.</param>
        /// <param name="modal">The modal containing the updated settings data.</param>
        public static void Update(BasePlayer player, ModalModule.Modal modal)
        {
            var colorPalette = modal.Get<ColorPaletteOptions>(nameof(UserPreferenceData.ColorPalette));
            var contrast = modal.Get<ContrastOptions>(nameof(UserPreferenceData.ContrastOption));
            var fontSize = modal.Get<FontSizeOptions>(nameof(UserPreferenceData.FontSize));
            var fontType = modal.Get<FontTypeOptions>(nameof(UserPreferenceData.FontType));
            var background = modal.Get<BackgroundOptions>(nameof(UserPreferenceData.BackgroundOption));

            var userPreference = For(player);
            userPreference.ColorPaletteOption = colorPalette;
            userPreference.ContrastOption = contrast;
            userPreference.FontSizeOption = fontSize;
            userPreference.FontTypeOption = fontType;
            userPreference.BackgroundOption = background;

            Save(player, userPreference);
        }
    }

    private class UserPreferenceData
    {
        /// <summary>
        /// The color palette used for the UI.
        /// </summary>
        [JsonIgnore]
        public ColorPalette ColorPalette { get; private set; }

        /// <summary>
        /// The color palette option selected by the user.
        /// </summary>
        public ColorPaletteOptions ColorPaletteOption
        {
            get => _colorPaletteOption;
            set => SetColorPalette(value, ContrastOption);
        }
        private ColorPaletteOptions _colorPaletteOption;

        /// <summary>
        /// The contrast option selected by the user.
        /// </summary>
        public ContrastOptions ContrastOption
        {
            get => _contrastOption;
            set => SetColorPalette(ColorPaletteOption, value);
        }
        private ContrastOptions _contrastOption;

        /// <summary>
        /// The font size used for the UI.
        /// </summary>
        [JsonIgnore]
        public FontSize FontSize { get; private set; }

        /// <summary>
        /// The font size option selected by the user.
        /// </summary>
        public FontSizeOptions FontSizeOption
        {
            get => _fontSizeOption;
            set => SetFontSize(value);
        }
        private FontSizeOptions _fontSizeOption;

        /// <summary>
        /// The font type used for the UI.
        /// </summary>
        [JsonIgnore]
        public FontType FontType { get; private set; }

        /// <summary>
        /// The font type option selected by the user.
        /// </summary>
        public FontTypeOptions FontTypeOption
        {
            get => _fontTypeOption;
            set => SetFontType(value);
        }
        private FontTypeOptions _fontTypeOption;

        /// <summary>
        /// The background option selected by the user.
        /// </summary>
        public BackgroundOptions BackgroundOption { get; set; }

        /// <summary>
        /// Default constructor for the user preference data.
        /// </summary>
        public UserPreferenceData() : this(
            ColorPaletteOptions.Dark, 
            ContrastOptions.Medium, 
            FontSizeOptions.Medium, 
            FontTypeOptions.Roboto,
            BackgroundOptions.Blur)
        {
        }

        /// <summary>
        /// Constructor for the user preference data with specified options.
        /// </summary>
        /// <param name="colorPalette">The color palette option.</param>
        /// <param name="contrast">The contrast option.</param>
        /// <param name="fontSize">The font size option.</param>
        /// <param name="fontType">The font type option.</param>
        /// <param name="background">The background option.</param>
        public UserPreferenceData(
            ColorPaletteOptions colorPalette, 
            ContrastOptions contrast, 
            FontSizeOptions fontSize, 
            FontTypeOptions fontType,
            BackgroundOptions background)
        {
            ColorPaletteOption = colorPalette;
            ContrastOption = contrast;
            FontSizeOption = fontSize;
            FontTypeOption = fontType;
            BackgroundOption = background;
        }

        /// <summary>
        /// Sets the color palette based on the selected options.
        /// </summary>
        /// <param name="colorOption">The color palette option.</param>
        /// <param name="contrastOption">The contrast option.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid option is provided.</exception>
        private void SetColorPalette(ColorPaletteOptions colorOption, ContrastOptions contrastOption)
        {
            if (colorOption == ColorPaletteOption && contrastOption == ContrastOption)
            {
                return;
            }

            ColorPalette = colorOption switch
            {
                ColorPaletteOptions.Light => contrastOption switch
                {
                    ContrastOptions.Low => ColorPalettes.Light.LowContrast,
                    ContrastOptions.Medium => ColorPalettes.Light.MediumContrast,
                    ContrastOptions.High => ColorPalettes.Light.HighContrast,
                    _ => throw new ArgumentOutOfRangeException(nameof(contrastOption), contrastOption, null)
                },
                ColorPaletteOptions.Dark => contrastOption switch
                {
                    ContrastOptions.Low => ColorPalettes.Dark.LowContrast,
                    ContrastOptions.Medium => ColorPalettes.Dark.MediumContrast,
                    ContrastOptions.High => ColorPalettes.Dark.HighContrast,
                    _ => throw new ArgumentOutOfRangeException(nameof(contrastOption), contrastOption, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(colorOption), colorOption, null)
            };
            _colorPaletteOption = colorOption;
            _contrastOption = contrastOption;
        }

        /// <summary>
        /// Sets the font size based on the selected option.
        /// </summary>
        /// <param name="option">The font size option.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid option is provided.</exception>
        private void SetFontSize(FontSizeOptions option)
        {
            FontSize = option switch
            {
                FontSizeOptions.Small => FontSizes.Small,
                FontSizeOptions.Medium => FontSizes.Medium,
                FontSizeOptions.Large => FontSizes.Large,
                _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
            };
            _fontSizeOption = option;
        }

        /// <summary>
        /// Sets the font type based on the selected option.
        /// </summary>
        /// <param name="option">The font type option.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid option is provided.</exception>
        private void SetFontType(FontTypeOptions option)
        {
            FontType = option switch
            {
                FontTypeOptions.Roboto => FontTypes.Roboto,
                FontTypeOptions.RobotoBold => FontTypes.RobotoBold,
                FontTypeOptions.DroidSans => FontTypes.DroidSans,
                FontTypeOptions.NotoSans => FontTypes.NotoSans,
                FontTypeOptions.PermanentMarker => FontTypes.PermanentMarker,
                _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
            };
            _fontTypeOption = option;
        }
    }

    public enum ColorPaletteOptions
    {
        Light,
        Dark,
    }

    public enum ContrastOptions
    {
        Low,
        Medium,
        High
    }

    public static class ColorPalettes
    {
        public static string[] ColorOptions { get; } = Enum
            .GetValues(typeof(ColorPaletteOptions))
            .Cast<ColorPaletteOptions>()
            .Select(option => option.ToString())
            .ToArray();

        public static string[] ContrastOptions { get; } = Enum
            .GetValues(typeof(ContrastOptions))
            .Cast<ContrastOptions>()
            .Select(option => option.ToString())
            .ToArray();

        public static class Light
        {
            public static readonly ColorPalette LowContrast = new()
            {
                Primary = "0.850 0.400 0.300 1.0",
                OnPrimary = "1.0 0.998 0.998 1.0",
                Secondary = "0.612 0.400 0.365 1.0",
                OnSecondary = "1.0 1.0 1.0 1.0",
                Tertiary = "0.620 0.671 0.286 1.0",
                OnTertiary = "0.0 0.0 0.0 1.0",
                BackgroundBase = "1.0 1.0 1.0",
                OnBackground = "0.0 0.0 0.0 0.87",
                Surface = "1.0 1.0 1.0",
                OnSurface = "0.0 0.0 0.0 0.87",
                Button = "0.850 0.400 0.300 1.0",
                OnButton = "1.0 1.0 1.0 1.0",
                HighlightItem = "0.950 0.900 0.875 1.0",
                OnHighlightItem = "0.0 0.0 0.0 1.0",
                Watermark = "0.0 0.0 0.0 0.4",
            };
            public static readonly ColorPalette MediumContrast = new()
            {
                Primary = "0.800 0.300 0.200 1.0",
                OnPrimary = "1.0 1.0 1.0 1.0",
                Secondary = "0.612 0.400 0.365 1.0",
                OnSecondary = "1.0 1.0 1.0 1.0",
                Tertiary = "0.620 0.671 0.286 1.0",
                OnTertiary = "0.0 0.0 0.0 1.0",
                BackgroundBase = "0.960 0.960 0.960",
                OnBackground = "0.0 0.0 0.0 1.0",
                Surface = "0.980 0.980 0.980",
                OnSurface = "0.0 0.0 0.0 1.0",
                Button = "0.800 0.300 0.200 1.0",
                OnButton = "1.0 1.0 1.0 1.0",
                HighlightItem = "0.900 0.700 0.600 1.0",
                OnHighlightItem = "0.0 0.0 0.0 1.0",
                Watermark = "0.0 0.0 0.0 0.4",
            };
            public static readonly ColorPalette HighContrast = new()
            {
                Primary = "0.600 0.100 0.000 1.0",
                OnPrimary = "1.0 1.0 1.0 1.0",
                Secondary = "0.612 0.400 0.365 1.0",
                OnSecondary = "1.0 1.0 1.0 1.0",
                Tertiary = "0.620 0.671 0.286 1.0",
                OnTertiary = "0.0 0.0 0.0 1.0",
                BackgroundBase = "0.920 0.920 0.920",
                OnBackground = "0.0 0.0 0.0 1.0",
                Surface = "0.960 0.960 0.960",
                OnSurface = "0.0 0.0 0.0 1.0",
                Button = "0.600 0.100 0.000 1.0",
                OnButton = "1.0 1.0 1.0 1.0",
                HighlightItem = "1.000 0.800 0.700 1.0",
                OnHighlightItem = "0.0 0.0 0.0 1.0",
                Watermark = "0.0 0.0 0.0 0.4",
            };
        }
        public static class Dark
        {
            public static readonly ColorPalette LowContrast = new()
            {
                Primary = "0.950 0.600 0.500 1.0",
                OnPrimary = "0.0 0.0 0.0 1.0",
                Secondary = "0.765 0.639 0.616 1.0",
                OnSecondary = "0.0 0.0 0.0 1.0",
                Tertiary = "0.769 0.800 0.569 1.0",
                OnTertiary = "0.0 0.0 0.0 1.0",
                BackgroundBase = "0.071 0.071 0.071",
                OnBackground = "1.0 1.0 1.0 0.87",
                Surface = "0.071 0.071 0.071",
                OnSurface = "1.0 1.0 1.0 0.87",
                Button = "0.950 0.600 0.500 1.0",
                OnButton = "0.0 0.0 0.0 1.0",
                HighlightItem = "0.500 0.200 0.150 1.0",
                OnHighlightItem = "1.0 1.0 1.0 1.0",
                Watermark = "1.0 1.0 1.0 0.1",
            };
            public static readonly ColorPalette MediumContrast = new()
            {
                Primary = "0.900 0.400 0.300 1.0",
                OnPrimary = "0.0 0.0 0.0 1.0",
                Secondary = "0.765 0.639 0.616 1.0",
                OnSecondary = "0.0 0.0 0.0 1.0",
                Tertiary = "0.769 0.800 0.569 1.0",
                OnTertiary = "0.0 0.0 0.0 1.0",
                BackgroundBase = "0.050 0.050 0.050",
                OnBackground = "1.0 1.0 1.0 1.0",
                Surface = "0.090 0.090 0.090",
                OnSurface = "1.0 1.0 1.0 1.0",
                Button = "0.900 0.400 0.300 1.0",
                OnButton = "0.0 0.0 0.0 1.0",
                HighlightItem = "0.600 0.250 0.200 1.0",
                OnHighlightItem = "1.0 1.0 1.0 1.0",
                Watermark = "1.0 1.0 1.0 0.1",
            };
            public static readonly ColorPalette HighContrast = new()
            {
                Primary = "1.000 0.200 0.000 1.0",
                OnPrimary = "0.0 0.0 0.0 1.0",
                Secondary = "0.765 0.639 0.616 1.0",
                OnSecondary = "0.0 0.0 0.0 1.0",
                Tertiary = "0.769 0.800 0.569 1.0",
                OnTertiary = "0.0 0.0 0.0 1.0",
                BackgroundBase = "0.000 0.000 0.000",
                OnBackground = "1.0 1.0 1.0 1.0",
                Surface = "0.050 0.050 0.050",
                OnSurface = "1.0 1.0 1.0 1.0",
                Button = "1.000 0.200 0.000 1.0",
                OnButton = "0.0 0.0 0.0 1.0",
                HighlightItem = "0.700 0.300 0.250 1.0",
                OnHighlightItem = "1.0 1.0 1.0 1.0",
                Watermark = "1.0 1.0 1.0 0.1",
            };
        }
    }

    public class ColorPalette
    {
        public required string Primary { get; init; }
        public required string OnPrimary { get; init; }
        public required string Secondary { get; init; }
        public required string OnSecondary { get; init; }
        public required string Tertiary { get; init; }
        public required string OnTertiary { get; init; }
        public required string BackgroundBase { get; init; }
        public required string OnBackground { get; init; }
        public required string Surface { get; init; }
        public required string OnSurface { get; init; }
        public required string Button { get; init; }
        public required string OnButton { get; init; }
        public required string HighlightItem { get; init; }
        public required string OnHighlightItem { get; init; }
        public required string Watermark { get; init; }
        public string Blur => "0 0 0 0.2";
        public string Transparent => "0 0 0 0.01";
        public string Outline => "0.0 0.0 0.0 1.0";
    }

    public enum FontSizeOptions
    {
        Small,
        Medium,
        Large
    }

    private static class FontSizes
    {
        public static string[] Options { get; } = Enum
            .GetValues(typeof(FontSizeOptions))
            .Cast<FontSizeOptions>()
            .Select(option => option.ToString())
            .ToArray();

        public static readonly FontSize Small = new()
        {
            Header = 14,
            Body = 12,
        };

        public static readonly FontSize Medium = new()
        {
            Header = 18,
            Body = 16,
        };

        public static readonly FontSize Large = new()
        {
            Header = 22,
            Body = 20,
        };
    }

    private class FontSize
    {
        public required int Header { get; init; }
        public required int Body { get; init; }
    }

    public enum FontTypeOptions
    {
        Roboto,
        RobotoBold,
        DroidSans,
        NotoSans,
        PermanentMarker,
    }

    private static class FontTypes
    {
        public static string[] Options { get; } = Enum
            .GetValues(typeof(FontTypeOptions))
            .Cast<FontTypeOptions>()
            .Select(option => option.ToString())
            .ToArray();

        public static readonly FontType Roboto = new()
        {
            Header = CUI.Handler.FontTypes.RobotoCondensedRegular,
            Body = CUI.Handler.FontTypes.RobotoCondensedRegular,
        };

        public static readonly FontType RobotoBold = new()
        {
            Header = CUI.Handler.FontTypes.RobotoCondensedBold,
            Body = CUI.Handler.FontTypes.RobotoCondensedBold,
        };

        public static readonly FontType DroidSans = new()
        {
            Header = CUI.Handler.FontTypes.DroidSansMono,
            Body = CUI.Handler.FontTypes.DroidSansMono,
        };

        public static readonly FontType NotoSans = new()
        {
            Header = CUI.Handler.FontTypes.NotoSansArabicBold,
            Body = CUI.Handler.FontTypes.NotoSansArabicBold,
        };

        public static readonly FontType PermanentMarker = new()
        {
            Header = CUI.Handler.FontTypes.PermanentMarker,
            Body = CUI.Handler.FontTypes.PermanentMarker,
        };
    }

    private class FontType
    {
        public required CUI.Handler.FontTypes Header { get; init; }
        public required CUI.Handler.FontTypes Body { get; init; }
    }

    private enum BackgroundOptions
    {
        Solid,
        Translucent,
        Blur,
    }

    private static class Backgrounds
    {
        public static string[] Options { get; } = Enum
            .GetValues(typeof(BackgroundOptions))
            .Cast<BackgroundOptions>()
            .Select(option => option.ToString())
            .ToArray();
    }

    #endregion
}
