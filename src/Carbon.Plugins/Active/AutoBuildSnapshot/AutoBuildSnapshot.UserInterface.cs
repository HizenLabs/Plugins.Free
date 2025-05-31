using Carbon.Base;
using Carbon.Components;
using Carbon.Modules;
using Facepunch;
using HizenLabs.Extensions.UserPreference.Data;
using HizenLabs.Extensions.UserPreference.Material.API;
using HizenLabs.Extensions.UserPreference.UI;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Carbon.Components.CUI.Handler;
using static GameTip;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Handles all user interface functionality for the plugin.
    /// </summary>
    private static class UserInterface
    {
        #region Fields

        private const string BlurAsset = "assets/content/ui/uibackgroundblur.mat";
        public const string MainMenuId = "abs.mainmenu";
        public const string ConfirmMenuId = "abs.confirmation";

        private const int ItemCameraId = -2001260025;
        private const int ItemToolCupboardId = -97956382;

        private const string mdIconBaseUrl = "https://raw.githubusercontent.com/google/material-design-icons/refs/heads/master/png";

        private const string IconCloseId = "md_close";
        private const string IconCloseUrl = $"{mdIconBaseUrl}/navigation/close/materialicons/48dp/2x/baseline_close_black_48dp.png";

        private const string IconDeleteId = "md_delete";
        private const string IconDeleteUrl = $"{mdIconBaseUrl}/action/delete/materialicons/48dp/2x/baseline_delete_black_48dp.png";

        private const string IconDisplaySettingsId = "md_display_settings";
        private const string IconDisplaySettingsUrl = $"{mdIconBaseUrl}/action/display_settings/materialicons/48dp/2x/baseline_display_settings_black_48dp.png";

        #endregion

        #region API

        /// <summary>
        /// Initializes the user interface for the plugin.
        /// </summary>
        public static void Init()
        {
            var imageDb = BaseModule.GetModule<ImageDatabaseModule>();

            imageDb.Queue(IconCloseId, IconCloseUrl);
            imageDb.Queue(IconDeleteId, IconDeleteUrl);
            imageDb.Queue(IconDisplaySettingsId, IconDisplaySettingsUrl);
        }

        /// <summary>
        /// Unloads the user interface for the plugin.
        /// </summary>
        public static void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                HideMenu(player);
            }
        }

        /// <summary>
        /// Handles player disconnection events.
        /// </summary>
        /// <param name="player">The player that disconnected.</param>
        public static void HandleDisconnect(BasePlayer player)
        {
            HideMenu(player);
        }

        /// <summary>
        /// Toggles the main menu for the user.
        /// </summary>
        /// <param name="player">The player to toggle the menu for.</param>
        public static void ToggleMenu(BasePlayer player)
        {
            if (!player) return;

            if (player.AutoBuildSnapshot_MenuState)
            {
                HideMenu(player);
            }
            else
            {
                ShowMenu(player);
            }
        }

        public static void SwitchTab(BasePlayer player, int targetIndex = 0)
        {
            if (!player) return;

            player.AutoBuildSnapshot_TabIndex = targetIndex;

            ShowMenu(player);
        }

        /// <summary>
        /// Closes the main menu for the user.
        /// </summary>
        /// <param name="player">The player to close the menu for.</param>
        public static void HideMenu(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, MainMenuId);
            CuiHelper.DestroyUi(player, ConfirmMenuId);

            // Free record options
            if (player.AutoBuildSnapshot_RecordOptions is List<ChangeManagement.RecordingId> recordingIds)
            {
                Pool.FreeUnmanaged(ref recordingIds);
                player.AutoBuildSnapshot_RecordOptions = null;
            }

            // Free snapshot options
            if (player.AutoBuildSnapshot_SnapshotOptions is List<MetaInfo> snapshotMetas)
            {
                Pool.FreeUnmanaged(ref snapshotMetas);
                player.AutoBuildSnapshot_SnapshotOptions = null;
            }

            // Reset the menu
            player.AutoBuildSnapshot_MenuState = false;
            player.AutoBuildSnapshot_OnConfirm = null;
            player.AutoBuildSnapshot_SelectedRecordIndex = 0;
            player.AutoBuildSnapshot_TabIndex = 0;
        }

        /// <summary>
        /// Shows the main menu for the user.
        /// </summary>
        /// <param name="player">The player to show the menu to.</param>
        public static void ShowMenu(BasePlayer player)
        {
            using var cui = _instance.CreateCUI();
            Render(cui, player);

            cui.v2.SendUi(player);

            player.AutoBuildSnapshot_MenuState = true;
        }

        #endregion

        #region Render

        #region Main Container

        /// <summary>
        /// Renders the main menu for the user.
        /// </summary>
        /// <param name="cui">The CUI instance to use for rendering.</param>
        /// <param name="player">The player to render for.</param>
        private static void Render(CUI cui, BasePlayer player)
        {
            const int headFontSize = 16;
            const FontTypes headFontType = FontTypes.RobotoCondensedBold;

            var userPreference = UserPreferenceData.Load(_instance, player);
            var theme = userPreference.Theme;

            var headerText = Localizer.Text(LangKeys.menu_title, player, _instance.Version);

            var tabIndex = player.AutoBuildSnapshot_TabIndex;

            var parent = cui.v2
                .CreateParent(
                    parent: CUI.ClientPanels.HudMenu,
                    position: LuiPosition.Full,
                    name: MainMenuId)
                .AddCursor()
                .SetDestroy(MainMenuId);

            var outline = cui.v2
                .CreatePanel(
                    container: parent,
                    position: LuiPosition.MiddleCenter,
                    offset: new(-451, -275.65f, 451, 276),
                    color: theme.Outline
                );

            var container = cui.v2
                .CreatePanel(
                    container: parent,
                    position: LuiPosition.MiddleCenter,
                    offset: new(-450, -275, 450, 275),
                    color: theme.Background
                );


            var headerHeight = 0.08f;
            var headerStartY = 1 - headerHeight;
            var header = cui.v2
                .CreatePanel(
                    container: container,
                    position: new(0, headerStartY, 1, 1),
                    offset: LuiOffset.None,
                    color: theme.PrimaryContainer
                );

            cui.v2.CreateText(
                    container: header,
                    position: new(.015f, 0, .5f, 1),
                    offset: new(1, 1, 0, 0),
                    color: theme.OnPrimaryContainer,
                    fontSize: headFontSize,
                    text: headerText,
                    alignment: TextAnchor.MiddleLeft
                )
                .SetTextFont(headFontType);

            var headerButtons = cui.v2
                .CreatePanel(
                    container: header,
                    position: new(.5f, 0, 1, 1),
                    offset: new(0, 5, -5, -5),
                    color: theme.Transparent
                );

            var closeButtonWidth = 0.08f;
            var closeButtonX = 1 - closeButtonWidth;
            var closeButton = CreateButton(cui, player, userPreference,
                container: headerButtons,
                position: new(closeButtonX, 0, 1, 1),
                offset: LuiOffset.None,
                color: theme.PrimaryContainer,
                textColor: theme.OnPrimaryContainer,
                textKey: LangKeys.StringEmpty,
                commandName: nameof(CommandMenuClose),
                fontSize: 14
            );

            cui.v2
                .CreateImageFromDb(
                    container: closeButton,
                    position: LuiPosition.MiddleCenter,
                    offset: new(-14, -14, 14, 14),
                    dbName: IconCloseId,
                    color: theme.OnPrimaryContainer);

            var optionsButtonWidth = 0.08f;
            var optionsButtonX = closeButtonX - optionsButtonWidth;
            var optionsButton = CreateButton(cui, player, userPreference,
                container: headerButtons,
                position: new(optionsButtonX - .005f, 0, closeButtonX - .005f, 1),
                offset: LuiOffset.None,
                color: theme.PrimaryContainer,
                textColor: theme.OnPrimaryContainer,
                textKey: LangKeys.StringEmpty,
                commandName: nameof(CommandMenuSettings),
                fontSize: 14
            );

            cui.v2
                .CreateImageFromDb(
                    container: optionsButton,
                    position: LuiPosition.MiddleCenter,
                    offset: new(-13, -13, 13, 13),
                    dbName: IconDisplaySettingsId,
                    color: theme.OnPrimaryContainer);

            var content = cui.v2
                .CreatePanel(
                    container: container,
                    position: new(0, 0, 1, headerStartY),
                    offset: LuiOffset.None,
                    color: theme.Transparent
                );

            // watermark
            cui.v2
                .CreateText(
                    container: content,
                    position: LuiPosition.Full,
                    offset: new(0, 1, 0, 0),
                    fontSize: 8,
                    color: "0.5 0.5 0.5 0.4",
                    text: $"AutoBuildSnapshot v{_instance.Version} by hizenxyz",
                    alignment: TextAnchor.LowerCenter)
                .SetTextFont(CUI.Handler.FontTypes.DroidSansMono);

            var tabButtonsHeight = 0.1f;
            var tabButtonsStartY = 1 - tabButtonsHeight;
            var tabButtons = cui.v2
                .CreatePanel(
                    container: content,
                    position: new(0, tabButtonsStartY, 1, 1),
                    offset: new(12, 12, -12, -12),
                    color: theme.Transparent
                );

            var contentButtonWidth = 0.12f;
            var homeButtonX = 0f;
            CreateButton(cui, player, userPreference,
                container: tabButtons,
                position: new(homeButtonX, 0, contentButtonWidth, 1),
                offset: LuiOffset.None,
                color: GetTabButtonColor(tabIndex, 0, theme),
                textColor: GetTabButtonTextColor(tabIndex, 0, theme),
                textKey: LangKeys.menu_tab_home,
                commandName: nameof(CommandMenuSwitchTab),
                commandArgs: "0"
            );

            var logsButtonX = contentButtonWidth;
            CreateButton(cui, player, userPreference,
                container: tabButtons,
                position: new(logsButtonX, 0, contentButtonWidth + logsButtonX, 1),
                offset: new(5, 0, 0, 0),
                color: GetTabButtonColor(tabIndex, 1, theme),
                textColor: GetTabButtonTextColor(tabIndex, 1, theme),
                textKey: LangKeys.menu_tab_logs,
                commandName: nameof(CommandMenuSwitchTab),
                commandArgs: "1"
            );

            var tabContent = cui.v2
                .CreatePanel(
                    container: content,
                    position: new(0, 0, 1, tabButtonsStartY),
                    offset: LuiOffset.None,
                    color: theme.Transparent
                );

            switch (tabIndex)
            {
                case 1:
                    RenderLogsTab(cui, player, tabButtons, tabContent, userPreference);
                    break;

                case 0:
                default:
                    RenderHomeTab(cui, player, tabButtons, tabContent, userPreference);
                    break;
            }
        }

        #endregion

        #region Home

        #region Home Container

        private static void RenderHomeTab(
            CUI cui,
            BasePlayer player,
            LUI.LuiContainer tabButtons,
            LUI.LuiContainer tabContent,
            UserPreferenceData userPreference)
        {
            var theme = userPreference.Theme;

            var recordOptionsContainer = cui.v2
                .CreatePanel(tabContent,
                    position: new(0, 0, .26f, 1),
                    offset: new(12, 12, -12, 0),
                    color: theme.SurfaceContainer
                );

            var snapshotOptionsContainer = cui.v2
                .CreatePanel(tabContent,
                    position: new(.26f, 0, .5f, 1),
                    offset: new(0, 12, -12, 0),
                    color: theme.SurfaceContainer
                );

            var snapshotDetailsContainer = cui.v2
                .CreatePanel(tabContent,
                    position: new(.5f, 0, 1, 1),
                    offset: new(0, 12, -12, 0),
                    color: theme.SurfaceContainer
                );

            if (player.AutoBuildSnapshot_RecordOptions is List<ChangeManagement.RecordingId> recordingIds)
            {
                Pool.FreeUnmanaged(ref recordingIds);
            }

            recordingIds = Pool.Get<List<ChangeManagement.RecordingId>>();
            SaveManager.GetRecordingsByDistanceTo(player.ServerPosition, recordingIds);

            player.AutoBuildSnapshot_RecordOptions = recordingIds;

            if (player.AutoBuildSnapshot_SelectedRecordIndex >= recordingIds.Count)
            {
                player.AutoBuildSnapshot_SelectedRecordIndex = 0;
            }

            if (recordingIds.Count > 0)
            {
                var selectedRecord = recordingIds[player.AutoBuildSnapshot_SelectedRecordIndex];

                RenderRecordOptions(cui, recordOptionsContainer, player, recordingIds, userPreference);

                RenderRecordButtons(cui, tabButtons, player, recordingIds, userPreference);

                if (player.AutoBuildSnapshot_SnapshotOptions is List<MetaInfo> snapshotMetas)
                {
                    Pool.FreeUnmanaged(ref snapshotMetas);
                }

                snapshotMetas = Pool.Get<List<MetaInfo>>();
                SaveManager.GetSaves(selectedRecord, snapshotMetas);
                player.AutoBuildSnapshot_SnapshotOptions = snapshotMetas;

                if (snapshotMetas.Count > 0)
                {
                    RenderSnapshotOptions(cui, snapshotOptionsContainer, player, snapshotMetas, userPreference);

                    if (player.AutoBuildSnapshot_SelectedSnapshotIndex >= snapshotMetas.Count)
                    {
                        player.AutoBuildSnapshot_SelectedSnapshotIndex = 0;
                    }

                    var snapshotMetaInfo = snapshotMetas[player.AutoBuildSnapshot_SelectedSnapshotIndex];
                    RenderSnapshotDetails(cui, snapshotDetailsContainer, player, snapshotMetaInfo, userPreference);
                }
                else
                {
                    RenderEmpty(cui, snapshotOptionsContainer, player, userPreference, LangKeys.menu_no_snapshots_found);
                    RenderEmpty(cui, snapshotDetailsContainer, player, userPreference, LangKeys.menu_no_snapshot_selected);
                }
            }
            else
            {
                RenderEmpty(cui, recordOptionsContainer, player, userPreference, LangKeys.menu_no_records_found);
                RenderEmpty(cui, snapshotOptionsContainer, player, userPreference, LangKeys.menu_no_snapshots_found);
                RenderEmpty(cui, snapshotDetailsContainer, player, userPreference, LangKeys.menu_no_snapshot_selected);
            }
        }

        #endregion

        #region Record Buttons

        private static void RenderRecordButtons(
            CUI cui,
            LUI.LuiContainer tabButtons,
            BasePlayer player,
            List<ChangeManagement.RecordingId> recordingIds,
            UserPreferenceData userPreference)
        {
            var theme = userPreference.Theme;
            var buttonIndex = 0;
            var buttonWidth = 0.12f;

            var recordId = recordingIds[player.AutoBuildSnapshot_SelectedRecordIndex];
            var isActive = ChangeManagement.Recordings.TryGetValue(recordId, out var recording) && recording.IsActive;

            if (Settings.Commands.Rollback.HasPermission(player))
            {
                var offsetX = 0.005f * buttonIndex;
                var buttonXMax = 1 - buttonIndex * buttonWidth - offsetX;
                var buttonXMin = 1 - ++buttonIndex * buttonWidth - offsetX;
                CreateButton(cui, player, userPreference,
                    container: tabButtons,
                    position: new(buttonXMin, 0, buttonXMax, 1),
                    offset: LuiOffset.None,
                    color: theme.TertiaryContainer,
                    textColor: theme.OnTertiaryContainer,
                    textKey: LangKeys.menu_content_rollback,
                    commandName: nameof(CommandMenuRollback)
                );
            }

            if (isActive && Settings.Commands.Backup.HasPermission(player))
            {
                var offsetX = 0.005f * buttonIndex;
                var buttonXMax = 1 - buttonIndex * buttonWidth - offsetX;
                var buttonXMin = 1 - ++buttonIndex * buttonWidth - offsetX;
                CreateButton(cui, player, userPreference,
                    container: tabButtons,
                    position: new(buttonXMin, 0, buttonXMax, 1),
                    offset: LuiOffset.None,
                    color: theme.SecondaryContainer,
                    textColor: theme.OnSecondaryContainer,
                    textKey: LangKeys.menu_content_backup,
                    commandName: nameof(CommandMenuBackup)
                );
            }

            if (Settings.Commands.HasAdminPermission(player))
            {
                var offsetX = 0.005f * buttonIndex;
                var buttonXMax = 1 - buttonIndex * buttonWidth - offsetX;
                var buttonXMin = 1 - ++buttonIndex * buttonWidth - offsetX;
                CreateButton(cui, player, userPreference,
                    container: tabButtons,
                    position: new(buttonXMin, 0, buttonXMax, 1),
                    offset: LuiOffset.None,
                    color: theme.PrimaryContainer,
                    textColor: theme.OnPrimaryContainer,
                    textKey: LangKeys.menu_content_teleport,
                    commandName: nameof(CommandMenuTeleport)
                );
            }
        }

        #endregion

        #region Record Options

        private static void RenderRecordOptions(
            CUI cui,
            LUI.LuiContainer recordOptionsContainer,
            BasePlayer player,
            List<ChangeManagement.RecordingId> recordingIds,
            UserPreferenceData userPreference)
        {
            var theme = userPreference.Theme;

            int panelHeight = 45;
            int panelWidth = 205;

            RenderOptionsList(
                cui: cui,
                container: recordOptionsContainer,
                player: player,
                userPreference: userPreference,
                height: panelHeight,
                width: panelWidth,
                onSelectCommand: nameof(CommandMenuSelectRecord),
                options: recordingIds,
                isSelectedFunc: index => index == player.AutoBuildSnapshot_SelectedRecordIndex,
                template: (button, index, recordId, isSelected) =>
                {
                    cui.v2
                        .CreateItemIcon(
                            container: button,
                            position: LuiPosition.LowerLeft,
                            offset: new(2, 2, panelHeight - 7, panelHeight - 7),
                            itemId: ItemToolCupboardId
                        );

                    if (!isSelected)
                    {
                        var iconMaskColor = theme.SecondaryContainer.WithOpacity(.7f);


                        cui.v2
                            .CreateItemIcon(
                                container: button,
                                position: LuiPosition.LowerLeft,
                                offset: new(2, 2, panelHeight - 7, panelHeight - 7),
                                itemId: ItemToolCupboardId,
                                color: iconMaskColor);
                    }

                    var isActive = ChangeManagement.Recordings.TryGetValue(recordId, out var recording) && recording.IsActive;
                    if (!isActive)
                    {
                        cui.v2
                            .CreateImageFromDb(
                                container: button,
                                position: LuiPosition.LowerLeft,
                                offset: new(2, 2, 20, 20),
                                dbName: IconDeleteId,
                                color: theme.Background.WithOpacity(.95f));
                    }

                    float posX = recordingIds[index].Position.x;
                    float posY = recordingIds[index].Position.y;
                    float posZ = recordingIds[index].Position.z;

                    cui.v2
                        .CreateText(
                            container: button,
                            position: LuiPosition.LowerLeft,
                            offset: new(panelHeight, 0, panelWidth, panelHeight - 8),
                            fontSize: 8,
                            color: theme.OnSecondaryContainer,
                            text: $"X: {posX}\nY: {posY}\nZ: {posZ}",
                            alignment: TextAnchor.UpperLeft
                        )
                        .SetTextFont(FontTypes.DroidSansMono);

                    var lastSave = SaveManager.GetLastSave(recordId);
                    cui.v2
                        .CreateText(
                            container: button,
                            position: LuiPosition.LowerLeft,
                            offset: new(panelHeight * 2 + 20, 0, panelWidth, panelHeight - 8),
                            fontSize: 8,
                            color: theme.OnSecondaryContainer,
                            text: $"Entities: {lastSave.OriginalEntityCount}\nBlocks: {lastSave.ZoneCount}\nAuth: {lastSave.AuthorizedCount}",
                            alignment: TextAnchor.UpperLeft
                        )
                        .SetTextFont(FontTypes.DroidSansMono);
                }
            );
        }

        #endregion

        #region Snapshot Options

        private static void RenderSnapshotOptions(
            CUI cui,
            LUI.LuiContainer snapshotOptionContainer,
            BasePlayer player,
            List<MetaInfo> snapshotMetas,
            UserPreferenceData userPreference)
        {
            var theme = userPreference.Theme;

            int panelHeight = 34;
            int panelWidth = 200;

            RenderOptionsList(
                cui: cui,
                container: snapshotOptionContainer,
                player: player,
                userPreference: userPreference,
                height: panelHeight,
                width: panelWidth,
                onSelectCommand: nameof(CommandMenuSelectSnapshot),
                options: snapshotMetas,
                isSelectedFunc: index => index == player.AutoBuildSnapshot_SelectedSnapshotIndex,
                template: (button, index, meta, isSelected) =>
                {
                    cui.v2
                        .CreateItemIcon(
                            container: button,
                            position: LuiPosition.LowerLeft,
                            offset: new(2, 2, panelHeight - 7, panelHeight - 7),
                            itemId: ItemCameraId
                        );

                    if (!isSelected)
                    {
                        var iconMaskColor = theme.SecondaryContainer.WithOpacity(.7f);


                        cui.v2
                            .CreateItemIcon(
                                container: button,
                                position: LuiPosition.LowerLeft,
                                offset: new(2, 2, panelHeight - 7, panelHeight - 7),
                                itemId: ItemCameraId,
                                color: iconMaskColor);
                    }

                    cui.v2
                        .CreateText(
                            container: button,
                            position: LuiPosition.LowerLeft,
                            offset: new(panelHeight, 0, panelWidth, panelHeight - 8),
                            fontSize: 8,
                            color: theme.OnSecondaryContainer,
                            text: $"Time: {meta.TimeStamp}\nEntities: {meta.OriginalEntityCount} | Blocks: {meta.ZoneCount}",
                            alignment: TextAnchor.UpperLeft
                        )
                        .SetTextFont(FontTypes.DroidSansMono);
                }
            );
        }

        #endregion

        #region Snapshot Details

        private static void RenderSnapshotDetails(
            CUI cui,
            LUI.LuiContainer container,
            BasePlayer player,
            MetaInfo meta,
            UserPreferenceData userPreference)
        {
            int line = 0;
            RenderInfoLine(cui, container, userPreference, Localizer.Text(LangKeys.info_snapshotid, player, meta.Id), line++);
            RenderInfoLine(cui, container, userPreference, Localizer.Text(LangKeys.info_position, player, meta.OriginPosition), line++);
            RenderInfoLine(cui, container, userPreference, Localizer.Text(LangKeys.info_timestamp, player, meta.TimeStamp), line++);
            RenderInfoLine(cui, container, userPreference, Localizer.Text(LangKeys.info_entities, player, meta.OriginalEntityCount), line++);
            RenderInfoLine(cui, container, userPreference, Localizer.Text(LangKeys.info_buildingblocks, player, meta.ZoneCount), line++);
            RenderInfoLine(cui, container, userPreference, Localizer.Text(LangKeys.info_authorized, player, meta.AuthorizedCount), line++);
        }

        #endregion

        #endregion

        #region Logs

        private static void RenderLogsTab(
            CUI cui,
            BasePlayer player,
            LUI.LuiContainer tabButtons,
            LUI.LuiContainer tabContent,
            UserPreferenceData userPreference)
        {
            var theme = userPreference.Theme;

            if (Settings.Commands.HasAdminPermission(player))
            {
                var clearButtonWidth = 0.12f;
                CreateButton(cui, player, userPreference,
                    container: tabButtons,
                    position: new(1 - clearButtonWidth, 0, 1, 1),
                    offset: LuiOffset.None,
                    color: theme.PrimaryContainer,
                    textColor: theme.OnPrimaryContainer,
                    textKey: LangKeys.menu_content_clear,
                    commandName: nameof(CommandMenuClearLogs)
                );
            }

            var logsPanel = cui.v2
                .CreatePanel(tabContent,
                    position: new(0, 0, 1, 1),
                    offset: new(12, 12, -12, 0),
                    color: theme.Surface
                );

            using var logs = Helpers.GetLogs(player);

            const int lineHeight = 16;
            const int lineWidth = 888;

            var logsScrollView = cui.v2
                .CreateScrollView(logsPanel,
                    position: LuiPosition.Full,
                    offset: new(-5, 10, -5, -5),
                    vertical: true,
                    horizontal: false,
                    movementType: ScrollRect.MovementType.Elastic,
                    elasticity: 0,
                    inertia: true,
                    decelerationRate: 0.1f,
                    scrollSensitivity: 10,
                    /*
                    horizontalScrollOptions: new()
                    {
                        size = 6,
                        autoHide = true,
                        handleColor = theme.Primary
                    },
                    */
                    verticalScrollOptions: new()
                    {
                        size = 6,
                        autoHide = true,
                        handleColor = theme.Primary
                    });

            const int maxVisible = 500;
            var logsVisible = Math.Min(logs.Count, maxVisible);
            var scrollY = -lineHeight * logsVisible + 420;
            if (scrollY < 0)
            {
                logsScrollView.SetScrollContent(LuiPosition.Full, new(0, scrollY, lineWidth, 0));
            }

            if (logsVisible > 0)
            {
                int index = 0;
                foreach (var log in logs)
                {
                    if (index > logsVisible) break;

                    var yOffset = -lineHeight * ++index;
                    cui.v2
                        .CreateText(logsScrollView,
                            position: LuiPosition.UpperLeft,
                            offset: new(12, yOffset, lineWidth, yOffset + lineHeight),
                            fontSize: 10,
                            color: theme.OnSurface,
                            text: log,
                            alignment: TextAnchor.UpperLeft)
                        .SetTextFont(FontTypes.DroidSansMono);
                }
            }
            else
            {
                cui.v2
                    .CreateText(logsScrollView,
                        position: LuiPosition.UpperLeft,
                        offset: new(12, -lineHeight, lineWidth, 0),
                        fontSize: 10,
                        color: theme.OnSurface,
                        text: Localizer.Text(LangKeys.menu_logs_empty, player),
                        alignment: TextAnchor.MiddleCenter)
                    .SetTextFont(FontTypes.DroidSansMono);
            }
        }

        #endregion

        #region UI Helpers

        private static void RenderOptionsList<T>(
            CUI cui,
            LUI.LuiContainer container,
            BasePlayer player,
            UserPreferenceData userPreference,
            int height,
            int width,
            string onSelectCommand,
            List<T> options,
            Func<int, bool> isSelectedFunc,
            Action<LUI.LuiContainer, int, T, bool> template)
        {
            var theme = userPreference.Theme;

            var optionsScrollView = cui.v2
                .CreateScrollView(container,
                    position: LuiPosition.Full,
                    offset: LuiOffset.None,
                    vertical: true,
                    horizontal: false,
                    movementType: ScrollRect.MovementType.Elastic,
                    elasticity: 0,
                    inertia: true,
                    decelerationRate: 0.1f,
                    scrollSensitivity: 20,
                    verticalScrollOptions: new()
                    {
                        size = 6,
                        autoHide = true,
                        handleColor = theme.Primary
                    });

            var scrollY = -height * options.Count + 420;
            if (scrollY < 0)
            {
                optionsScrollView.SetScrollContent(LuiPosition.Full, new(0, scrollY, width, 0));
            }

            for (int i = 0; i < options.Count; i++)
            {
                var isSelected = isSelectedFunc(i);
                int offsetY = -height * i;
                var color = isSelected ? theme.PrimaryContainer : theme.OutlineVariant;

                var selectRecordButton = CreateButton(cui, player, userPreference,
                    container: optionsScrollView,
                    position: LuiPosition.UpperLeft,
                    offset: new(5, -height + offsetY, width, -5 + offsetY),
                    color: color,
                    textColor: theme.OnPrimary,
                    textKey: LangKeys.StringEmpty,
                    commandName: onSelectCommand,
                    commandArgs: i.ToString()
                );

                template(selectRecordButton, i, options[i], isSelected);
            }
        }

        private static LUI.LuiContainer CreateButton(
            CUI cui,
            BasePlayer player,
            UserPreferenceData userPreference,
            LUI.LuiContainer container,
            LuiPosition position,
            LuiOffset offset,
            string color,
            string textColor,
            LangKeys textKey,
            string commandName,
            string commandArgs = null,
            int fontSize = 12,
            FontTypes fontType = FontTypes.RobotoCondensedRegular)
        {
            var buttonText = Localizer.Text(textKey, player);

            if (!commandName.StartsWith(CommandPrefix))
            {
                commandName = CommandPrefix + commandName;
            }

            if (commandArgs != null)
            {
                commandName += " " + commandArgs;
            }

            var button = cui.v2
                .CreateButton(
                    container: container,
                    position: position,
                    offset: offset,
                    command: commandName,
                    color: color
                );

            cui.v2.CreateText(
                    container: button,
                    position: new(0, 0, 1, 1),
                    offset: new(0, 0, 0, 0),
                    color: textColor,
                    fontSize: fontSize,
                    text: buttonText,
                    alignment: TextAnchor.MiddleCenter)
                .SetTextFont(fontType);

            return button;
        }

        private static void RenderEmpty(
            CUI cui,
            LUI.LuiContainer container,
            BasePlayer player,
            UserPreferenceData userPreference,
            LangKeys textKey)
        {
            var theme = userPreference.Theme;
            cui.v2
                .CreateText(
                    container: container,
                    position: LuiPosition.MiddleCenter,
                    offset: new(-100, -20, 100, 20),
                    fontSize: 12,
                    color: theme.OnSurface,
                    text: Localizer.Text(textKey, player),
                    alignment: TextAnchor.MiddleCenter)
                .SetTextFont(FontTypes.RobotoCondensedRegular);
        }

        private static void RenderInfoLine(
            CUI cui,
            LUI.LuiContainer container,
            UserPreferenceData userPreference,
            string text,
            int line)
        {
            const int lineHeight = 25;

            var theme = userPreference.Theme;
            int yOffset = -lineHeight * line - 5;
            cui.v2
                .CreateText(
                    container: container,
                    position: LuiPosition.UpperLeft,
                    offset: new(10, yOffset - lineHeight, 400, yOffset),
                    fontSize: 12,
                    color: theme.OnSurface,
                    text: text,
                    alignment: TextAnchor.MiddleLeft)
                .SetTextFont(FontTypes.RobotoCondensedRegular);
        }

        #endregion

        #region Confirmation

        public static void ShowConfirmation(
            BasePlayer player,
            Action<BasePlayer> onConfirm,
            LangKeys langKey,
            object arg1 = null,
            object arg2 = null)
        {
            var userPreference = UserPreferenceData.Load(_instance, player);
            var theme = userPreference.Theme;
            using var cui = _instance.CreateCUI();

            var parent = cui.v2
                .CreateParent(
                    parent: CUI.ClientPanels.Overall,
                    position: LuiPosition.Full,
                    name: ConfirmMenuId)
                .AddCursor()
                .SetDestroy(ConfirmMenuId);

            var layer = cui.v2
                .CreatePanel(
                    container: parent,
                    position: LuiPosition.Full,
                    offset: LuiOffset.None,
                    color: "0 0 0 .7"
                );

            var container = cui.v2
                .CreatePanel(
                    container: parent,
                    position: LuiPosition.MiddleCenter,
                    offset: new(-150, -60, 150, 60),
                    color: theme.SurfaceContainerHigh
                );

            cui.v2
                .CreateText(
                    container: container,
                    position: LuiPosition.MiddleCenter,
                    offset: new(-150, 0, 150, 50),
                    fontSize: 12,
                    color: theme.OnSurface,
                    text: Localizer.Text(langKey, player, arg1, arg2),
                    alignment: TextAnchor.MiddleCenter)
                .SetTextFont(FontTypes.RobotoCondensedRegular);

            CreateButton(cui, player, userPreference,
                container: container,
                position: LuiPosition.LowerCenter,
                offset: new(-90, 20, -7, 45),
                color: theme.Primary,
                textColor: theme.OnPrimary,
                textKey: LangKeys.menu_confirm,
                commandName: nameof(CommandMenuConfirmConfirm)
            );

            CreateButton(cui, player, userPreference,
                container: container,
                position: LuiPosition.LowerCenter,
                offset: new(7, 20, 90, 45),
                color: theme.Error,
                textColor: theme.OnError,
                textKey: LangKeys.menu_cancel,
                commandName: nameof(CommandMenuConfirmCancel)
            );

            player.AutoBuildSnapshot_OnConfirm = () => onConfirm(player);

            cui.v2.SendUi(player);
        }

        #endregion

        #endregion

        #region Helpers

        private static string GetTabButtonColor(int index, int targetIndex, MaterialTheme theme)
        {
            return index == targetIndex ? theme.PrimaryContainer : theme.OutlineVariant;
        }

        private static string GetTabButtonTextColor(int index, int targetIndex, MaterialTheme theme)
        {
            return index == targetIndex ? theme.OnPrimaryContainer : theme.OnPrimaryContainer;
        }

        #endregion
    }
}
