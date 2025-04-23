using Carbon.Components;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // Simplification warning ignore
#pragma warning disable IDE1006 // Naming Styles

[Info("AutoBuildSnapshot", "hizenxyz", "0.0.19")]
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
    private static Dictionary<ulong, BuildRecord> _buildRecords;

    /// <summary>
    /// The list of temporary entities that are created for visual feedback (e.g. spheres).
    /// </summary>
    private static Dictionary<ulong, List<Components.ClientEntity>> _tempEntities;

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
            [JsonProperty("Multi-TC Snapshots Enabled")]
            public bool Enabled { get; set; } = _defaultTryLinkMultiTCBuildings;

            /// <summary>
            /// The radius to scan for other TCs when trying to link multiple TCs together.
            /// </summary>
            [JsonProperty("Multi-TC Scan Radius")]
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
            /// The type to use when saving the snapshot data.
            /// </summary>
            [JsonProperty("Data Save Format")]
            public DataFormat DataSaveFormat { get; set; } = DataFormat.GZip;
        }
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
            public string error_target_no_record => "Unable to find any backup records in the target area.";

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
        /// Unable to find any backup records in the target area.
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

        if (!_config.Commands.UserHasPermission(player, _config.Commands.ToggleMenu, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return;
        }

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
        if (!_config.Commands.UserHasPermission(player, _config.Commands.Backup, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return;
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
        if (!_config.Commands.UserHasPermission(player, _config.Commands.Rollback, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return;
        }
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
        _tempEntities = Pool.Get<Dictionary<ulong, List<Components.ClientEntity>>>();
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

        var snapshotMetaDataFiles = Pool.Get<List<string>>();
        foreach (var directory in Directory.GetDirectories(snapshotDirectory))
        {
            snapshotMetaDataFiles.AddRange(Directory.GetFiles(directory, $"*.{_snapshotMetaExtension}"));
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
            var metaFile = Interface.Oxide.DataFileSystem.GetDatafile(Path.Combine(snapshotDirectory, fileName));
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

    #region Helpers

    /// <summary>
    /// Generates an id for the entity based on prefab and location that should persist between server restarts.
    /// </summary>
    /// <param name="entity">The entity to generate the id for.</param>
    /// <returns>The generated id.</returns>
    public static string GetPersistanceID<T>(T entity)
        where T : BaseEntity =>
        GetPersistanceID(entity.GetType(), entity.ServerPosition);

    /// <summary>
    /// Generates an id for the entity based on prefab and location that should persist between server restarts.
    /// </summary>
    /// <param name="type">The type of the entity.</param>
    /// <param name="position">The position of the entity.</param>
    /// <returns>The generated id.</returns>
    public static string GetPersistanceID(Type type, Vector3 position) =>
        GetPersistanceID(type.Name, position.x, position.y, position.z);

    /// <summary>
    /// Generates an id for the entity based on prefab and location that should persist between server restarts.
    /// </summary>
    /// <param name="typeName">The type name of the entity.</param>
    /// <param name="position">The position of the entity.</param>
    /// <returns>The generated id.</returns>
    public static string GetPersistanceID(string typeName, Vector3 position) =>
        GetPersistanceID(typeName, position);

    /// <summary>
    /// Generates an id for the entity based on prefab and location that should persist between server restarts.
    /// </summary>
    /// <param name="typeName">The type name of the entity.</param>
    /// <param name="x">The x coordinate of the entity.</param>
    /// <param name="y">The y coordinate of the entity.</param>
    /// <param name="z">The z coordinate of the entity.</param>
    /// <returns>The generated id.</returns>
    public static string GetPersistanceID(string typeName, float x, float y, float z) =>
        $"{typeName}({x:F2},{y:F2},{z:F2})";

    /// <summary>
    /// Checks if the entity has a valid id and isn't destroyed.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    private static bool ValidEntity(BaseNetworkable entity) =>
        entity != null && entity.net != null && entity.net.ID.IsValid && !entity.IsDestroyed;

    /// <summary>
    /// Creates a temporary, client-side entity for the player.
    /// </summary>
    /// <param name="player">The player to create the entity for.</param>
    /// <param name="prefabName">The prefab name of the entity.</param>
    /// <param name="position">The position of the entity.</param>
    /// <param name="entityBuilder">An optional action to modify the entity.</param>
    private static void CreateTempEntity(BasePlayer player, string prefabName, Vector3 position, Quaternion rotation = default, Action<ClientEntity> entityBuilder = null)
    {
        var entity = ClientEntity.Create(prefabName, position, rotation);
        entityBuilder?.Invoke(entity);

        entity.SpawnFor(player.net.connection);

        if (!_tempEntities.TryGetValue(player.userID, out var playerEntities))
        {
            playerEntities = Pool.Get<List<ClientEntity>>();
            _tempEntities[player.userID] = playerEntities;
        }

        playerEntities.Add(entity);
    }

    /// <summary>
    /// Kills all temporary client entities.
    /// </summary>
    /// <param name="playerID">Optionally, the player ID to kill entities for. Kills all temps if not specified.</param>
    private static void KillTempEntities(ulong playerID = 0)
    {
        if (_tempEntities == null || _tempEntities.Values.Count == 0)
            return;

        if (playerID == 0)
        {
            foreach (var entities in _tempEntities.Values)
            {
                KillEntities(entities);
            }
        }
        else if (_tempEntities.TryGetValue(playerID, out var entities))
        {
            KillEntities(entities);
        }
    }

    /// <summary>
    /// Kills the specified entities.
    /// </summary>
    /// <param name="entities">The entities to kill.</param>
    private static void KillEntities(List<Components.ClientEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (entity == null)
                continue;

            entity.KillAll();
            entity.Dispose();
        }

        entities.Clear();
    }

    /// <summary>
    /// Checks if the player has any of the specified permissions.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="permissions">The permissions to check for.</param>
    /// <returns>True if the player has any of the specified permissions, false otherwise.</returns>
    private bool UserHasAnyPermission(BasePlayer player, params string[] permissions) =>
        permissions.Any(perm => permission.UserHasPermission(player.UserIDString, perm));

    /// <summary>
    /// Frees the unmanaged resources used by the dictionary list.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to free.</param>
    private static void FreeDictionaryList<TKey, TValue>(ref Dictionary<TKey, List<TValue>> dict)
    {
        var keys = Pool.Get<List<TKey>>();
        keys.AddRange(dict.Keys);

        foreach (var key in keys)
        {
            var index = dict[key];
            Pool.FreeUnmanaged(ref index);
            dict[key] = null;
        }

        Pool.FreeUnmanaged(ref keys);
        Pool.FreeUnmanaged(ref dict);
    }

    /// <summary>
    /// Tries to get the entity that the player is looking at.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="target">The target entity.</param>
    /// <returns>True if the player is looking at an entity, false otherwise.</returns>
    private bool TryGetPlayerTargetEntity(BasePlayer player, out BaseEntity target, float maxDistance = _defaultMaxTargetDistance)
    {
        // Only scan within 10 blocks.

        Ray ray = new(player.eyes.position, player.eyes.HeadForward());
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, _maskBaseEntities))
        {
            target = hit.GetEntity();
            return target;
        }

        target = null;
        return false;
    }

    /// <summary>
    /// Tries to get the coordinates of the target the player is looking at.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="position">The target coordinates.</param>
    /// <returns>True if the target coordinates were found, false otherwise.</returns>
    private bool TryGetPlayerTargetCoordinates(BasePlayer player, out Vector3 position, float maxDistance = _defaultMaxTargetDistance)
    {
        // Start from the player's eye position
        Vector3 eyePosition = player.eyes.position;
        Vector3 eyeDirection = player.eyes.HeadForward();

        // Create a ray pointing forward and slightly downward
        Vector3 direction = new Vector3(eyeDirection.x, -0.3f, eyeDirection.z).normalized;
        Ray ray = new(eyePosition, direction);

        // Cast the ray and get the hit point
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, _maskDefault | _maskGround))
        {
            position = hit.point;
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Tries to get the snapshot IDs at the specified position.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <param name="snapshotIds">The list to store the snapshot IDs.</param>
    /// <returns>True if snapshot IDs were found, false otherwise.</returns>
    private bool TryGetSnapshotIdsAtPosition(Vector3 position, List<System.Guid> snapshotIds)
    {
        var zones = _zoneSnapshotIndex
            .Where(idx => ZoneContains(idx.Key, position))
            .SelectMany(idx => idx.Value)
            .Distinct();

        snapshotIds.AddRange(zones);

        return snapshotIds.Count > 0;
    }

    /// <summary>
    /// Checks if the zone contains the specified coordinate.
    /// </summary>
    /// <param name="zone">The zone to check.</param>
    /// <param name="coordinate">The coordinate to check.</param>
    /// <returns>True if the zone contains the coordinate, false otherwise.</returns>
    private bool ZoneContains(Vector4 zone, Vector3 coordinate) =>
        (coordinate - (Vector3)zone).sqrMagnitude <= zone.w * zone.w;

    /// <summary>
    /// Gets the snapshot state for the specified snapshot ID.
    /// </summary>
    /// <param name="snapshotId">The snapshot ID to check.</param>
    /// <returns>The snapshot state.</returns>
    private SnapshotState GetSnapshotState(BasePlayer player, System.Guid snapshotId)
    {
        if (_snapshotHandles.TryGetValue(snapshotId, out var handle))
        {
            if (handle.PlayerUserID != player.userID)
            {
                return SnapshotState.Locked;
            }

            return handle.State;
        }

        return SnapshotState.Idle;
    }

    /// <summary>
    /// Formats the relative time for display.
    /// </summary>
    /// <param name="timeSpan">The time span to format.</param>
    /// <returns>The formatted relative time string.</returns>
    private string FormatRelativeTime(System.TimeSpan timeSpan)
    {
        List<string> parts = Pool.Get<List<string>>();

        // Add days if they exist
        if (timeSpan.Days > 0)
            parts.Add($"{timeSpan.Days} day{(timeSpan.Days != 1 ? "s" : "")}");

        // Add hours if they exist
        if (timeSpan.Hours > 0)
            parts.Add($"{timeSpan.Hours} hour{(timeSpan.Hours != 1 ? "s" : "")}");

        // Add minutes if they exist
        if (timeSpan.Minutes > 0)
            parts.Add($"{timeSpan.Minutes} minute{(timeSpan.Minutes != 1 ? "s" : "")}");

        // Add seconds if they exist (or if no other time units exist)
        if (timeSpan.Seconds > 0 || parts.Count == 0)
            parts.Add($"{timeSpan.Seconds} second{(timeSpan.Seconds != 1 ? "s" : "")}");

        // Join with commas
        var result = string.Join(", ", parts);

        Pool.FreeUnmanaged(ref parts);

        return result;
    }

    #endregion
}
