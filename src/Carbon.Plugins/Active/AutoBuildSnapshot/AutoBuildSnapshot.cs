using Carbon.Components;
using Cysharp.Threading.Tasks;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // Simplification warning ignore
#pragma warning disable IDE1006 // Naming Styles

[Info("AutoBuildSnapshot", "hizenxyz", "0.0.20")]
[Description("Automatically backs up a player's base when they build to it, allowing it to be restored later.")]
public partial class AutoBuildSnapshot : CarbonPlugin
{
    #region Fields

    private static AutoBuildSnapshot _instance;

    /// <summary>
    /// The default distance to scan for targets when running player commands.
    /// </summary>
    private const float _defaultMaxTargetDistance = 60;

    /// <summary>
    /// The maximum duration, in seconds, that a user can hold a snapshot handle.
    /// </summary>
    private const float _maxSnapshotHandleDuration = 120f;

    #region Debug Constants

    private const string prefabSphere = "assets/prefabs/visualization/sphere.prefab";
    private const string prefabSphereRed = "assets/bundled/prefabs/modding/events/twitch/br_sphere_red.prefab";
    private const string prefabSphereGreen = "assets/bundled/prefabs/modding/events/twitch/br_sphere_green.prefab";
    private const string prefabSpherePurple = "assets/bundled/prefabs/modding/events/twitch/br_sphere_purple.prefab";

    #endregion

    #region Game Constants

    // Base entity masks -- established on Init
    private static int _maskDefault;
    private static int _maskGround;
    private static int _maskDeployed;
    private static int _maskConstruction;
    private static int _maskVehicle;
    private static int _maskHarvestable;
    private static int _maskBaseEntities;

    // Prevention masks -- established on Init
    private static int _maskPreventBuilding;
    private static int _maskPreventMovement;
    private static int _maskPreventActions;

    #endregion

    #region Plugin Constants

    #region Snapshot Files

    private const string _snapshotDataDirectory = "abs/v1";
    private const string _snapshotDataExtension = "data";
    private const string _snapshotMetaExtension = "meta";

    #endregion

    #region User Interface

    private const string _mainMenuId = "abs.menu.main";
    private const string _snapshotMenuId = "abs.menu.snapshot";
    private const string _confirmationDialogId = "abs.menu.confirm";

    #endregion

    #endregion

    #region Settings

    private static AutoBuildSnapshotConfig _config;

    private const int _defaultBuildMonitorInterval = 600;
    private const int _defaultDelayBetweenSaves = 3600;
    private const int _defaultSnapshotRetentionHours = 720;
    private const bool _defaultBackupIncludeGroundResources = true;
    private const bool _defaultIncludeNonAuthorizedDeployables = true;

    private const bool _defaultTryLinkMultiTCBuildings = true;
    private const float _defaultMultiTCScanRadius = 80;

    private const string _defaultPermissionAdmin = "abs.admin";
    private const string _defaultCommandShowMenu = "abs.menu";
    private const string _defaultCommandCreateBackup = "abs.backup";
    private const string _defaultCommandRollback = "abs.rollback";

    private const float _defaultFoundationPrivRadius = 40;
    private const int _defaultMaxSaveFailRetries = 3;
    private const int _defaultMaxStepFrameDuration = 50;
    private const float _defaultMaxZoneScanRadius = 100;

    #endregion

    #region Resources

    /// <summary>
    /// The timer that monitors for build changes and updates states, triggers saves, etc.
    /// </summary>
    private Timer _buildMonitor;

    /// <summary>
    /// The list of all buildings being recorded/tracked by the plugin.
    /// </summary>
    private Dictionary<ulong, BuildRecord> _buildRecords;

    /// <summary>
    /// The list of manually linked records, by PersistantID.
    /// </summary>
    private Dictionary<string, HashSet<string>> _manualLinks;

    /// <summary>
    /// The list of temporary entities that are created for visual feedback (e.g. spheres).
    /// </summary>
    private static Dictionary<ulong, List<BaseEntity>> _tempEntities;

    /// <summary>
    /// The set of log messages.
    /// </summary>
    private List<string> _logMessages;

    /// <summary>
    /// The connected players, indexed by userId.
    /// </summary>
    private Dictionary<ulong, BasePlayer> _connectedPlayers;

    #region Snapshot Data

    /// <summary>
    /// A set of all the snapshots that have been created.
    /// </summary>
    private Dictionary<Guid, BuildSnapshotMetaData> _snapshotMetaData;

    /// <summary>
    /// Handles for the snapshots that are currently being processed.
    /// </summary>
    private static Dictionary<Guid, SnapshotHandle> _snapshotHandles;

    /// <summary>
    /// The currently held handles by player.
    /// </summary>
    private static Dictionary<ulong, Guid> _playerSnapshotHandles;

    /// <summary>
    /// Index of snapshot ids by persistant building IDs for faster lookup.
    /// </summary>
    private Dictionary<string, List<System.Guid>> _buildingIDToSnapshotIndex;

    /// <summary>
    /// Dictionary of zones and the snapshot ids recorded in them.
    /// </summary>
    private Dictionary<Vector4, List<System.Guid>> _zoneSnapshotIndex;

    #endregion

    #region User Interface

    /// <summary>
    /// Menu state, by player.
    /// </summary>
    private Dictionary<ulong, MenuLayer> _playerMenuStates;

    /// <summary>
    /// Previous menu state, by player.
    /// </summary>
    private Dictionary<ulong, MenuLayer> _previousMenuState;

    /// <summary>
    /// Current menu tab selection, by player.
    /// </summary>
    private Dictionary<ulong, MenuTab> _currentMenuTab;

    /// <summary>
    /// Current selected build record, by player.
    /// </summary>
    private Dictionary<ulong, ulong> _currentBuildRecord;

    /// <summary>
    /// Current selected snapshot, by player.
    /// </summary>
    private Dictionary<ulong, Guid> _currentSelectedSnapshot;

    /// <summary>
    /// Current snapshot view scroll index, by player.
    /// </summary>
    private Dictionary<ulong, int> _snapshotScrollIndex;

    /// <summary>
    /// Current build record view scroll index, by player.
    /// </summary>
    private Dictionary<ulong, int> _playerRecordScrollIndex;

    #endregion

    #endregion

    #endregion

    #region Configuration

    /// <summary>
    /// Loads the configuration.
    /// </summary>
    protected override void LoadConfig()
    {
        base.LoadConfig();

        try
        {
            _config = Config.ReadObject<AutoBuildSnapshotConfig>();

            if (_config == null)
            {
                AddLogMessage($"Failed to load config, creating from default.");
                LoadDefaultConfig();
            }
            else
            {
                SaveConfig();
            }
        }
        catch
        {
            AddLogMessage($"Failed to load config, creating from default.");
            LoadDefaultConfig();
        }
    }

    /// <summary>
    /// Loads the default configuration.
    /// </summary>
    protected override void LoadDefaultConfig()
    {
        base.LoadDefaultConfig();

        _config = new();

        SaveConfig();
    }

    /// <summary>
    /// Saves the configuration.
    /// </summary>
    protected override void SaveConfig()
    {
        base.SaveConfig();

        Config.WriteObject(_config, true);
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
            public float BuildMonitorInterval { get; set; } = _defaultBuildMonitorInterval;

            /// <summary>
            /// The minimum delay between snapshots.
            /// </summary>
            [JsonProperty("Delay Between Snapshots (seconds)")]
            public float DelayBetweenSaves { get; set; } = _defaultDelayBetweenSaves;

            /// <summary>
            /// The number of hours to keep snapshots before they are deleted.
            /// </summary>
            [JsonProperty("Snapshot Retention Period (hours)")]
            public int SnapshotRetentionPeriodHours { get; set; } = _defaultSnapshotRetentionHours;

            /// <summary>
            /// Whether to include ground resources in the backup that are not player-owned.
            /// </summary>
            [JsonProperty("Include Ground Resources")]
            public bool IncludeGroundResources { get; set; } = _defaultBackupIncludeGroundResources;

            /// <summary>
            /// Whether to include non-authorized deployables in the backup.
            /// </summary>
            [JsonProperty("Include Non-Authorized Deployables")]
            public bool IncludeNonAuthorizedDeployables { get; set; } = _defaultIncludeNonAuthorizedDeployables;
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
                ["Disabled"] = MultiTCMode.Disabled,
                // ["Manual"] = MultiTCMode.Manual,
                // ["Automatic (Experimental)"] = MultiTCMode.Automatic
            };

            /// <summary>
            /// Whether to try to link multiple TCs together when they are in the same area.
            /// </summary>
            [JsonProperty("Multi-TC Snapshots Mode")]
            public MultiTCMode Mode { get; set; } = MultiTCMode.Disabled;

            /// <summary>
            /// The radius to scan for other TCs when trying to link multiple TCs together.
            /// </summary>
            [JsonProperty("Multi-TC Scan Radius (Automatic Mode)")]
            public float ScanRadius { get; set; } = _defaultMultiTCScanRadius;
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
            public string AdminPermission { get; set; } = _defaultPermissionAdmin;

            /// <summary>
            /// Settings for the toggle menu command.
            /// </summary>
            [JsonProperty("Toggle Menu")]
            public CommandSetting ToggleMenu { get; set; } = new(_defaultCommandShowMenu);

            /// <summary>
            /// Settings for the manual backup command.
            /// </summary>
            [JsonProperty("Backup (manual)")]
            public CommandSetting Backup { get; set; } = new(_defaultCommandCreateBackup);

            /// <summary>
            /// Settings for the rollback command.
            /// </summary>
            [JsonProperty("Rollback")]
            public CommandSetting Rollback { get; set; } = new(_defaultCommandRollback);

            /// <summary>
            /// Checks if the user has permission to run the command.
            /// </summary>
            /// <param name="player">The player to check.</param>
            /// <param name="command">The command to check.</param>
            /// <param name="plugin">The plugin instance.</param>
            /// <returns>True if the user has permission, false otherwise.</returns>
            public bool UserHasPermission(BasePlayer player, CommandSetting command, AutoBuildSnapshot plugin) =>
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
            public float FoundationPrivilegeRadius { get; set; } = _defaultFoundationPrivRadius;

            /// <summary>
            /// The maximum number of retries to save a snapshot before giving up.
            /// </summary>
            [JsonProperty("Max Retry Attempts on Failure")]
            public int MaxSaveFailRetries { get; set; } = _defaultMaxSaveFailRetries;

            /// <summary>
            /// The maximum duration, in milliseconds, that we can process a step 
            /// before we should try to release processing into the next frame.
            /// </summary>
            [JsonProperty("Max Frame Step Duration (ms)")]
            public int MaxStepFrameDuration { get; set; } = _defaultMaxStepFrameDuration;

            /// <summary>
            /// The maximum radius to create zones before splitting them up.
            /// This is used when scanning entities for snapshots.
            /// </summary>
            [JsonProperty("Max Zone Scan Radius")]
            public float MaxScanZoneRadius { get; set; } = _defaultMaxZoneScanRadius;

            /// <summary>
            /// List of data formats that can be used to save the snapshot data.
            /// </summary>
            [JsonProperty("Data Save FormatLegend")]
            public Dictionary<string, DataFormat> DataSaveFormatLegend => _dataFormatLegend;
            private static readonly Dictionary<string, DataFormat> _dataFormatLegend = new()
            {
                ["Binary"] = DataFormat.Binary,
                ["Binary (GZip Compressed)"] = DataFormat.GZip,
                ["Json"] = DataFormat.Json,
                ["Json (Expanded)"] = DataFormat.JsonExpanded
            };

            /// <summary>
            /// The type to use when saving the snapshot data.
            /// </summary>
            [JsonProperty("Data Save Format")]
            public DataFormat DataSaveFormat { get; set; } = DataFormat.GZip;
        }
    }

    private enum MultiTCMode
    {
        Disabled = 0,
        Manual = 1,
        Automatic = 2
    }

    private enum DataFormat
    {
        Binary = 0,
        GZip = 1,
        Json = 2,
        JsonExpanded = 3
    }

    #endregion

    #region Localization

    private static AutoBuildSnapshotLang _lang;

    /// <summary>
    /// Loads the default localization messages for the plugin.
    /// </summary>
    protected override void LoadDefaultMessages()
    {
        base.LoadDefaultMessages();

        _lang = new(this);
    }

    /// <summary>
    /// Handles localization for the plugin.
    /// </summary>
    private class AutoBuildSnapshotLang
    {
        private readonly AutoBuildSnapshot _abs;

        /// <summary>
        /// Creates a new instance of the AutoBuildSnapshotLang class.
        /// </summary>
        /// <param name="abs">The AutoBuildSnapshot instance.</param>
        public AutoBuildSnapshotLang(AutoBuildSnapshot abs)
        {
            _abs = abs;

            RegisterMessages();
        }

        /// <summary>
        /// Gets the localized message for the specified key.
        /// </summary>
        /// <param name="key">The language key.</param>
        /// <param name="player">The player to get the message for.</param>
        /// <returns>The localized message for the specified key.</returns>
        public string GetMessage(LangKeys key, BasePlayer player) =>
            _abs.lang.GetMessage(key.ToString(), _abs, player.UserIDString);

        /// <summary>
        /// Register all the language packs.
        /// </summary>
        private void RegisterMessages()
        {
            var langSets = new IAutoBuildSnapshotLangSet[]
            {
                new AutoBuildSnapshotLangSet_EN()
            };

            foreach (var set in langSets)
            {
                _abs.lang.RegisterMessages(new()
                {
                    [nameof(LangKeys.error_no_permission)] = set.error_no_permission,
                    [nameof(LangKeys.error_invalid_target)] = set.error_invalid_target,

                    [nameof(LangKeys.ui_main_title)] = set.ui_main_title,
                },
                _abs,
                set.LangCode);
            }
        }

        /// <summary>
        /// Template for all language packs.
        /// </summary>
        private interface IAutoBuildSnapshotLangSet
        {
            string LangCode { get; }

            // Errors
            string error_no_permission { get; }
            string error_invalid_target { get; }
            string error_target_no_backups { get; }
            string error_target_no_record { get; }

            // User Interface
            string ui_main_title { get; }
        }

        /// <summary>
        /// English language pack.
        /// </summary>
        private class AutoBuildSnapshotLangSet_EN : IAutoBuildSnapshotLangSet
        {
            public string LangCode => "en";

            public string error_no_permission => "You do not have permission to use this command.";
            public string error_invalid_target => "You must be looking at a target object or ground.";
            public string error_target_no_backups => "Unable to find any backups in the target area.";
            public string error_target_no_record => "Unable to find any tracked bases in the target area.";

            public string ui_main_title => "Auto Build Snapshot";

        }
    }

    private enum LangKeys
    {
        /// <summary>
        /// You do not have permission to use this command.
        /// </summary>
        error_no_permission,

        /// <summary>
        /// You must be looking at a target object or ground.
        /// </summary>
        error_invalid_target,

        /// <summary>
        /// Unable to find any backups in the target area.
        /// </summary>
        error_target_no_backups,

        /// <summary>
        /// Unable to find any tracked bases in the target area.
        /// </summary>
        error_target_no_record,

        /// <summary>
        /// Auto Build Snapshot
        /// </summary>
        ui_main_title,
    }

    #endregion

    #region Hooks

    #region Plugin Load/Unload

    /// <summary>
    /// Called when the plugin is loaded.
    /// </summary>
    void Init()
    {
        _instance = this;

        InitResources();
        InitBuildMonitor(_config.General.BuildMonitorInterval);
        InitMasks();

        RegisterPermissions();
        RegisterCommands();
    }

    /// <summary>
    /// Called when the plugin is unloaded.
    /// </summary>
    void Unload()
    {
        FreeResources();

        _instance = null;
    }

    #endregion

    #region Entity tracking

    /// <summary>
    /// Called when an entity is spawned (during startup or player placement).
    /// </summary>
    /// <param name="entity">The entity that was spawned.</param>
    void OnEntitySpawned(BaseNetworkable entity)
    {
        if (entity is BuildingPrivlidge tc)
        {
            StartRecording(tc);
        }
    }

    /// <summary>
    /// Called when an entity is killed (destroyed).
    /// </summary>
    /// <param name="networkable">The entity that was killed.</param>
    void OnEntityKill(BaseNetworkable networkable)
    {
        if (networkable is BuildingPrivlidge tc)
        {
            if (_buildRecords.TryGetValue(tc.net.ID.Value, out var record))
            {
                // Release record back to pool for reuse
                Pool.Free(ref record);

                // Remove from tracking once freed
                _buildRecords.Remove(tc.net.ID.Value);
            }
        }

        /*
        // Leave here in case we want to do this in the future, but for now, I think we ignore
        // Otherwise we probably will end up with a lot more data than we want to track
        // In most cases, the most recent valid change record will be deploys and storage updates
        if (networkable is BaseEntity entity)
        {
            // get tc for this entity
            tc = entity.GetBuildingPrivilege();

            // if tc exists and we're tracking it, create a change record so it can be flagged for processing
            if (tc && _buildRecords.TryGetValue(tc.net.ID.Value, out var record))
            {
                record.AddChange(null, ChangeAction.Destroy, entity);
            }
        }
        */
    }

    /// <summary>
    /// Called whenever a player "tabs out" of the inventory.
    /// I think this is the "least called" entity modifier by a player so we can use it to track changes.
    /// </summary>
    /// <param name="player">The player that looted the entity.</param>
    /// <param name="entity">The entity that was looted.</param>
    void OnLootEntityEnd(BasePlayer player, BaseEntity entity) =>
        ProcessChange(entity, ChangeAction.Update, player);

    /// <summary>
    /// Called when some construction is placed on the ground by the player.
    /// We are only using this to track changes in the BuildRecords.
    /// </summary>
    /// <param name="entity">The entity that was placed.</param>
    /// <param name="component">The construction component.</param>
    /// <param name="constructionTarget">The target of the construction.</param>
    /// <param name="player">The player that placed the entity.</param>
    /// <returns>Returns null so that we don't change original hook behavior.</returns>
    object OnConstructionPlace(BaseEntity entity, Construction component, Construction.Target constructionTarget, BasePlayer player)
    {
        // building priv tracked through OnEntitySpawned
        if (entity is null or BuildingPrivlidge)
        {
            return null;
        }

        ProcessChange(entity, ChangeAction.Update, player);

        return null;
    }

    /// <summary>
    /// Called when a building part decays and spawns debris (collapsed structure debris entity created).
    /// </summary>
    /// <param name="entity">The entity that was decayed.</param>
    void OnDebrisSpawn(DecayEntity entity) =>
        ProcessChange(entity, ChangeAction.Decay);

    /// <summary>
    /// Process the potential change action.
    /// </summary>
    /// <param name="entity">The entity that was changed.</param>
    /// <param name="action">The action that was performed.</param>
    /// <param name="player">The player that performed the action.</param>
    private void ProcessChange(BaseEntity entity, ChangeAction action, BasePlayer player = null)
    {
        // get tc for this entity
        var tc = entity.GetBuildingPrivilege();

        // Just check if it has a TC and if we're tracking said TC. That's it.
        if (tc && _buildRecords.TryGetValue(tc.net.ID.Value, out var record))
        {
            record.AddChange(entity, action, player);
        }
    }

    #endregion

    #region Player Connections

    void OnPlayerConnected(BasePlayer player)
    {
        _connectedPlayers[player.userID] = player;
    }

    void OnPlayerDisconnected(BasePlayer player, string reason)
    {
        if (_connectedPlayers.ContainsKey(player.userID))
        {
            _connectedPlayers.Remove(player.userID);
        }
    }

    #endregion

    #endregion

    #region Commands

    /// <summary>
    /// Shows the snapshot menu to the player.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    /// <param name="command">The command.</param>
    /// <param name="args">The arguments.</param>
    private void CommandToggleMenu(BasePlayer player, string command, string[] args)
    {
        if (_playerMenuStates.TryGetValue(player.userID, out var state) && state != MenuLayer.Closed)
        {
            NavigateMenu(player, MenuLayer.Closed);
            return;
        }

        if (!UserHasPermission(player, _config.Commands.ToggleMenu.Permission)) return;

        NavigateMenu(player, MenuLayer.MainMenu);
    }

    /// <summary>
    /// Performs a manual backup of the base the player is currently looking at.
    /// This should bypass any timers and perform the backup immediately.
    /// </summary>
    /// <param name="player">The player performing the backup.</param>
    /// <param name="command">The command.</param>
    /// <param name="args">The arguments.</param>
    private void CommandBackup(BasePlayer player, string command, string[] args)
    {
        if (!UserHasPermission(player, _config.Commands.Backup)) return;

        Vector3 targetCoords;
        if (TryGetPlayerTargetEntity(player, out var target))
        {
            // Found target, get zones from target coords
            targetCoords = target.ServerPosition;
        }
        else if (!TryGetPlayerTargetCoordinates(player, out targetCoords))
        {
            // No target or coordinates within maxDistance found
            player.ChatMessage(_lang.GetMessage(LangKeys.error_invalid_target, player));
            return;
        }

        var updateRecords = Pool.Get<Queue<BuildRecord>>();
        foreach (var record in _buildRecords.Values)
        {
            if (record.LinkedZones.Any(zone => ZoneContains(zone, targetCoords)))
                updateRecords.Enqueue(record);
        }

        if (updateRecords.Count > 0)
        {
            AddLogMessage(player, $"Found {updateRecords.Count} record(s) containing target, creating backup...");

            // Perform backup
            ProcessNextSave(updateRecords, (success, snapshots) =>
            {
                if (success)
                {
                    AddLogMessage(player, $"Backup completed in {snapshots.Sum(x => x.Duration)} ms");
                }
                else
                {
                    AddLogMessage(player, "Failed to create backup, rollback aborted.");
                    SnapshotHandle.Release(player);
                }
            });
        }
        else
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_target_no_record, player));
            Pool.FreeUnmanaged(ref updateRecords);
        }
    }

    /// <summary>
    /// Used to restore a base to a previous state.
    /// Split into two parts: init and confirm.
    /// </summary>
    /// <param name="player">The player performing the backup.</param>
    /// <param name="command">The command.</param>
    /// <param name="args">The arguments (init or confirm)</param>
    private void CommandRollback(BasePlayer player, string command, string[] args)
    {
        if (!UserHasPermission(player, _config.Commands.Rollback)) return;

        Vector3 targetCoords;
        if (TryGetPlayerTargetEntity(player, out var target))
        {
            // Found target, get zones from target coords
            targetCoords = target.ServerPosition;
        }
        else if (!TryGetPlayerTargetCoordinates(player, out targetCoords))
        {
            // No target or coordinates within maxDistance found
            player.ChatMessage(_lang.GetMessage(LangKeys.error_invalid_target, player));
            return;
        }

        var snapshotIds = Pool.Get<List<Guid>>();
        if (!TryGetSnapshotIdsAtPosition(targetCoords, snapshotIds))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_target_no_backups, player));
            return;
        }

        NavigateMenu(player, MenuLayer.Snapshots, false, false, null, snapshotIds);
    }

    /// <summary>
    /// Registers the commands for the plugin using the specified configuration.
    /// </summary>
    private void RegisterCommands()
    {
        AddCovalenceCommand(_config.Commands.ToggleMenu.Alias, nameof(CommandToggleMenu));
        AddCovalenceCommand(_config.Commands.Backup.Alias, nameof(CommandBackup));
        AddCovalenceCommand(_config.Commands.Rollback.Alias, nameof(CommandRollback));
    }

    #endregion

    #region Init Plugin Data

    /// <summary>
    /// Initializes the permissions for the plugin.
    /// </summary>
    private void RegisterPermissions()
    {
        permission.RegisterPermission(_config.Commands.AdminPermission, this);
        permission.RegisterPermission(_config.Commands.ToggleMenu.Permission, this);
        permission.RegisterPermission(_config.Commands.Backup.Permission, this);
        permission.RegisterPermission(_config.Commands.Rollback.Permission, this);
    }

    private void InitMasks()
    {
        _maskDefault = LayerMask.GetMask("Default");
        _maskGround = LayerMask.GetMask("Ground", "Terrain", "World");
        _maskDeployed = LayerMask.GetMask("Deployed");
        _maskConstruction = LayerMask.GetMask("Construction");
        _maskVehicle = LayerMask.GetMask("Vehicle Detailed", "Vehicle World", "Vehicle Large");
        _maskHarvestable = LayerMask.GetMask("Harvestable");
        _maskBaseEntities = _maskDefault | _maskDeployed | _maskConstruction | _maskVehicle | _maskHarvestable;

        _maskPreventBuilding = LayerMask.GetMask("Prevent Building");
        _maskPreventMovement = LayerMask.GetMask("Prevent Movement");
        _maskPreventActions = _maskPreventBuilding | _maskPreventMovement;
    }

    #endregion

    #region Resource Management

    /// <summary>
    /// Initializes the resources needed for the plugin.
    /// </summary>
    private void InitResources()
    {
        InitDrawingResources();

        _buildRecords = Pool.Get<Dictionary<ulong, BuildRecord>>();
        _snapshotMetaData = Pool.Get<Dictionary<Guid, BuildSnapshotMetaData>>();
        _snapshotHandles = Pool.Get<Dictionary<Guid, SnapshotHandle>>();
        _playerSnapshotHandles = Pool.Get<Dictionary<ulong, Guid>>();
        _buildingIDToSnapshotIndex = Pool.Get<Dictionary<string, List<Guid>>>();
        _zoneSnapshotIndex = Pool.Get<Dictionary<Vector4, List<Guid>>>();
        _tempEntities = Pool.Get<Dictionary<ulong, List<BaseEntity>>>();
        _logMessages = Pool.Get<List<string>>();
        _connectedPlayers = Pool.Get<Dictionary<ulong, BasePlayer>>();

        _playerMenuStates = Pool.Get<Dictionary<ulong, MenuLayer>>();
        _previousMenuState = Pool.Get<Dictionary<ulong, MenuLayer>>();
        _currentMenuTab = Pool.Get<Dictionary<ulong, MenuTab>>();
        _currentBuildRecord = Pool.Get<Dictionary<ulong, ulong>>();
        _currentSelectedSnapshot = Pool.Get<Dictionary<ulong, Guid>>();
        _snapshotScrollIndex = Pool.Get<Dictionary<ulong, int>>();
        _playerRecordScrollIndex = Pool.Get<Dictionary<ulong, int>>();

        var buildings = BaseNetworkable.serverEntities.OfType<BuildingPrivlidge>();
        AddLogMessage($"Initialize found {buildings.Count()} building(s) to track");

        foreach (var tc in buildings)
        {
            StartRecording(tc);
        }

        foreach (var player in BasePlayer.activePlayerList)
        {
            if (player)
            {
                _connectedPlayers[player.userID] = player;
            }
        }

        LoadSnapshotMetaData();
    }

    /// <summary>
    /// Frees the resources used by the plugin.
    /// </summary>
    private void FreeResources()
    {
        FreeDrawingResources();

        KillTempEntities();

        Pool.Free(ref _buildRecords, true);
        Pool.FreeUnmanaged(ref _snapshotMetaData);
        Pool.Free(ref _snapshotHandles, true);
        Pool.FreeUnmanaged(ref _playerSnapshotHandles);
        FreeDictionaryList(ref _buildingIDToSnapshotIndex);
        FreeDictionaryList(ref _zoneSnapshotIndex);
        FreeDictionaryList(ref _tempEntities);
        Pool.FreeUnmanaged(ref _logMessages);
        Pool.FreeUnmanaged(ref _connectedPlayers);

        Pool.FreeUnmanaged(ref _playerMenuStates);
        Pool.FreeUnmanaged(ref _previousMenuState);
        Pool.FreeUnmanaged(ref _currentMenuTab);
        Pool.FreeUnmanaged(ref _currentBuildRecord);
        Pool.FreeUnmanaged(ref _currentSelectedSnapshot);
        Pool.FreeUnmanaged(ref _snapshotScrollIndex);
        Pool.FreeUnmanaged(ref _playerRecordScrollIndex);
    }

    /// <summary>
    /// Loads the metadata for the snapshots.
    /// </summary>
    private void LoadSnapshotMetaData()
    {
        var snapshotDirectory = Path.Combine(Interface.Oxide.DataFileSystem.Directory, _snapshotDataDirectory);
        if (!Directory.Exists(snapshotDirectory))
        {
            AddLogMessage("Did not find any snapshots (data directory does not exist).");
            return;
        }

        using var snapshotMetaDataFiles = Pool.Get<PooledList<string>>();
        foreach (var directory in Directory.GetDirectories(snapshotDirectory))
        {
            snapshotMetaDataFiles.AddRange(Directory.GetFiles(directory, $"*.{_snapshotMetaExtension}.json"));
        }

        if (snapshotMetaDataFiles.Count == 0)
        {
            AddLogMessage("Did not find any snapshots (directory was empty or had no matching meta data files).");
            return;
        }

        AddLogMessage($"Loading {snapshotMetaDataFiles.Count} snapshot(s)");

        var retentionDate = DateTime.UtcNow.AddHours(-_config.General.SnapshotRetentionPeriodHours);
        int deletions = 0;
        foreach (var filePath in snapshotMetaDataFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var directory = Path.GetDirectoryName(filePath);
            var metaFile = Interface.Oxide.DataFileSystem.GetDatafile(Path.Combine(directory, fileName));
            var metaData = metaFile.ReadObject<BuildSnapshotMetaData>();

            // Check for retention and delete
            if (metaData.TimestampUTC < retentionDate)
            {
                metaFile.Delete();

                var dataFile = Interface.Oxide.DataFileSystem.GetDatafile(metaData.DataFile);
                dataFile.Delete();

                deletions++;
                continue;
            }

            SyncSnapshotMetaData(metaData);
        }

        if (deletions > 0)
        {
            AddLogMessage($"Deleted {deletions} snapshot(s) older than {_config.General.SnapshotRetentionPeriodHours} hours");
        }
    }

    /// <summary>
    /// Updates the snapshot metadata with the given metadata.
    /// </summary>
    /// <param name="metaData">The snapshot metadata to update.</param>
    private void SyncSnapshotMetaData(BuildSnapshotMetaData metaData)
    {
        _snapshotMetaData[metaData.ID] = metaData;

        IndexSnapshotMetaData(metaData);
    }

    /// <summary>
    /// Indexes the snapshot metadata.
    /// </summary>
    /// <param name="metaData">The snapshot metadata to index.</param>
    private void IndexSnapshotMetaData(BuildSnapshotMetaData metaData)
    {
        IndexLinkedBuildings(metaData);
        IndexZoneSnapshot(metaData);
    }

    /// <summary>
    /// Indexes the linked buildings in the snapshot metadata.
    /// </summary>
    /// <param name="metaData">The snapshot metadata to index.</param>
    private void IndexLinkedBuildings(BuildSnapshotMetaData metaData)
    {
        foreach (var linkedBuilding in metaData.LinkedBuildings)
        {
            if (!_buildingIDToSnapshotIndex.ContainsKey(linkedBuilding.Key))
            {
                _buildingIDToSnapshotIndex[linkedBuilding.Key] = Pool.Get<List<Guid>>();
            }

            if (!_buildingIDToSnapshotIndex[linkedBuilding.Key].Contains(metaData.ID))
            {
                _buildingIDToSnapshotIndex[linkedBuilding.Key].Add(metaData.ID);
            }
        }
    }

    /// <summary>
    /// Indexes the zones in the snapshot metadata.
    /// </summary>
    /// <param name="metaData">The snapshot metadata to index.</param>
    private void IndexZoneSnapshot(BuildSnapshotMetaData metaData)
    {
        foreach (var zone in metaData.LinkedBuildings.Values.SelectMany(b => b.Zones))
        {
            if (!_zoneSnapshotIndex.ContainsKey(zone))
            {
                _zoneSnapshotIndex[zone] = Pool.Get<List<Guid>>();
            }

            if (!_zoneSnapshotIndex[zone].Contains(metaData.ID))
            {
                _zoneSnapshotIndex[zone].Add(metaData.ID);
            }
        }
    }

    #endregion

    #region Debugging

    [ChatCommand($"debug")]
    private void CommandUIDebug(BasePlayer player, string command, string[] args)
    {
        SomeLongTask(player).Forget();
    }

    private async UniTaskVoid SomeLongTask(BasePlayer player)
    {
        player.ChatMessage("Begin waiting...");
        player.ChatMessage("Wait complete, found entities: " + await GetRandomNumber(player));
    }

    private async UniTask<int> GetRandomNumber(BasePlayer player)
    {
        int count;
        await UniTask.SwitchToThreadPool();

        TryGetPlayerTargetCoordinates(BasePlayer.activePlayerList[0], out var targetCoords);

        var entities = Pool.Get<List<BaseEntity>>();
        Vis.Entities(targetCoords, 100, entities, _maskBaseEntities);
        count = entities.Count;
        Pool.FreeUnmanaged(ref entities);

        Task.Delay(5000).Wait();

        await UniTask.SwitchToMainThread();
        return count;
    }

    #endregion
}
