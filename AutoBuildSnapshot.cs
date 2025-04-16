using Carbon.Components;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using Oxide.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE1006 // Naming Styles

[Info("AutoBuildSnapshot", "hizenxyz", "0.0.16")]
[Description("Automatically backs up a player's base when they build to it, allowing it to be restored later.")]
public partial class AutoBuildSnapshot : CarbonPlugin
{
    #region Fields

    #region Debug Constants

    private const string spherePrefab = "assets/prefabs/visualization/sphere.prefab";
    private const string spherePrefabRed = "assets/bundled/prefabs/modding/events/twitch/br_sphere_red.prefab";
    private const string spherePrefabGreen = "assets/bundled/prefabs/modding/events/twitch/br_sphere_green.prefab";
    private const string spherePrefabPurple = "assets/bundled/prefabs/modding/events/twitch/br_sphere_purple.prefab";

    #endregion

    #region Game Constants

    // Base entity masks -- established on Init
    private static int _maskDefault;
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

    private const string _snapshotDataDirectory = "autobuildsnapshot/v1";
    private const string _snapshotDataExtension = "data";
    private const string _snapshotMetaExtension = "data.info";

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
    private Dictionary<ulong, List<ClientEntity>> _tempEntities;

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
    /// Index of snapshot ids by persistant building IDs for faster lookup.
    /// </summary>
    private Dictionary<string, List<Guid>> _idxSnapshotBuildingID;

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
        }

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
                    [nameof(LangKeys.command_no_permission)] = set.command_no_permission,

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

            // Commands
            string command_no_permission { get; }

            // User Interface
            string ui_main_title { get; }
        }

        /// <summary>
        /// English language pack.
        /// </summary>
        private class AutoBuildSnapshotLangSet_EN : IAutoBuildSnapshotLangSet
        {
            public string LangCode => "en";

            public string command_no_permission => "You do not have permission to use this command.";

            public string ui_main_title => "Auto Build Snapshot";

        }
    }

    private enum LangKeys
    {
        /// <summary>
        /// You do not have permission to use this command.
        /// </summary>
        command_no_permission,

        /// <summary>
        /// Auto Build Snapshot
        /// </summary>
        ui_main_title
    }

    #endregion

    #region Hooks

    #region Plugin Load/Unload

    /// <summary>
    /// Called when the plugin is loaded.
    /// </summary>
    void Init()
    {
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
        if (_playerMenuStates.TryGetValue(player.userID, out var state) && state == MenuLayer.MainMenu)
        {
            NavigateMenu(player, MenuLayer.Closed);
            return;
        }

        if (!_config.Commands.UserHasPermission(player, _config.Commands.ToggleMenu, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.command_no_permission, player));
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
            player.ChatMessage(_lang.GetMessage(LangKeys.command_no_permission, player));
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
            player.ChatMessage(_lang.GetMessage(LangKeys.command_no_permission, player));
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

    #region User Interface

    #region Main Menu

    /// <summary>
    /// Sends the main UI to the player.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    private void CreateMainMenu(CUI cui, BasePlayer player)
    {
        // Create the base container with cursor
        var container = cui.v2
            .CreateParent(
                parent: CUI.ClientPanels.HudMenu,
                position: LuiPosition.Full,
                name: _mainMenuId)
            .AddCursor();

        // Main panel
        var main = cui.v2
            .CreatePanel(
                container: container,
                position: LuiPosition.MiddleCenter,
                offset: new(-450, -275, 450, 275),
                color: "0 0 0 .95"
            );

        // Header
        var header = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, .94f, 1, 1),
                offset: LuiOffset.None,
                color: "0 0 0 .9"
            );

        // Title
        cui.v2.CreateText(
                container: header,
                position: new(.015f, 0.01f, .99f, .95f),
                offset: new(0, 0, 0, 0),
                color: "1 1 1 .8",
                fontSize: 18,
                text: _lang.GetMessage(LangKeys.ui_main_title, player),
                alignment: TextAnchor.MiddleLeft
            )
            .SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Close button
        var closeButton = cui.v2
            .CreateButton(
                container: header,
                position: new(.965f, .15f, .99f, .85f),
                offset: new(4, 0, 4, 0),
                command: $"{nameof(AutoBuildSnapshot)}.menu.close",
                color: ".6 .2 .2 .9"
            );

        cui.v2.CreateImageFromDb(
            container: closeButton,
            position: new(.2f, .2f, .8f, .8f),
            offset: LuiOffset.None,
            dbName: "close",
            color: "1 1 1 .5"
        );

        // Tabs at the top
        var tabsPanel = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, .90f, 1, .94f),
                offset: LuiOffset.None,
                color: "0.2 0.2 0.2 1"
            );

        // Records tab button
        var recordsTabButton = cui.v2
            .CreateButton(
                container: tabsPanel,
                position: new(0, 0, .5f, 1),
                offset: LuiOffset.None,
                command: $"{nameof(AutoBuildSnapshot)}.menu.tab.records",
                color: "0.3 0.3 0.3 1"
            );

        cui.v2.CreateText(
            container: recordsTabButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 .8",
            fontSize: 14,
            text: "Tracked Buildings",
            alignment: TextAnchor.MiddleCenter
        );

        // Logs tab button
        var logsTabButton = cui.v2
            .CreateButton(
                container: tabsPanel,
                position: new(.5f, 0, 1, 1),
                offset: LuiOffset.None,
                command: $"{nameof(AutoBuildSnapshot)}.menu.tab.logs",
                color: "0.2 0.2 0.2 1"
            );

        cui.v2.CreateText(
            container: logsTabButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 .8",
            fontSize: 14,
            text: "Logs",
            alignment: TextAnchor.MiddleCenter
        );

        // Content area
        var contentPanel = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, .05f, 1, .90f),
                offset: LuiOffset.None,
                color: "0.15 0.15 0.15 1"
            );

        if (_currentMenuTab.TryGetValue(player.userID, out var tab))
        {
            switch (tab)
            {
                case MenuTab.Records:
                    ShowRecordsList(player, cui, contentPanel);
                    break;
                case MenuTab.Logs:
                    ShowLogsPanel(player, cui, contentPanel);
                    break;
            }
        }
        else
        {
            // Default to records tab
            _currentMenuTab[player.userID] = MenuTab.Records;
            ShowRecordsList(player, cui, contentPanel);
        }
    }

    // Command handling methods for the UI
    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.menu.tab.records")]
    private void CommandSwitchToRecordsTab(BasePlayer player) =>
        RefreshMenuWithTab(player, MenuTab.Records);

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.menu.tab.logs")]
    private void CommandSwitchToLogsTab(BasePlayer player) =>
        RefreshMenuWithTab(player, MenuTab.Logs);

    #endregion

    #region Main Menu Tabs

    /// <summary>
    /// Shows the records list in the content panel.
    /// </summary>
    /// <param name="player">The player viewing the UI.</param>
    /// <param name="cui">The CUI instance.</param>
    /// <param name="contentPanel">The content panel to fill.</param>
    private void ShowRecordsList(BasePlayer player, CUI cui, LUI.LuiContainer contentPanel)
    {
        // Title
        cui.v2.CreateText(
            container: contentPanel,
            position: new(0, .95f, 1, 1),
            offset: new(10, 0, -10, 0),
            color: "1 1 1 .8",
            fontSize: 14,
            text: "Building Records",
            alignment: TextAnchor.MiddleLeft
        ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Records list area with scrolling
        var recordsScrollContainer = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0, 1, .95f),
                offset: new(10, 10, -10, -5),
                color: "0 0 0 0"
            );

        var recordsList = _buildRecords.Values.ToList();

        if (recordsList.Count == 0)
        {
            // No records message
            cui.v2.CreateText(
                container: recordsScrollContainer,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "1 1 1 .5",
                fontSize: 14,
                text: "There are currently no records being tracked.",
                alignment: TextAnchor.MiddleCenter
            );
        }
        else
        {
            float itemHeight = 0.08f;
            float totalHeight = itemHeight * recordsList.Count;
            float visibleHeight = 1f;
            bool needsScrolling = totalHeight > visibleHeight;

            // Calculate visible records
            int visibleRecordsCount = Mathf.FloorToInt(visibleHeight / itemHeight);
            int topIndex = _playerRecordScrollIndex.TryGetValue(player.userID, out int index) ? index : 0;

            // Clamp the top index
            topIndex = Mathf.Clamp(topIndex, 0, Mathf.Max(0, recordsList.Count - visibleRecordsCount));
            _playerRecordScrollIndex[player.userID] = topIndex;

            // Records list
            for (int i = topIndex; i < Mathf.Min(topIndex + visibleRecordsCount, recordsList.Count); i++)
            {
                var record = recordsList[i];
                float yMin = 1f - (i - topIndex + 1) * itemHeight;
                float yMax = 1f - (i - topIndex) * itemHeight;

                // Record item background (alternate colors)
                var recordItem = cui.v2
                    .CreatePanel(
                        container: recordsScrollContainer,
                        position: new(0, yMin, 1, yMax),
                        offset: LuiOffset.None,
                        color: i % 2 == 0 ? "0.2 0.2 0.2 0.5" : "0.25 0.25 0.25 0.5"
                    );

                // TC position text
                cui.v2.CreateText(
                    container: recordItem,
                    position: new(0, 0, .6f, 1),
                    offset: new(10, 0, 0, 0),
                    color: "1 1 1 .8",
                    fontSize: 12,
                    text: $"TC: {record.BaseTC.ServerPosition:F1} | Status: {record.State}",
                    alignment: TextAnchor.MiddleLeft
                );

                // Teleport button
                var teleportButton = cui.v2
                    .CreateButton(
                        container: recordItem,
                        position: new(.65f, .2f, .8f, .8f),
                        offset: LuiOffset.None,
                        command: $"{nameof(AutoBuildSnapshot)}.teleport {record.NetworkID}",
                        color: "0.3 0.5 0.3 1"
                    );

                cui.v2.CreateText(
                    container: teleportButton,
                    position: LuiPosition.Full,
                    offset: LuiOffset.None,
                    color: "1 1 1 1",
                    fontSize: 12,
                    text: "Teleport",
                    alignment: TextAnchor.MiddleCenter
                );

                // Snapshots button
                var snapshotsButton = cui.v2
                    .CreateButton(
                        container: recordItem,
                        position: new(.82f, .2f, .97f, .8f),
                        offset: LuiOffset.None,
                        command: $"{nameof(AutoBuildSnapshot)}.snapshots {record.NetworkID}",
                        color: "0.3 0.3 0.5 1"
                    );

                cui.v2.CreateText(
                    container: snapshotsButton,
                    position: LuiPosition.Full,
                    offset: LuiOffset.None,
                    color: "1 1 1 1",
                    fontSize: 12,
                    text: "Snapshots",
                    alignment: TextAnchor.MiddleCenter
                );
            }

            // Add scrolling controls if needed
            if (needsScrolling)
            {
                // Scroll up button
                if (topIndex > 0)
                {
                    var scrollUpButton = cui.v2
                        .CreateButton(
                            container: contentPanel,
                            position: new(.96f, .9f, .99f, .95f),
                            offset: LuiOffset.None,
                            command: $"{nameof(AutoBuildSnapshot)}.scroll.records -1",
                            color: "0.3 0.3 0.3 1"
                        );

                    cui.v2.CreateText(
                        container: scrollUpButton,
                        position: LuiPosition.Full,
                        offset: LuiOffset.None,
                        color: "1 1 1 1",
                        fontSize: 14,
                        text: "▲",
                        alignment: TextAnchor.MiddleCenter
                    );
                }

                // Scroll down button
                if (topIndex < recordsList.Count - visibleRecordsCount)
                {
                    var scrollDownButton = cui.v2
                        .CreateButton(
                            container: contentPanel,
                            position: new(.96f, .05f, .99f, .1f),
                            offset: LuiOffset.None,
                            command: $"{nameof(AutoBuildSnapshot)}.scroll.records 1",
                            color: "0.3 0.3 0.3 1"
                        );

                    cui.v2.CreateText(
                        container: scrollDownButton,
                        position: LuiPosition.Full,
                        offset: LuiOffset.None,
                        color: "1 1 1 1",
                        fontSize: 14,
                        text: "▼",
                        alignment: TextAnchor.MiddleCenter
                    );
                }
            }
        }
    }

    /// <summary>
    /// Shows the logs panel in the content area.
    /// </summary>
    /// <param name="player">The player viewing the UI.</param>
    /// <param name="cui">The CUI instance.</param>
    /// <param name="contentPanel">The content panel to fill.</param>
    private void ShowLogsPanel(BasePlayer player, CUI cui, LUI.LuiContainer contentPanel)
    {
        // Title
        cui.v2.CreateText(
            container: contentPanel,
            position: new(0, .95f, 1, 1),
            offset: new(10, 0, -10, 0),
            color: "1 1 1 .8",
            fontSize: 14,
            text: "Plugin Log Messages",
            alignment: TextAnchor.MiddleLeft
        ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Log area
        var logsPanel = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0, 1, .95f),
                offset: new(10, 10, -10, -5),
                color: "0.1 0.1 0.1 0.5"
            );

        // Get logs (This would need to be implemented in your plugin to track logs)
        List<string> logs = GetRecentLogs(20); // Get 20 most recent log entries

        if (logs.Count == 0)
        {
            // No logs message
            cui.v2.CreateText(
                container: logsPanel,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "1 1 1 .5",
                fontSize: 14,
                text: "No log messages available.",
                alignment: TextAnchor.MiddleCenter
            );
        }
        else
        {
            float itemHeight = 0.05f;
            float startY = 1f;

            // Display logs in reverse chronological order (newest at top)
            for (int i = 0; i < logs.Count; i++)
            {
                float yMin = startY - (i + 1) * itemHeight;
                float yMax = startY - i * itemHeight;

                cui.v2.CreateText(
                    container: logsPanel,
                    position: new(0, yMin, 1, yMax),
                    offset: new(5, 0, -5, 0),
                    color: "1 1 1 .7",
                    fontSize: 12,
                    text: logs[i],
                    alignment: TextAnchor.MiddleLeft
                );
            }
        }

        // Clear logs button
        var clearButton = cui.v2
            .CreateButton(
                container: contentPanel,
                position: new(.85f, .95f, .98f, .98f),
                offset: LuiOffset.None,
                command: $"{nameof(AutoBuildSnapshot)}.logs.clear",
                color: "0.5 0.3 0.3 1"
            );

        cui.v2.CreateText(
            container: clearButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 1",
            fontSize: 12,
            text: "Clear Logs",
            alignment: TextAnchor.MiddleCenter
        );
    }


    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.scroll.records")]
    private void CommandScrollRecords(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0) return;

        if (int.TryParse(args[0], out int delta))
        {
            if (!_playerRecordScrollIndex.TryGetValue(player.userID, out int currentIndex))
            {
                _playerRecordScrollIndex[player.userID] = 0;
                currentIndex = 0;
            }

            _playerRecordScrollIndex[player.userID] = Mathf.Max(0, currentIndex + delta);

            NavigateMenu(player, MenuLayer.MainMenu, false, MenuTab.Records);
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.teleport")]
    private void CommandTeleportToRecord(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0) return;

        if (ulong.TryParse(args[0], out ulong recordId) && _buildRecords.TryGetValue(recordId, out var record))
        {
            // Check admin permission
            if (!permission.UserHasPermission(player.UserIDString, _config.Commands.AdminPermission))
            {
                player.ChatMessage(_lang.GetMessage(LangKeys.command_no_permission, player));
                return;
            }

            // Teleport player to the TC position
            player.Teleport(record.BaseTC.ServerPosition + new Vector3(0, 1, 0));
            player.SendNetworkUpdate();

            player.ChatMessage($"Teleported to TC at {record.BaseTC.ServerPosition:F1}");
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots")]
    private void CommandShowSnapshots(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0) return;

        if (ulong.TryParse(args[0], out ulong recordId) && _buildRecords.TryGetValue(recordId, out var record))
        {
            // Switch to snapshots mode
            OpenSnapshotsMenu(player, record);
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.logs.clear")]
    private void CommandClearLogs(BasePlayer player)
    {
        // Check admin permission
        if (!permission.UserHasPermission(player.UserIDString, _config.Commands.AdminPermission))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.command_no_permission, player));
            return;
        }

        ClearLogMessages();

        NavigateMenu(player, MenuLayer.MainMenu, false, MenuTab.Logs);
    }

    /// <summary>
    /// Helper method to refresh the main menu with the specified tab.
    /// </summary>
    /// <param name="player">The player to refresh the menu for.</param>
    /// <param name="tab">The tab to show.</param>
    private void RefreshMenuWithTab(BasePlayer player, MenuTab tab)
    {
        _currentMenuTab[player.userID] = tab;

        NavigateMenu(player, MenuLayer.MainMenu);
    }

    // Add this to your enums
    private enum MenuTab
    {
        Records,
        Logs
    }

    // Helper methods to manage logs
    private void AddLogMessage(string message)
    {
        Puts(message);

        // Add timestamp to message
        string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";

        // Add to log list with a cap (e.g., 100 messages)
        _logMessages.Insert(0, timestampedMessage);
        if (_logMessages.Count > 100)
        {
            _logMessages.RemoveAt(_logMessages.Count - 1);
        }
    }

    private List<string> GetRecentLogs(int count)
    {
        return _logMessages.Take(Mathf.Min(count, _logMessages.Count)).ToList();
    }

    private void ClearLogMessages()
    {
        _logMessages.Clear();
        AddLogMessage("Log cleared");
    }

    #endregion

    #region Snapshots Menu

    /// <summary>
    /// Opens the snapshots menu for a specific building record.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    /// <param name="record">The building record to show snapshots for.</param>
    private void OpenSnapshotsMenu(BasePlayer player, BuildRecord record)
    {
        _currentBuildRecord[player.userID] = record.NetworkID;

        List<Guid> snapshots = GetSnapshotsForRecord(record);
        if (snapshots.Count > 0 && !_currentSelectedSnapshot.ContainsKey(player.userID))
        {
            _currentSelectedSnapshot[player.userID] = snapshots[0];
        }

        NavigateMenu(player, MenuLayer.Snapshots, true, record, snapshots);
    }

    /// <summary>
    /// Gets the list of snapshots for the given record.
    /// </summary>
    /// <param name="record">The building record to get snapshots for.</param>
    /// <returns>A list of snapshot IDs for the record.</returns>
    private List<Guid> GetSnapshotsForRecord(BuildRecord record)
    {
        List<Guid> result = new();

        // Try to find snapshots by the persistent ID
        if (_idxSnapshotBuildingID.TryGetValue(record.PersistentID, out var snapshots))
        {
            result.AddRange(snapshots);
        }

        // Sort by timestamp (newest first)
        result.Sort((a, b) =>
        {
            if (_snapshotMetaData.TryGetValue(a, out var metaA) &&
                _snapshotMetaData.TryGetValue(b, out var metaB))
            {
                return metaB.TimestampUTC.CompareTo(metaA.TimestampUTC);
            }
            return 0;
        });

        return result;
    }

    /// <summary>
    /// Sends the snapshots menu to the player.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    /// <param name="record">The building record being viewed.</param>
    /// <param name="snapshots">The list of snapshots for the record.</param>
    private void CreateSnapshotsMenu(CUI cui, BasePlayer player, BuildRecord record, List<Guid> snapshots)
    {
        // Create the base container with cursor
        var container = cui.v2
            .CreateParent(
                parent: CUI.ClientPanels.HudMenu,
                position: LuiPosition.Full,
                name: _snapshotMenuId)
            .AddCursor();

        // Main panel
        var main = cui.v2
            .CreatePanel(
                container: container,
                position: LuiPosition.MiddleCenter,
                offset: new(-450, -275, 450, 275),
                color: "0 0 0 .95"
            );

        // Header
        var header = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, .94f, 1, 1),
                offset: LuiOffset.None,
                color: "0 0 0 .9"
            );

        // Title
        cui.v2.CreateText(
                container: header,
                position: new(.015f, 0.01f, .8f, .95f),
                offset: new(0, 0, 0, 0),
                color: "1 1 1 .8",
                fontSize: 18,
                text: $"Snapshots for Building at {record.BaseTC.ServerPosition:F1}",
                alignment: TextAnchor.MiddleLeft
            )
            .SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Back button
        var backButton = cui.v2
            .CreateButton(
                container: header,
                position: new(.8f, .15f, .93f, .85f),
                offset: new(4, 0, 4, 0),
                command: $"{nameof(AutoBuildSnapshot)}.menu.snapshots.back",
                color: "0.3 0.3 0.6 0.9"
            );

        cui.v2.CreateText(
            container: backButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 .9",
            fontSize: 14,
            text: "Back",
            alignment: TextAnchor.MiddleCenter
        );

        // Close button
        var closeButton = cui.v2
            .CreateButton(
                container: header,
                position: new(.965f, .15f, .99f, .85f),
                offset: new(4, 0, 4, 0),
                command: $"{nameof(AutoBuildSnapshot)}.menu.close",
                color: ".6 .2 .2 .9"
            );

        cui.v2.CreateImageFromDb(
            container: closeButton,
            position: new(.2f, .2f, .8f, .8f),
            offset: LuiOffset.None,
            dbName: "close",
            color: "1 1 1 .5"
        );

        // Content area - split into left and right panels
        var contentPanel = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, .08f, 1, .94f),
                offset: LuiOffset.None,
                color: "0.15 0.15 0.15 0"
            );

        // Left panel (snapshot list)
        var leftPanel = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0, .35f, 1),
                offset: new(10, 10, -5, -10),
                color: "0.15 0.15 0.15 1"
            );

        // Title for snapshot list
        cui.v2.CreateText(
            container: leftPanel,
            position: new(0, .95f, 1, 1),
            offset: new(10, 0, -10, 0),
            color: "1 1 1 .8",
            fontSize: 14,
            text: "Available Snapshots",
            alignment: TextAnchor.MiddleLeft
        ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Snapshot list
        var snapshotList = cui.v2
            .CreatePanel(
                container: leftPanel,
                position: new(0, 0, 1, .95f),
                offset: new(5, 5, -5, -5),
                color: "0 0 0 0"
            );

        if (snapshots.Count == 0)
        {
            // No snapshots message
            cui.v2.CreateText(
                container: snapshotList,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "1 1 1 .5",
                fontSize: 14,
                text: "No snapshots available for this building.",
                alignment: TextAnchor.MiddleCenter
            );
        }
        else
        {
            // List snapshots
            float itemHeight = 0.08f;
            int visibleCount = Mathf.Min(snapshots.Count, 10);

            // Get scroll position
            if (!_snapshotScrollIndex.TryGetValue(player.userID, out int scrollIndex))
            {
                _snapshotScrollIndex[player.userID] = 0;
                scrollIndex = 0;
            }

            // Clamp scroll index
            int maxScroll = Mathf.Max(0, snapshots.Count - visibleCount);
            scrollIndex = Mathf.Clamp(scrollIndex, 0, maxScroll);
            _snapshotScrollIndex[player.userID] = scrollIndex;

            // Get current selection
            Guid selectedId = Guid.Empty;
            _currentSelectedSnapshot.TryGetValue(player.userID, out selectedId);

            // Display snapshots
            for (int i = scrollIndex; i < scrollIndex + visibleCount && i < snapshots.Count; i++)
            {
                Guid snapshotId = snapshots[i];
                float yMin = 1f - (i - scrollIndex + 1) * itemHeight;
                float yMax = 1f - (i - scrollIndex) * itemHeight;

                bool isSelected = snapshotId == selectedId;

                // Background
                var itemBg = cui.v2
                    .CreatePanel(
                        container: snapshotList,
                        position: new(0, yMin, 1, yMax),
                        offset: new(0, 2, 0, -2),
                        color: isSelected ? "0.2 0.4 0.6 0.8" : (i % 2 == 0 ? "0.2 0.2 0.2 0.5" : "0.25 0.25 0.25 0.5")
                    );

                // Get metadata
                string displayText = "Unknown Snapshot";
                if (_snapshotMetaData.TryGetValue(snapshotId, out var meta))
                {
                    displayText = $"{meta.TimestampUTC:yyyy-MM-dd HH:mm} ({meta.Entities} entities)";
                }

                // Button to select snapshot
                cui.v2
                    .CreateButton(
                        container: itemBg,
                        position: LuiPosition.Full,
                        offset: LuiOffset.None,
                        command: $"{nameof(AutoBuildSnapshot)}.snapshots.select {snapshotId}",
                        color: "1 0 0 1"
                    )
                    .AddCursor();

                // Snapshot text
                cui.v2.CreateText(
                    container: itemBg,
                    position: new(0, 0, 1, 1),
                    offset: new(10, 0, -10, 0),
                    color: "1 1 1 .9",
                    fontSize: 12,
                    text: displayText,
                    alignment: TextAnchor.MiddleLeft
                );
            }

            // Add scroll buttons if needed
            if (snapshots.Count > visibleCount)
            {
                // Up button (if not at top)
                if (scrollIndex > 0)
                {
                    var upButton = cui.v2
                        .CreateButton(
                            container: leftPanel,
                            position: new(.9f, .95f, .98f, .99f),
                            offset: LuiOffset.None,
                            command: $"{nameof(AutoBuildSnapshot)}.snapshots.scroll -1",
                            color: "0.3 0.3 0.3 1"
                        );

                    cui.v2.CreateText(
                        container: upButton,
                        position: LuiPosition.Full,
                        offset: LuiOffset.None,
                        color: "1 1 1 1",
                        fontSize: 12,
                        text: "▲",
                        alignment: TextAnchor.MiddleCenter
                    );
                }

                // Down button (if not at bottom)
                if (scrollIndex < maxScroll)
                {
                    var downButton = cui.v2
                        .CreateButton(
                            container: leftPanel,
                            position: new(.9f, .01f, .98f, .05f),
                            offset: LuiOffset.None,
                            command: $"{nameof(AutoBuildSnapshot)}.snapshots.scroll 1",
                            color: "0.3 0.3 0.3 1"
                        );

                    cui.v2.CreateText(
                        container: downButton,
                        position: LuiPosition.Full,
                        offset: LuiOffset.None,
                        color: "1 1 1 1",
                        fontSize: 12,
                        text: "▼",
                        alignment: TextAnchor.MiddleCenter
                    );
                }
            }
        }

        // Right panel (snapshot details)
        var rightPanel = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(.35f, 0, 1, 1),
                offset: new(5, 10, -10, -10),
                color: "0.15 0.15 0.15 1"
            );

        // Show snapshot details if one is selected
        if (snapshots.Count > 0 && _currentSelectedSnapshot.TryGetValue(player.userID, out Guid selectedSnapshot) &&
            _snapshotMetaData.TryGetValue(selectedSnapshot, out var snapshotData))
        {
            // Snapshot details section
            var detailsHeader = cui.v2
                .CreatePanel(
                    container: rightPanel,
                    position: new(0, .9f, 1, 1),
                    offset: new(0, 0, 0, 0),
                    color: "0.2 0.2 0.3 1"
                );

            cui.v2.CreateText(
                container: detailsHeader,
                position: LuiPosition.Full,
                offset: new(10, 0, -10, 0),
                color: "1 1 1 1",
                fontSize: 14,
                text: "Snapshot Details",
                alignment: TextAnchor.MiddleLeft
            ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

            // Details content panel
            var detailsContent = cui.v2
                .CreatePanel(
                    container: rightPanel,
                    position: new(0, .3f, 1, .9f),
                    offset: new(10, 5, -10, -5),
                    color: "0.1 0.1 0.1 0.5"
                );

            // Timestamp
            cui.v2.CreateText(
                container: detailsContent,
                position: new(0, .85f, 1, 1),
                offset: new(5, 0, -5, 0),
                color: "1 1 1 .9",
                fontSize: 14,
                text: $"Timestamp: {snapshotData.TimestampUTC:yyyy-MM-dd HH:mm:ss} UTC",
                alignment: TextAnchor.MiddleLeft
            );

            // Entities count
            cui.v2.CreateText(
                container: detailsContent,
                position: new(0, .75f, 1, .85f),
                offset: new(5, 0, -5, 0),
                color: "1 1 1 .9",
                fontSize: 14,
                text: $"Total Entities: {snapshotData.Entities}",
                alignment: TextAnchor.MiddleLeft
            );

            // Linked buildings count
            cui.v2.CreateText(
                container: detailsContent,
                position: new(0, .65f, 1, .75f),
                offset: new(5, 0, -5, 0),
                color: "1 1 1 .9",
                fontSize: 14,
                text: $"Linked Buildings: {snapshotData.LinkedBuildings.Count}",
                alignment: TextAnchor.MiddleLeft
            );

            // Authorized users section
            cui.v2.CreateText(
                container: detailsContent,
                position: new(0, .55f, 1, .65f),
                offset: new(5, 0, -5, 0),
                color: "1 1 1 .9",
                fontSize: 14,
                text: $"Authorized Users: {snapshotData.AuthorizedPlayers.Count}",
                alignment: TextAnchor.MiddleLeft
            );

            // Display authorized users
            int authCount = Mathf.Min(snapshotData.AuthorizedPlayers.Count, 3); // Show up to 3 users
            for (int i = 0; i < authCount; i++)
            {
                var user = snapshotData.AuthorizedPlayers[i];
                cui.v2.CreateText(
                    container: detailsContent,
                    position: new(0.1f, .55f - ((i + 1) * 0.08f), 1, .55f - (i * 0.08f)),
                    offset: new(5, 0, -5, 0),
                    color: "0.9 0.9 0.9 .8",
                    fontSize: 12,
                    text: $"• {user.UserName} ({user.UserID})",
                    alignment: TextAnchor.MiddleLeft
                );
            }

            // Show "more users" if there are more than we displayed
            if (snapshotData.AuthorizedPlayers.Count > authCount)
            {
                int remainingUsers = snapshotData.AuthorizedPlayers.Count - authCount;
                cui.v2.CreateText(
                    container: detailsContent,
                    position: new(0.1f, .55f - ((authCount + 1) * 0.08f), 1, .55f - (authCount * 0.08f)),
                    offset: new(5, 0, -5, 0),
                    color: "0.7 0.7 0.7 .7",
                    fontSize: 12,
                    text: $"• And {remainingUsers} more user{(remainingUsers > 1 ? "s" : "")}...",
                    alignment: TextAnchor.MiddleLeft
                );
            }

            // Linked buildings section
            var linkedBuildingsPanel = cui.v2
                .CreatePanel(
                    container: rightPanel,
                    position: new(0, .1f, 1, .3f),
                    offset: new(10, 5, -10, -5),
                    color: "0.1 0.1 0.1 0.5"
                );

            cui.v2.CreateText(
                container: linkedBuildingsPanel,
                position: new(0, .85f, 1, 1),
                offset: new(5, 0, -5, 0),
                color: "1 1 1 .9",
                fontSize: 14,
                text: "Linked Buildings:",
                alignment: TextAnchor.MiddleLeft
            ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

            // List linked buildings
            int buildingCount = Mathf.Min(snapshotData.LinkedBuildings.Count, 3); // Show up to 3 buildings
            int buildingIndex = 0;

            foreach (var building in snapshotData.LinkedBuildings)
            {
                if (buildingIndex >= buildingCount) break;

                // Building container
                var buildingItem = cui.v2
                    .CreatePanel(
                        container: linkedBuildingsPanel,
                        position: new(0, .85f - ((buildingIndex + 1) * 0.2f), 1, .85f - (buildingIndex * 0.2f)),
                        offset: new(5, 2, -5, -2),
                        color: buildingIndex % 2 == 0 ? "0.15 0.15 0.15 0.7" : "0.18 0.18 0.18 0.7"
                    );

                // Building info
                cui.v2.CreateText(
                    container: buildingItem,
                    position: new(0, 0, .7f, 1),
                    offset: new(5, 0, -5, 0),
                    color: "1 1 1 .9",
                    fontSize: 12,
                    text: $"Position: {building.Value.Position:F1} | Entities: {building.Value.Entities}",
                    alignment: TextAnchor.MiddleLeft
                );

                // Teleport button
                var teleportButton = cui.v2
                    .CreateButton(
                        container: buildingItem,
                        position: new(.7f, .2f, .95f, .8f),
                        offset: LuiOffset.None,
                        command: $"{nameof(AutoBuildSnapshot)}.snapshots.teleport {building.Key}",
                        color: "0.3 0.5 0.3 1"
                    );

                cui.v2.CreateText(
                    container: teleportButton,
                    position: LuiPosition.Full,
                    offset: LuiOffset.None,
                    color: "1 1 1 1",
                    fontSize: 12,
                    text: "Teleport",
                    alignment: TextAnchor.MiddleCenter
                );

                buildingIndex++;
            }

            // Show "more buildings" if there are more than we displayed
            if (snapshotData.LinkedBuildings.Count > buildingCount)
            {
                int remainingBuildings = snapshotData.LinkedBuildings.Count - buildingCount;
                cui.v2.CreateText(
                    container: linkedBuildingsPanel,
                    position: new(0, .85f - ((buildingCount + 1) * 0.2f), 1, .85f - (buildingCount * 0.2f)),
                    offset: new(5, 0, -5, 0),
                    color: "0.7 0.7 0.7 .7",
                    fontSize: 12,
                    text: $"And {remainingBuildings} more building{(remainingBuildings > 1 ? "s" : "")}...",
                    alignment: TextAnchor.MiddleLeft
                );
            }
        }
        else
        {
            // No snapshot selected message
            cui.v2.CreateText(
                container: rightPanel,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "1 1 1 .5",
                fontSize: 14,
                text: "Select a snapshot to view details.",
                alignment: TextAnchor.MiddleCenter
            );
        }

        // Bottom buttons panel
        var buttonPanel = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, 0, 1, .08f),
                offset: new(10, 5, -10, -5),
                color: "0.2 0.2 0.2 1"
            );

        // Show Zones button
        var showZonesButton = cui.v2
            .CreateButton(
                container: buttonPanel,
                position: new(0, .2f, .25f, .8f),
                offset: new(10, 0, -5, 0),
                command: $"{nameof(AutoBuildSnapshot)}.snapshots.showzones",
                color: "0.3 0.3 0.6 1"
            );

        cui.v2.CreateText(
            container: showZonesButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 1",
            fontSize: 14,
            text: "Show Zones",
            alignment: TextAnchor.MiddleCenter
        );

        // Rollback button
        var rollbackButton = cui.v2
            .CreateButton(
                container: buttonPanel,
                position: new(.35f, .2f, .6f, .8f),
                offset: new(5, 0, -5, 0),
                command: $"{nameof(AutoBuildSnapshot)}.snapshots.rollback",
                color: "0.6 0.3 0.3 1"
            );

        cui.v2.CreateText(
            container: rollbackButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 1",
            fontSize: 14,
            text: "Rollback",
            alignment: TextAnchor.MiddleCenter
        );

        // Undo button
        var undoButton = cui.v2
            .CreateButton(
                container: buttonPanel,
                position: new(.7f, .2f, .95f, .8f),
                offset: new(5, 0, -10, 0),
                command: $"{nameof(AutoBuildSnapshot)}.snapshots.undo",
                color: "0.5 0.5 0.3 1"
            );

        cui.v2.CreateText(
            container: undoButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 1",
            fontSize: 14,
            text: "Undo",
            alignment: TextAnchor.MiddleCenter
        );
    }

    #region Snapshot Button Commands

    // Command handlers for the snapshots UI
    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.menu.snapshots.back")]
    private void CommandSnapshotsBack(BasePlayer player)
    {
        CuiHelper.DestroyUi(player, _snapshotMenuId);
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.select")]
    private void CommandSelectSnapshot(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0)
            return;

        if (Guid.TryParse(args[0], out Guid snapshotId) && _snapshotMetaData.ContainsKey(snapshotId))
        {
            _currentSelectedSnapshot[player.userID] = snapshotId;

            if (_currentBuildRecord.TryGetValue(player.userID, out ulong recordId) &&
                _buildRecords.TryGetValue(recordId, out var record))
            {
                OpenSnapshotsMenu(player, record);
            }
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.scroll")]
    private void CommandScrollSnapshots(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0)
            return;

        if (int.TryParse(args[0], out int delta))
        {
            if (!_snapshotScrollIndex.TryGetValue(player.userID, out int currentIndex))
            {
                _snapshotScrollIndex[player.userID] = 0;
                currentIndex = 0;
            }

            _snapshotScrollIndex[player.userID] = Mathf.Max(0, currentIndex + delta);

            if (_currentBuildRecord.TryGetValue(player.userID, out ulong recordId) &&
                _buildRecords.TryGetValue(recordId, out var record))
            {
                OpenSnapshotsMenu(player, record);
            }
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.teleport")]
    private void CommandSnapTeleportToBuilding(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0)
            return;

        // Check admin permission
        if (!permission.UserHasPermission(player.UserIDString, _config.Commands.AdminPermission))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.command_no_permission, player));
            return;
        }

        // Get the position from the building metadata
        if (_currentSelectedSnapshot.TryGetValue(player.userID, out Guid snapshotId) &&
            _snapshotMetaData.TryGetValue(snapshotId, out var meta) &&
            meta.LinkedBuildings.TryGetValue(args[0], out var buildingMeta))
        {
            // Teleport player to the building position
            player.Teleport(buildingMeta.Position + new Vector3(0, 1, 0));
            player.SendNetworkUpdate();

            player.ChatMessage($"Teleported to building at {buildingMeta.Position:F1}");
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.showzones")]
    private void CommandShowSnapZones(BasePlayer player)
    {
        // Check admin permission
        if (!permission.UserHasPermission(player.UserIDString, _config.Commands.AdminPermission))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.command_no_permission, player));
            return;
        }

        /*
        // Visualization will be added later
        if (_currentSelectedSnapshot.TryGetValue(player.userID, out var snapshotId))
        {
            // TODO: Create visualization for the zones
            // Create timer callback to remove the zones after a certain time
        }
        */
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.rollback")]
    private void CommandRollbackSnapshot(BasePlayer player)
    {
        // Check admin permission
        if (!_config.Commands.UserHasPermission(player, _config.Commands.Rollback, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.command_no_permission, player));
            return;
        }

        if (_currentSelectedSnapshot.TryGetValue(player.userID, out Guid snapshotId))
        {
            NavigateMenu(player, MenuLayer.ConfirmationDialog, false,
                "Confirm Rollback",
                $"Are you sure you want to rollback to the snapshot from {_snapshotMetaData[snapshotId].TimestampUTC:yyyy-MM-dd HH:mm:ss}?",
                $"{nameof(AutoBuildSnapshot)}.confirm.rollback {snapshotId}"
            );
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.undo")]
    private void CommandUndoRollback(BasePlayer player)
    {
        // Check admin permission
        if (!_config.Commands.UserHasPermission(player, _config.Commands.Rollback, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.command_no_permission, player));
            return;
        }

        NavigateMenu(player, MenuLayer.ConfirmationDialog, false,
            "Confirm Undo",
            "Are you sure you want to undo the last rollback operation?",
            $"{nameof(AutoBuildSnapshot)}.confirm.undo"
        );
    }

    #endregion

    #endregion

    #region Confirmation Dialog

    /// <summary>
    /// Shows a confirmation dialog to the player.
    /// </summary>
    /// <param name="player">The player to show the dialog to.</param>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message to display in the dialog.</param>
    /// <param name="confirmCommand">The command to execute if the player confirms.</param>
    private void CreateConfirmationDialog(CUI cui, BasePlayer player, string title, string message, string confirmCommand)
    {
        // Create the base container with cursor
        var container = cui.v2
            .CreateParent(
                parent: CUI.ClientPanels.Overlay,
                position: LuiPosition.Full,
                name: _confirmationDialogId)
            .AddCursor();

        // Semi-transparent overlay
        cui.v2
            .CreatePanel(
                container: container,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "0 0 0 0.7"
            );

        // Dialog panel
        var dialogPanel = cui.v2
            .CreatePanel(
                container: container,
                position: LuiPosition.MiddleCenter,
                offset: new(-200, -125, 200, 125),
                color: "0.1 0.1 0.1 0.95"
            );

        // Dialog border
        cui.v2
            .CreatePanel(
                container: dialogPanel,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "0.3 0.3 0.3 1"
            )
            .SetOutline("0.5 0.5 0.5 1", new(2, 2));

        // Dialog content
        var contentPanel = cui.v2
            .CreatePanel(
                container: dialogPanel,
                position: new(0.01f, 0.01f, 0.99f, 0.99f),
                offset: LuiOffset.None,
                color: "0.15 0.15 0.15 1"
            );

        // Title bar
        var titleBar = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0.85f, 1, 1),
                offset: LuiOffset.None,
                color: "0.2 0.2 0.3 1"
            );

        // Title
        cui.v2.CreateText(
            container: titleBar,
            position: new(0, 0, 1, 1),
            offset: new(15, 0, -15, 0),
            color: "1 1 1 0.95",
            fontSize: 16,
            text: title,
            alignment: TextAnchor.MiddleCenter
        ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Message area
        var messageArea = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0.2f, 1, 0.85f),
                offset: new(15, 10, -15, -10),
                color: "0 0 0 0"
            );

        // Message text
        cui.v2.CreateText(
            container: messageArea,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 0.9",
            fontSize: 14,
            text: message,
            alignment: TextAnchor.MiddleCenter
        );

        // Buttons area
        var buttonsArea = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0.03f, 1, 0.2f),
                offset: LuiOffset.None,
                color: "0 0 0 0"
            );

        // Confirm button
        var confirmBtn = cui.v2
            .CreateButton(
                container: buttonsArea,
                position: new(0.15f, 0.2f, 0.45f, 0.8f),
                offset: LuiOffset.None,
                command: confirmCommand,
                color: "0.3 0.5 0.3 1"
            );

        // Confirm text
        cui.v2.CreateText(
            container: confirmBtn,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 0.95",
            fontSize: 14,
            text: "Confirm",
            alignment: TextAnchor.MiddleCenter
        );

        // Cancel button
        var cancelBtn = cui.v2
            .CreateButton(
                container: buttonsArea,
                position: new(0.55f, 0.2f, 0.85f, 0.8f),
                offset: LuiOffset.None,
                command: $"{nameof(AutoBuildSnapshot)}.confirm.cancel",
                color: "0.5 0.3 0.3 1"
            );

        // Cancel text
        cui.v2.CreateText(
            container: cancelBtn,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 0.95",
            fontSize: 14,
            text: "Cancel",
            alignment: TextAnchor.MiddleCenter
        );
    }

    /// <summary>
    /// Closes the confirmation dialog.
    /// </summary>
    /// <param name="player">The player to close the dialog for.</param>
    private void CloseConfirmationDialog(BasePlayer player)
    {
        CuiHelper.DestroyUi(player, _confirmationDialogId);
    }

    // Command handlers for confirmation dialog
    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.confirm.cancel")]
    private void CommandCancelConfirmation(BasePlayer player)
    {
        CloseConfirmationDialog(player);
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.confirm.rollback")]
    private void CommandConfirmRollback(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0)
            return;

        // Check admin permission
        if (!_config.Commands.UserHasPermission(player, _config.Commands.Rollback, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.command_no_permission, player));
            CloseConfirmationDialog(player);
            return;
        }

        if (Guid.TryParse(args[0], out Guid snapshotId) && _snapshotMetaData.ContainsKey(snapshotId))
        {
            CloseConfirmationDialog(player);

            // Execute rollback (implementation would be in your plugin)
            ExecuteRollback(player, snapshotId);
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.confirm.undo")]
    private void CommandConfirmUndo(BasePlayer player)
    {
        // Check admin permission
        if (!_config.Commands.UserHasPermission(player, _config.Commands.Rollback, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.command_no_permission, player));
            CloseConfirmationDialog(player);
            return;
        }

        CloseConfirmationDialog(player);

        // Execute undo (implementation would be in your plugin)
        ExecuteUndo(player);
    }

    #endregion

    /// <summary>
    /// Executes the rollback operation for the specified snapshot.
    /// </summary>
    /// <param name="player">The player who initiated the rollback.</param>
    /// <param name="snapshotId">The ID of the snapshot to rollback to.</param>
    private void ExecuteRollback(BasePlayer player, Guid snapshotId)
    {
        // This method would contain your implementation of the rollback process
        // For now, it just logs the action and notifies the player
        AddLogMessage($"Player {player.displayName} initiated rollback to snapshot {snapshotId}");
        player.ChatMessage($"Rollback to snapshot {snapshotId} initiated. This feature is not yet fully implemented.");

        // In a complete implementation, you would:
        // 1. Load the snapshot data
        // 2. Create a backup of the current state as an "undo" point
        // 3. Perform the rollback by removing/adding entities according to the snapshot
        // 4. Update all relevant tracking information
    }

    /// <summary>
    /// Executes the undo operation for the last rollback.
    /// </summary>
    /// <param name="player">The player who initiated the undo.</param>
    private void ExecuteUndo(BasePlayer player)
    {
        // This method would contain your implementation of the undo process
        // For now, it just logs the action and notifies the player
        AddLogMessage($"Player {player.displayName} initiated undo of last rollback");
        player.ChatMessage("Undo of last rollback initiated. This feature is not yet fully implemented.");

        // In a complete implementation, you would:
        // 1. Check if there is an undo point available
        // 2. If so, load the undo data and restore the previous state
        // 3. Update all relevant tracking information
    }

    private void NavigateMenu(
        BasePlayer player,
        MenuLayer targetLayer,
        bool appendLayer = false,
        params object[] args)
    {
        if (targetLayer == MenuLayer.Closed)
        {
            // don't do this, it's not supported
            if (appendLayer)
            {
                throw new Exception($"Cannot append layer '{nameof(MenuLayer.Closed)}'");
            }
        }
        else
        {
            // also don't do this, it's more complicated to navigate multiple layers in one go
            if ((targetLayer & (targetLayer - 1)) != 0)
            {
                throw new Exception("Cannot process multiple layers");
            }
        }

        if (!_playerMenuStates.TryGetValue(player.userID, out var currentLayer))
            currentLayer = MenuLayer.Closed;

        if (targetLayer == currentLayer)
        {
            if (targetLayer == MenuLayer.MainMenu && args.Length > 0 && args[0] is MenuTab newTab)
            {
                if (_currentMenuTab.TryGetValue(player.userID, out var currentTab) && currentTab == newTab)
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        _previousMenuState[player.userID] = currentLayer;

        if (appendLayer)
        {
            currentLayer |= targetLayer;
        }
        else
        {
            currentLayer = targetLayer;
        }

        if (currentLayer == MenuLayer.ConfirmationDialog && !targetLayer.HasFlag(MenuLayer.ConfirmationDialog))
        {
            CuiHelper.DestroyUi(player, _confirmationDialogId);
        }

        if (currentLayer == MenuLayer.Snapshots && !targetLayer.HasFlag(MenuLayer.Snapshots))
        {
            CuiHelper.DestroyUi(player, _snapshotMenuId);
        }

        if (currentLayer == MenuLayer.MainMenu && !targetLayer.HasFlag(MenuLayer.MainMenu))
        {
            CuiHelper.DestroyUi(player, _mainMenuId);
        }

        if (targetLayer == MenuLayer.Closed)
            return;

        using var cui = CreateCUI();

        switch (targetLayer)
        {
            case MenuLayer.MainMenu:
                CreateMainMenu(cui, player);
                break;

            case MenuLayer.Snapshots:
            case MenuLayer.ConfirmationDialog:
            default:
                throw new NotSupportedException($"Unsupported menu layer: {targetLayer}");
        }

        cui.v2.SendUi(player);

        _playerMenuStates[player.userID] = currentLayer;
    }

    /// <summary>
    /// Represents the menu state for the player.
    /// </summary>
    [Flags]
    private enum MenuLayer
    {
        /// <summary>
        /// The menu is closed.
        /// </summary>
        Closed = 0,

        MainMenu = 1 << 0,

        Snapshots = 1 << 1,

        ConfirmationDialog = 1 << 2,
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
        _buildRecords = Pool.Get<Dictionary<ulong, BuildRecord>>();
        _snapshotMetaData = Pool.Get<Dictionary<Guid, BuildSnapshotMetaData>>();
        _idxSnapshotBuildingID = Pool.Get<Dictionary<string, List<Guid>>>();
        _tempEntities = Pool.Get<Dictionary<ulong, List<ClientEntity>>>();
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
        KillTempEntities();

        foreach (var player in BasePlayer.activePlayerList)
        {
            NavigateMenu(player, MenuLayer.Closed);
        }

        Pool.Free(ref _buildRecords, true);
        Pool.FreeUnmanaged(ref _snapshotMetaData);
        FreeDictionaryList(ref _idxSnapshotBuildingID);
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

        var snapshotMetaDataFiles = Interface.Oxide.DataFileSystem.GetFiles(_snapshotDataDirectory, $"*.{_snapshotMetaExtension}.json");
        if (snapshotMetaDataFiles.Length == 0)
        {
            AddLogMessage("Did not find any snapshots (directory was empty or had no matching meta data files).");
            return;
        }

        AddLogMessage($"Loading {snapshotMetaDataFiles.Length} snapshot(s)");

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

            _snapshotMetaData[metaData.ID] = metaData;

            IndexLinkedBuildings(metaData);
        }

        if (deletions > 0)
        {
            AddLogMessage($"Deleted {deletions} snapshot(s) older than {_config.General.SnapshotRetentionPeriodHours} hours");
        }
    }

    /// <summary>
    /// Indexes the linked buildings in the snapshot metadata.
    /// </summary>
    /// <param name="metaData">The snapshot metadata to index.</param>
    private void IndexLinkedBuildings(BuildSnapshotMetaData metaData)
    {
        foreach (var linkedBuilding in metaData.LinkedBuildings)
        {
            if (!_idxSnapshotBuildingID.ContainsKey(linkedBuilding.Key))
            {
                _idxSnapshotBuildingID[linkedBuilding.Key] = Pool.Get<List<Guid>>();
            }

            _idxSnapshotBuildingID[linkedBuilding.Key].Add(metaData.ID);
        }
    }

    #endregion

    #region Build Change Management

    /// <summary>
    /// Initializes the timer to monitor builds.
    /// This method will clean up any existing timer.
    /// </summary>
    /// <param name="interval">The interval, in seconds, to check for build updates.</param>
    private void InitBuildMonitor(float interval)
    {
        _buildMonitor?.Destroy();

        _buildMonitor = timer.Every(_config.General.BuildMonitorInterval, BuildMonitorCycle);
    }

    #region Processing

    /// <summary>
    /// Cycles through the build monitor, checking for changes and processing them.
    /// </summary>
    private void BuildMonitorCycle()
    {
        if (_buildRecords is null or { Count: 0 })
            return;

        foreach (var record in _buildRecords.Values)
        {
            // process retry attempts
            if (record.State == BuildState.Save_Failure)
            {
                if (record.RetryCount >= _config.Advanced.MaxSaveFailRetries)
                {
                    AddLogMessage($"Build save had {record.RetryCount} retry attempt(s), terminating");
                    record.Update(BuildState.RetryLimitExceeded);
                }
                else
                {
                    AddLogMessage($"Queue build save for retry attempt {record.RetryCount + 1}/{_config.Advanced.MaxSaveFailRetries}");
                    record.Update(BuildState.Queued);
                }
            }
            // process anything that is currently modified (including those just flagged above)
            else if (record.State == BuildState.Modified)
            {
                // Update anything set to 'modified' to 'queued' if applicable (> save delay span)
                if (DateTime.Now - record.LastSaveSuccess > TimeSpan.FromSeconds(_config.General.DelayBetweenSaves))
                {
                    record.Update(BuildState.Queued);
                }
            }
        }

        // trigger process queue
        ProcessNextSave(recursive: true);
    }

    /// <summary>
    /// Processes the next save in the queue.
    /// </summary>
    /// <param name="record">The record to process. If null, will find the next queued record.</param>
    /// <param name="callback">The callback to invoke when the save is complete.</param>
    /// <param name="recursive">Whether to continue to process queued saves (only meant to be called from the build monitor process).</param>
    private void ProcessNextSave(BuildRecord record = null, Action<bool, BuildSnapshot> callback = null, bool recursive = false)
    {
        if (record == null)
        {
            // Check if we have anything to process
            var queue = _buildRecords.Where(r => r.Value.State == BuildState.Queued);
            if (!queue.Any())
            {
                return;
            }

            // Get the next process item and flag that it is currently processing
            var next = queue.First();
            record = next.Value;
        }

        record.Update(BuildState.Processing);

        var snapshot = Pool.Get<BuildSnapshot>();
        snapshot.Init(this, record, (success, record) =>
        {
            if (success)
            {
                AddLogMessage($"Saved {record.LinkedRecords.Count} building(s) in {record.FrameCount} frames (total: {record.Duration} ms | longest: {record.LongestStepDuration} ms)");
            }
            else
            {
                if (record.Exception != null)
                {
                    AddLogMessage($"Failed to save {record.LinkedRecords.Count} building(s), reason: {record.Exception}");
                }
                else
                {
                    AddLogMessage($"Failed to save {record.LinkedRecords.Count} building(s), reason unknown");
                }
            }

            callback?.Invoke(success, record);

            if (recursive)
            {
                NextFrame(() => ProcessNextSave());
            }
        });

        snapshot.BeginSave();
    }

    #endregion

    #region Recording

    /// <summary>
    /// Attempts to begin recording/tracking the specified tc.
    /// </summary>
    /// <param name="tc">The tc to record.</param>
    private void StartRecording(BuildingPrivlidge tc)
    {
        if (!ValidEntity(tc))
        {
            AddLogMessage($"Failed to record entity (invalid): {tc.net.ID}");
            return;
        }

        var recordId = tc.net.ID.Value;
        if (_buildRecords.TryGetValue(recordId, out var existing))
        {
            // Try to catch all edge cases when we have an existing tc
            // Realistically, we should never be here, but just in case

            if (tc == existing.BaseTC)
            {
                return;
            }
            else if (!ValidEntity(existing.BaseTC))
            {
                AddLogMessage($"Warning: Found existing entity with id '{recordId}' that appears invalid and will stop recording.");

            }
            else if (existing.NetworkID != recordId)
            {
                AddLogMessage($"Warning: Found existing entity with id '{existing.NetworkID}' in bucket '{recordId}', reassigning.");
                NextFrame(() => StartRecording(existing.BaseTC));
            }
            else
            {
                var persistantID = GetPersistanceID(tc);
                if (existing.PersistentID == persistantID)
                {
                    return;
                }
                else
                {
                    AddLogMessage($"Warning: Found existing entity with id '{existing.NetworkID}' in location '{existing.BaseTC.ServerPosition}' that will stop recording.");
                }
            }

            // Regardless of where we ended up, if existing isn't the current tc, we need to get replace it
            Pool.Free(ref existing);
        }

        // By this point, our bucket should be ready to assign (warnings printed or method returned)
        var record = Pool.Get<BuildRecord>()
            .Init(tc);

        AddLogMessage($"Begin recording entity '{recordId}' at coordinates '{record.BaseTC.ServerPosition}'");
        _buildRecords[recordId] = record;
    }

    private enum BuildState
    {
        /// <summary>
        /// The build record is being initialized and needs to be set up.
        /// </summary>
        NeedsInit,

        /// <summary>
        /// Idle, nothing needs to be done
        /// </summary>
        Idle,

        /// <summary>
        /// Changes were made. Build will be added to process queue when ready.
        /// </summary>
        Modified,

        /// <summary>
        /// In processing queue. Only should be here when the build is modified
        /// and the time since last save exceeds the delay between saves.
        /// </summary>
        Queued,

        /// <summary>
        /// When the build has been picked up by an agent for processing.
        /// </summary>
        Processing,

        /// <summary>
        /// Saving: Finding all TCs in radius and linking them together in snapshot.
        /// </summary>
        Save_BuildingNetwork,

        /// <summary>
        /// Saving: Finding all entities to save in linked network.
        /// </summary>
        Save_FindingEntities,

        /// <summary>
        /// Saving: Writing the output snapshot to the save location (by default, saves are on disk).
        /// </summary>
        Save_Writing,

        /// <summary>
        /// Save successfully completed.
        /// </summary>
        Save_Success,

        /// <summary>
        /// Save process failed. This needs to be logged and retries incremented.
        /// After so many retries, this will be removed from the backup queue.
        /// </summary>
        Save_Failure,

        /// <summary>
        /// The build has attempted to snapshot and failed more times than allowed.
        /// </summary>
        RetryLimitExceeded
    }

    /// <summary>
    /// Represents a record of a build, including the zones and changes made to it.
    /// </summary>
    private class BuildRecord : Pool.IPooled
    {
        private List<Vector4> _entityZones;
        private List<Vector4> _linkedZones;
        private bool _zonesDirty;
        private List<ChangeRecord> _changeLog;
        private Dictionary<DateTime, bool> _saveAttempts;
        private DateTime _lastUpdate;

        /// <summary>
        /// Represents a unique id for this world entity that can persist between server restarts.
        /// Built from <see cref="GetPersistanceID(BaseEntity)"/> and includes prefab and position data.
        /// </summary>
        public string PersistentID { get; private set; }

        /// <summary>
        /// The network id of the entity. This id is pooled/recycled and can change between restarts.
        /// Do not use for persistence.
        /// </summary>
        public ulong NetworkID => BaseTC.net.ID.Value;

        /// <summary>
        /// The base TC that this record is tracking.
        /// </summary>
        public BuildingPrivlidge BaseTC { get; private set; }

        /// <summary>
        /// The list of entity zones that this record is tracking. This is a cached list and must be built with <see cref="RefreshZoneCache"/> first.
        /// </summary>
        public List<Vector4> EntityZones
        {
            get
            {
                RefreshZoneCache();
                return _entityZones;
            }
        }

        /// <summary>
        /// The list of linked tc zones that this record is tracking. This is a cached list and must be built with <see cref="RefreshZoneCache"/> first.
        /// </summary>
        public List<Vector4> LinkedZones
        {
            get
            {
                RefreshZoneCache();
                return _linkedZones;
            }
        }

        /// <summary>
        /// The list of changes that have been made to this record since the last save.
        /// </summary>
        public List<ChangeRecord> ChangeLog => _changeLog;

        /// <summary>
        /// The current state of the build record.
        /// </summary>
        public BuildState State { get; private set; }

        /// <summary>
        /// The last time this record was modified.
        /// </summary>
        public DateTime LastModified => ChangeLog.Count > 0
            ? ChangeLog.Last().Time
            : DateTime.MinValue;

        /// <summary>
        /// The last time a save was attempted.
        /// </summary>
        public DateTime LastSaveAttempt => SaveAttempts.Count > 0
            ? SaveAttempts.Last().Key
            : DateTime.MinValue;

        /// <summary>
        /// The last time a save was successful.
        /// </summary>
        public DateTime LastSaveSuccess => SaveAttempts.Any(x => x.Value)
            ? SaveAttempts.Where(x => x.Value).Last().Key
            : DateTime.MinValue;

        /// <summary>
        /// The last time this record was updated.
        /// </summary>
        public DateTime LastUpdate => _lastUpdate;

        /// <summary>
        /// The list of save attempts and their success status.
        /// </summary>
        public Dictionary<DateTime, bool> SaveAttempts => _saveAttempts;

        /// <summary>
        /// The number of retries that have been attempted for this record.
        /// </summary>
        public int RetryCount => SaveAttempts.Values.Reverse().TakeWhile(x => !x).Count() - 1;

        /// <summary>
        /// The last exception that was thrown during processing.
        /// </summary>
        public Exception LastException { get; private set; }

        /// <summary>
        /// Initializes the build record with the specified base TC.
        /// </summary>
        /// <param name="baseTC">The base TC to initialize the record with.</param>
        /// <returns>The initialized build record.</returns>
        public BuildRecord Init(BuildingPrivlidge baseTC)
        {
            BaseTC = baseTC;
            PersistentID = GetPersistanceID(baseTC);
            State = BuildState.Idle;
            return this;
        }

        /// <summary>
        /// Updates the entity and linked TC zone areas for this record's base TC.
        /// </summary>
        public void RefreshZoneCache()
        {
            if (_zonesDirty)
            {
                BuildingScanner.GetZones(_entityZones, BaseTC, _config.Advanced.FoundationPrivilegeRadius, _config.Advanced.MaxScanZoneRadius);

                if (_config.MultiTC.Enabled)
                {
                    BuildingScanner.GetZones(_linkedZones, BaseTC, _config.MultiTC.ScanRadius, _config.Advanced.MaxScanZoneRadius);
                }
            }

            _zonesDirty = false;
        }

        /// <summary>
        /// Adds a change to the change log.
        /// </summary>
        /// <param name="entity">The entity that was changed.</param>
        /// <param name="action">The action that was performed.</param>
        /// <param name="player">The player that made the change.</param>
        public void AddChange(BaseEntity entity, ChangeAction action, BasePlayer player = null)
        {
            ChangeLog.Add(new()
            {
                Time = DateTime.UtcNow,
                Action = action,
                PlayerID = player?.UserIDString,
                PlayerDisplayName = player?.displayName,
            });

            if (State == BuildState.Save_Success || State == BuildState.Idle)
            {
                Update(BuildState.Modified);
            }
            else if (State != BuildState.RetryLimitExceeded)
            {
                Update(State);
            }

            if (entity is BuildingBlock)
            {
                _zonesDirty = true;
            }
        }

        /// <summary>
        /// Scans 2x building priv area for any other TCs, and if they have 
        /// any overlapping owners, they will be linked.
        /// Planning to make this configurable in the future.
        /// </summary>
        public IEnumerable<BuildRecord> FindLinkedRecords()
        {
            return Array.Empty<BuildRecord>();
        }

        /// <summary>
        /// Updates the state of the build record.
        /// </summary>
        /// <param name="state">The new state to set.</param>
        public void Update(BuildState state)
        {
            _lastUpdate = DateTime.Now;

            State = state;

            if (state == BuildState.Save_Success)
            {
                SaveAttempts.Add(_lastUpdate, true);
            }
            else if (state == BuildState.Save_Failure)
            {
                SaveAttempts.Add(_lastUpdate, false);
            }
        }

        /// <summary>
        /// Updates the state of the build record to indicate that a save attempt failed.
        /// </summary>
        /// <param name="ex">The exception that was thrown during the save attempt.</param>
        public void Update(Exception ex)
        {
            LastException = ex;

            Update(BuildState.Save_Failure);
        }

        /// <summary>
        /// Enters the pool and frees any unmanaged resources.
        /// </summary>
        public void EnterPool()
        {
            State = BuildState.NeedsInit;

            BaseTC = null;
            PersistentID = null;
            LastException = null;

            Pool.FreeUnmanaged(ref _entityZones);
            Pool.FreeUnmanaged(ref _linkedZones);
            Pool.FreeUnmanaged(ref _changeLog);
            Pool.FreeUnmanaged(ref _saveAttempts);
        }

        /// <summary>
        /// Leaves the pool and allocates any unmanaged resources.
        /// </summary>
        public void LeavePool()
        {
            _entityZones = Pool.Get<List<Vector4>>();
            _linkedZones = Pool.Get<List<Vector4>>();
            _changeLog = Pool.Get<List<ChangeRecord>>();
            _saveAttempts = Pool.Get<Dictionary<DateTime, bool>>();

            _lastUpdate = DateTime.Now;
            _zonesDirty = true;
        }

        public PersistantEntity GetPersistantEntity() => BaseTC;
    }

    /// <summary>
    /// Represents a change that was made to an entity.
    /// </summary>
    private readonly struct ChangeRecord
    {
        /// <summary>
        /// The time that the change was made.
        /// </summary>
        public DateTime Time { get; init; }

        /// <summary>
        /// The action that was performed on the entity.
        /// </summary>
        public ChangeAction Action { get; init; }

        /// <summary>
        /// The entity that was changed.
        /// </summary>
        public PersistantEntity Entity { get; init; }

        /// <summary>
        /// The id of the player that made the change.
        /// </summary>
        public string PlayerID { get; init; }

        /// <summary>
        /// The display name of the player that made the change.
        /// </summary>
        public string PlayerDisplayName { get; init; }
    }

    /// <summary>
    /// Represents the action that was performed on the entity.
    /// </summary>
    private enum ChangeAction
    {
        Create,
        Update,
        Decay
    }

    #endregion

    #endregion

    #region Helpers

    /// <summary>
    /// Generates an id for the entity based on prefab and location that should persist between server restarts.
    /// </summary>
    /// <param name="entity">The entity to generate the id for.</param>
    /// <returns>The generated id.</returns>
    public static string GetPersistanceID<T>(T entity)
        where T : BaseEntity =>
        GetPersistanceID(typeof(T), entity.ServerPosition);

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
        $"{typeName}[{x:F2}|{y:F2}|{z:F2}]";

    /// <summary>
    /// Checks if the entity has a valid id and isn't destroyed.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    private static bool ValidEntity(BaseNetworkable entity) =>
        entity != null && entity.net != null && entity.net.ID.IsValid && !entity.IsDestroyed;

    /// <summary>
    /// Kills all temporary client entities.
    /// </summary>
    /// <param name="playerID">Optionally, the player ID to kill entities for. Kills all temps if not specified.</param>
    private void KillTempEntities(ulong playerID = 0)
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
    private void KillEntities(List<ClientEntity> entities)
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

    #endregion

    #region BuildSnapshot

    /// <summary>
    /// Represents all of the elements within set of zones.
    /// The zone set is comprised of the build area surrounding connected TCs.
    /// </summary>
    private class BuildSnapshot : Pool.IPooled
    {
        private AutoBuildSnapshot _plugin;
        private Action<bool, BuildSnapshot> _resultCallback;

        private Dictionary<BuildRecord, List<BaseEntity>> _buildingEntities;
        private List<PlayerMetaData> _authorizedPlayers;
        private List<BuildRecord> _linkedRecords;
        private Queue<BuildRecord> _stepRecordQueue;
        private BuildRecord _stepRecord;
        private Queue<BaseEntity> _stepEntityQueue;

        private Stopwatch _processWatch;
        private Stopwatch _stepWatch;

        /// <summary>
        /// The number of frames it took to process the snapshot.
        /// </summary>
        public int FrameCount { get; private set; }

        /// <summary>
        /// The maximum time that a step took during processing.
        /// </summary>
        public double LongestStepDuration { get; private set; }

        /// <summary>
        /// The exception, if any, that was thrown during processing.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// The list of records/tcs processed in this snapshot.
        /// </summary>
        public IReadOnlyList<BuildRecord> LinkedRecords => _linkedRecords;

        /// <summary>
        /// The time it took to process the snapshot, or the current running duration if not yet completed.
        /// </summary>
        public double Duration => _processWatch.Elapsed.TotalMilliseconds;

        /// <summary>
        /// The time it took to process the last step (frame), or the current step if not yet completed.
        /// </summary>
        public double StepDuration => _stepWatch.Elapsed.TotalMilliseconds;

        /// <summary>
        /// Controls whether the next step will be processed in a new frame.
        /// Duration is represented in fractional milliseconds.
        /// </summary>
        private bool NeedsFrame => StepDuration > _config.Advanced.MaxStepFrameDuration;

        /// <summary>
        /// Initializes a new build snapshot starting from the specified TC.
        /// This will gather all TC in radius as well.
        /// </summary>
        /// <param name="buildingPrivlidge"></param>
        public void Init(AutoBuildSnapshot plugin, BuildRecord record, Action<bool, BuildSnapshot> resultCallback)
        {
            _plugin = plugin;
            _resultCallback = resultCallback;

            AddLinkedRecord(record);
        }

        private void AddLinkedRecord(BuildRecord record)
        {
            if (_linkedRecords.Contains(record))
                return;

            _linkedRecords.Add(record);

            foreach (var user in record.BaseTC.authorizedPlayers)
            {
                if (_authorizedPlayers.Any(p => p.UserID == user.userid))
                    continue;

                _authorizedPlayers.Add(new PlayerMetaData
                {
                    UserID = user.userid,
                    UserName = user.username
                });
            }
        }

        /// <summary>
        /// Saves the current snapshot for the initialized record data, including all surrounding TCs/records.
        /// </summary>
        /// <param name="plugin">The base plugin reference.</param>
        /// <param name="resultCallback">The callback action when the save is complete, as this can span several frames.</param>
        public void BeginSave()
        {
            Action nextStep;
            BuildState nextState;

            if (_config.MultiTC.Enabled)
            {
                nextStep = BuildNetwork;
                nextState = BuildState.Save_BuildingNetwork;
            }
            else
            {
                nextStep = FindEntities;
                nextState = BuildState.Save_FindingEntities;
            }

            TryStep(() =>
            {
                if (_linkedRecords.Count == 0)
                {
                    throw new InvalidOperationException($"Snapshot can't be saved before initializing!");
                }

                if (++FrameCount > 1)
                {
                    throw new InvalidOperationException($"Cannot call save twice on snapshot!");
                }

                if (!ValidEntity(_linkedRecords[0].BaseTC))
                {
                    throw new InvalidOperationException("Base TC is invalid! Unable to perform snapshot.");
                }

                Update(nextState, _stepRecordQueue.Enqueue);
            },
            nextStep);
        }

        /// <summary>
        /// Recursively builds the network of linked TCs.
        /// </summary>
        private void BuildNetwork()
        {
            if (_stepRecordQueue.Count > 0)
            {
                TryStep(() =>
                {
                    var next = _stepRecordQueue.Dequeue();

                    foreach (var zone in next.LinkedZones)
                    {
                        var links = BuildingScanner.GetEntities<BuildingPrivlidge>(next.BaseTC, zone, _maskBaseEntities);
                        foreach (var link in links)
                        {
                            var recordId = link.net.ID.Value;

                            // if we're not tracking this tc, skip it
                            if (!_buildRecords.TryGetValue(recordId, out var record))
                                continue;

                            // if this record is already linked, skip it
                            if (_linkedRecords.Contains(record))
                                continue;

                            // if this record is already in the queue, skip it
                            if (_stepRecordQueue.Contains(record))
                                continue;

                            AddLinkedRecord(record);

                            _stepRecordQueue.Enqueue(record);
                        }
                    }
                },
                BuildNetwork);
            }
            else
            {
                // Network build complete, update states and begin finding entities
                TryStep(() =>
                {
                    Update(BuildState.Save_FindingEntities, _stepRecordQueue.Enqueue);
                },
                FindEntities);
            }
        }

        /// <summary>
        /// Finds all entities in the linked records.
        /// </summary>
        private void FindEntities()
        {
            if (_stepRecordQueue.Count > 0)
            {
                TryStep(() =>
                {
                    _stepRecord = _stepRecordQueue.Dequeue();
                    _buildingEntities.Add(_stepRecord, Pool.Get<List<BaseEntity>>());
                    foreach (var zone in _stepRecord.EntityZones)
                    {
                        var entities = BuildingScanner.GetEntities<BaseEntity>(_stepRecord.BaseTC, zone, _maskBaseEntities);
                        foreach (var entity in entities)
                        {
                            if (!_config.General.IncludeGroundResources && entity.OwnerID == 0 && entity is CollectibleEntity)
                                continue;

                            if (!_config.General.IncludeNonAuthorizedDeployables && !AuthorizedEntity(entity))
                                continue;

                            _stepEntityQueue.Enqueue(entity);
                        }
                    }
                },
                ProcessFoundEntities);
            }
            else
            {
                // Placeholder until final save step is implemented.
                TryStep(() =>
                {
                    Update(BuildState.Save_Writing);
                },
                FinishSave);
            }
        }

        private bool AuthorizedEntity(BaseEntity entity) =>
            _authorizedPlayers.Any(p => p.UserID == entity.OwnerID);

        /// <summary>
        /// Processes the found entities and adds them to the snapshot.
        /// </summary>
        private void ProcessFoundEntities()
        {
            if (_stepEntityQueue.Count > 0)
            {
                TryStep(() =>
                {
                    var next = _stepEntityQueue.Dequeue();
                    if (_buildingEntities[_stepRecord].Any(e => e.net.ID == next.net.ID))
                        return;

                    _buildingEntities[_stepRecord].Add(next);
                },
                ProcessFoundEntities);
            }
            else
            {
                FindEntities();
            }
        }

        /// <summary>
        /// Finishes the save process and writes the snapshot to disk (or other storage).
        /// </summary>
        private void FinishSave()
        {
            TryStep(() =>
            {
                var now = DateTime.UtcNow;
                var snapshotId = Guid.NewGuid();
                var snapshotPrefix = $"{now:hhmmss}_{snapshotId}";
                var dataFile = $"{_snapshotDataDirectory}/{snapshotPrefix}.{_snapshotDataExtension}";
                var metaFile = $"{_snapshotDataDirectory}/{snapshotPrefix}.{_snapshotMetaExtension}";

                WriteSnapshotMeta(now, snapshotId, dataFile, metaFile);
                WriteSnapshotData(now, snapshotId, dataFile, metaFile);

                Update(BuildState.Save_Success);
            },
            () => _resultCallback(true, this));
        }

        /// <summary>
        /// Writes the snapshot metadata to disk (or other storage).
        /// </summary>
        /// <param name="now">The current time.</param>
        /// <param name="snapshotId">The unique id of the snapshot.</param>
        /// <param name="dataFile">The file to write the data to.</param>
        /// <param name="metaFile">The file to write the metadata to.</param>
        private void WriteSnapshotMeta(DateTime now, Guid snapshotId, string dataFile, string metaFile)
        {
            var linkedBuildingsMeta = Pool.Get<Dictionary<string, BuildingMetaData>>();
            try
            {
                foreach (var kvp in _buildingEntities)
                {
                    linkedBuildingsMeta.Add(kvp.Key.PersistentID, new BuildingMetaData
                    {
                        Position = kvp.Key.BaseTC.ServerPosition,
                        Entities = kvp.Value.Count,
                        Zones = kvp.Key.EntityZones.Count,
                    });
                }

                var metaData = new BuildSnapshotMetaData
                {
                    ID = snapshotId,
                    DataFile = dataFile,
                    TimestampUTC = now,
                    Entities = _buildingEntities.Count,
                    LinkedBuildings = linkedBuildingsMeta,
                    AuthorizedPlayers = _authorizedPlayers,
                };

                Interface.Oxide.DataFileSystem.WriteObject(metaFile, metaData);
            }
            finally
            {
                Pool.FreeUnmanaged(ref linkedBuildingsMeta);
            }
        }

        /// <summary>
        /// Writes the snapshot data to disk (or other storage).
        /// </summary>
        /// <param name="now">The current time.</param>
        /// <param name="snapshotId">The unique id of the snapshot.</param>
        /// <param name="dataFile">The file to write the data to.</param>
        /// <param name="metaFile">The file to write the metadata to.</param>
        private void WriteSnapshotData(DateTime now, Guid snapshotId, string dataFile, string metaFile)
        {
            var buildingEntities = Pool.Get<Dictionary<string, List<PersistantEntity>>>();
            var zones = Pool.Get<List<Vector4>>();
            try
            {
                foreach (var buildingId in _buildingEntities.Keys)
                {
                    var entities = Pool.Get<List<PersistantEntity>>();
                    entities.AddRange(_buildingEntities[buildingId]
                        .Select(e => (PersistantEntity)e)
                        .OrderBy(e => e.PrefabName)
                        .OrderBy(e => e.Type));
                    buildingEntities.Add(buildingId.PersistentID, entities);
                }

                zones.AddRange(_linkedRecords.SelectMany(record => record.EntityZones).Distinct());

                var snapshotData = new SnapshotData
                {
                    ID = snapshotId,
                    MetaDataFile = metaFile,
                    Timestamp = now,
                    Entities = buildingEntities,
                    Zones = zones,
                };

                Interface.Oxide.DataFileSystem.WriteObject(dataFile, snapshotData);
            }
            finally
            {
                FreeDictionaryList(ref buildingEntities);
                Pool.FreeUnmanaged(ref zones);
            }
        }

        /// <summary>
        /// Attempts to perform a step in the process.
        /// Decides to run the step immediately or on the next frame depending on <see cref="NeedsFrame"/>.
        /// This is calculated based off of a preset duration between frames.
        /// </summary>
        /// <param name="stepAction">The action to perform.</param>
        /// <param name="successCallback">The callback to invoke on success.</param>
        private void TryStep(
            Action stepAction,
            Action successCallback
        )
        {
            // Persist all entities
            try
            {
                _processWatch.Start();
                _stepWatch.Restart();

                stepAction();

                _stepWatch.Stop();
                _processWatch.Stop();

                // Track the longest step duration
                if (StepDuration > LongestStepDuration)
                {
                    LongestStepDuration = StepDuration;
                }

                if (NeedsFrame)
                {
                    FrameCount++;
                    _plugin.NextFrame(successCallback);
                }
                else
                {
                    successCallback();
                }
            }
            catch (Exception ex)
            {
                foreach (var record in _linkedRecords)
                {
                    record.Update(ex);
                }

                Exception = ex;

                // on failure, call the primary result handler immediately
                _resultCallback(false, this);
            }
        }

        private void Update(BuildState state, Action<BuildRecord> updateAction = null)
        {
            foreach (var record in _linkedRecords)
            {
                record.Update(state);

                updateAction?.Invoke(record);
            }
        }

        /// <summary>
        /// Enters the pool and frees any unmanaged resources.
        /// </summary>
        public void EnterPool()
        {
            foreach (var recordId in _buildingEntities.Keys)
            {
                var recordEntities = _buildingEntities[recordId];
                if (recordEntities != null)
                {
                    Pool.FreeUnmanaged(ref recordEntities);
                    _buildingEntities[recordId] = null;
                }
            }

            Pool.FreeUnmanaged(ref _buildingEntities);
            Pool.FreeUnmanaged(ref _authorizedPlayers);
            Pool.FreeUnmanaged(ref _linkedRecords);
            Pool.FreeUnmanaged(ref _stepRecordQueue);
            Pool.FreeUnmanaged(ref _stepEntityQueue);

            _processWatch.Reset();
            _stepWatch.Reset();

            FrameCount = 0;
            LongestStepDuration = 0;
            Exception = null;
        }

        /// <summary>
        /// Leaves the pool and allocates any unmanaged resources.
        /// </summary>
        public void LeavePool()
        {
            _buildingEntities = Pool.Get<Dictionary<BuildRecord, List<BaseEntity>>>();
            _authorizedPlayers = Pool.Get<List<PlayerMetaData>>();
            _linkedRecords = Pool.Get<List<BuildRecord>>();
            _stepRecordQueue = Pool.Get<Queue<BuildRecord>>();
            _stepEntityQueue = Pool.Get<Queue<BaseEntity>>();

            _processWatch ??= new();
            _stepWatch ??= new();
        }
    }

    /// <summary>
    /// The metadata that is created when a snapshot is saved.
    /// Primarily used to index current backups.
    /// </summary>
    private readonly struct BuildSnapshotMetaData
    {
        /// <summary>
        /// The unique id of the snapshot.
        /// </summary>
        public required Guid ID { get; init; }

        /// <summary>
        /// The namne of the matching data file.
        /// Should be the same as this file, but with a ".data" extension.
        /// </summary>
        public required string DataFile { get; init; }

        /// <summary>
        /// The time the snapshot was created.
        /// </summary>
        public required DateTime TimestampUTC { get; init; }

        /// <summary>
        /// The number of entities in the snapshot.
        /// </summary>
        public required int Entities { get; init; }

        /// <summary>
        /// The linked buildings in the snapshot.
        /// </summary>
        public required Dictionary<string, BuildingMetaData> LinkedBuildings { get; init; }

        /// <summary>
        /// The list of players that are authorized in any of the linked buildings in the snapshot.
        /// </summary>
        public required List<PlayerMetaData> AuthorizedPlayers { get; init; }
    }

    /// <summary>
    /// The metadata for a building in the snapshot.
    /// </summary>
    private readonly struct BuildingMetaData
    {
        /// <summary>
        /// The position of the building TC.
        /// </summary>
        public Vector3 Position { get; init; }

        /// <summary>
        /// The number of entities attached to this building.
        /// </summary>
        public int Entities { get; init; }

        /// <summary>
        /// The number of zones this building is comprised of.
        /// </summary>
        public int Zones { get; init; }
    }

    /// <summary>
    /// The simplified player metadata.
    /// </summary>
    private readonly struct PlayerMetaData
    {
        /// <summary>
        /// The id of the player.
        /// </summary>
        public ulong UserID { get; init; }

        /// <summary>
        /// The display name of the player.
        /// </summary>
        public string UserName { get; init; }
    }

    private readonly struct SnapshotData
    {
        /// <summary>
        /// The unique id of the snapshot.
        /// </summary>
        public required Guid ID { get; init; }

        /// <summary>
        /// The namne of the matching metadata file.
        /// Should be the same as this file, but with a ".meta" extension.
        /// </summary>
        public required string MetaDataFile { get; init; }

        /// <summary>
        /// The time the snapshot was created.
        /// </summary>
        public required DateTime Timestamp { get; init; }

        /// <summary>
        /// The entities in this base, grouped by building.
        /// </summary>
        public required Dictionary<string, List<PersistantEntity>> Entities { get; init; }

        /// <summary>
        /// The zones this base is comprised of.
        /// </summary>
        public required List<Vector4> Zones { get; init; }
    }

    #endregion

    #region Persistance

    /// <summary>
    /// Represents a persistent entity, which just uses the prefabID and coords.
    /// </summary>
    private readonly struct PersistantEntity
    {
        /// <summary>
        /// A null entity that can be used to represent an invalid or non-existent entity.
        /// </summary>
        private static PersistantEntity Null { get; } = new()
        {
            PrefabID = 0,
            Position = Vector3.zero
        };

        /// <summary>
        /// Creates a new persistent entity from the specified entity.
        /// </summary>
        /// <param name="entity">The entity to create the persistent entity from.</param>
        public PersistantEntity(BaseEntity entity)
        {
            Type = entity.GetType().Name;
            PrefabName = entity.PrefabName;
            PrefabID = entity.prefabID;
            OwnerID = entity.OwnerID;
            Position = entity.ServerPosition;
            Rotation = entity.ServerRotation;

            Properties = new();

            if (entity is BuildingBlock block)
            {
                Properties["Grade"] = block.grade;
            }

            if (entity is StorageContainer storage)
            {
                var itemContainers = Pool.Get<List<ItemContainer>>();

                storage.GetAllInventories(itemContainers);
                Properties["Items"] = itemContainers
                    .SelectMany(container => container.itemList
                    .Select(item => new PersistantItem(item)))
                    .ToArray();

                Pool.Free(ref itemContainers);
            }

            if (entity is DecayEntity decay)
            {
                Properties["Health"] = decay.health;
                Properties["HealthFraction"] = decay.healthFraction;
            }

            if (entity.HasAnySlot())
            {

            }

            if (entity.skinID > 0)
            {
                Properties["SkinID"] = entity.skinID;
            }
        }

        /// <summary>
        /// Generates a unique ID for the entity based on its prefab and position.
        /// </summary>
        [JsonIgnore]
        public string ID => IsNull ? "null" : GetPersistanceID(Type, Position);

        /// <summary>
        /// Checks if the entity is null (i.e. has a prefab ID of 0).
        /// </summary>
        [JsonIgnore]
        public bool IsNull => PrefabID == 0;

        /// <summary>
        /// The type of this entity.
        /// </summary>
        public string Type { get; init; }

        /// <summary>
        /// The prefab name of the entity.
        /// </summary>
        public string PrefabName { get; init; }

        /// <summary>
        /// The prefab ID of the entity.
        /// </summary>
        public uint PrefabID { get; init; }

        /// <summary>
        /// The ID of the entity's owner.
        /// </summary>
        public ulong OwnerID { get; init; }

        /// <summary>
        /// The position of the entity.
        /// </summary>
        public Vector3 Position { get; init; }

        /// <summary>
        /// The rotation of the entity.
        /// </summary>
        public Quaternion Rotation { get; init; }

        /// <summary>
        /// The object properties.
        /// </summary>
        public Dictionary<string, object> Properties { get; init; }

        /// <summary>
        /// Implicitly converts a <see cref="BaseEntity"/> to a <see cref="PersistantEntity"/>.
        /// </summary>
        /// <param name="entity"></param>
        public static implicit operator PersistantEntity(BaseEntity entity)
        {
            if (!ValidEntity(entity))
            {
                return Null;
            }

            return new(entity);
        }
    }

    /// <summary>
    /// Represents a persistent item, which is a simplified version of the Base Item.
    /// </summary>
    private readonly struct PersistantItem
    {
        public PersistantItem(Item item)
        {
            UID = item.uid.Value;
            ItemID = item.info.itemid;
            Tag = item.info.tag;
            Amount = item.amount;
            Flags = item.flags;

            Properties = new();

            if (item.contents != null)
            {
                Properties["Items"] = item.contents.itemList?
                    .Select(i => new PersistantItem(i))
                    .ToArray()
                    ?? Array.Empty<PersistantItem>();
            }

            if (item.fuel > 0)
            {
                Properties["Fuel"] = item.fuel;
            }

            if (item.skin > 0)
            {
                Properties["SkinID"] = item.skin;
            }
        }

        public ulong UID { get; init; }

        public int ItemID { get; init; }

        public string Tag { get; init; }

        public int Amount { get; init; }

        public Item.Flag Flags { get; init; }

        public Dictionary<string, object> Properties { get; init; }
    }

    #endregion

}
