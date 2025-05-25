using Carbon.Base;
using Carbon.Components;
using Carbon.Modules;
using Facepunch;
using HizenLabs.Extensions.UserPreference.Data;
using HizenLabs.Extensions.UserPreference.Material.API;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Carbon.Components.CUI.Handler;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Handles all user interface functionality for the plugin.
    /// </summary>
    private static class UserInterface
    {
        private const string BlurAsset = "assets/content/ui/uibackgroundblur.mat";
        public const string MainMenuId = "abs.mainmenu";
        public const string ConfirmMenuId = "abs.confirmation";

        private const string IconGearId = "gear";
        private const string IconCloseId = "close";
        private const string IconTrashCanId = "trashcan";

        private const string IconToolCupboardId = "cupboard.tool";
        private const string IconToolCupboardUrl = "https://cdn.carbonmod.gg/items/cupboard.tool.png";

        /// <summary>
        /// Initializes the user interface for the plugin.
        /// </summary>
        public static void Init()
        {
            var imageDb = BaseModule.GetModule<ImageDatabaseModule>();
            imageDb.Queue(IconToolCupboardId, IconToolCupboardUrl);
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

            // We use pooled list for RecordOptions, so just dispose it so it goes back into the pool
            if (player.AutoBuildSnapshot_RecordOptions is List<ChangeManagement.RecordingId> recordingIds)
            {
                Pool.FreeUnmanaged(ref recordingIds);
                player.AutoBuildSnapshot_RecordOptions = null;
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

            var closeButtonWidth = 0.18f;
            var closeButtonX = 1 - closeButtonWidth;
            CreateButton(cui, player, userPreference,
                container: headerButtons,
                position: new(closeButtonX, 0, 1, 1),
                offset: LuiOffset.None,
                color: theme.PrimaryContainer,
                textColor: theme.OnPrimaryContainer,
                textKey: LangKeys.menu_close,
                commandName: nameof(CommandMenuClose),
                fontSize: 14
            );

            var optionsButtonWidth = 0.2f;
            var optionsButtonX = closeButtonX - optionsButtonWidth;
            CreateButton(cui, player, userPreference,
                container: headerButtons,
                position: new(optionsButtonX, 0, closeButtonX, 1),
                offset: LuiOffset.None,
                color: theme.PrimaryContainer,
                textColor: theme.OnPrimaryContainer,
                textKey: LangKeys.menu_options,
                commandName: nameof(CommandMenuSettings),
                fontSize: 14
            );

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
                    offset: new(0, 3, 0, 0),
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

        private static void RenderHomeTab(
            CUI cui,
            BasePlayer player,
            LUI.LuiContainer tabButtons,
            LUI.LuiContainer tabContent,
            UserPreferenceData userPreference)
        {
            var theme = userPreference.Theme;

            var snapshotSelectionContainer = cui.v2
                .CreatePanel(tabContent,
                    position: new(0, 0, .26f, 1),
                    offset: new(12, 12, -12, 0),
                    color: theme.SurfaceContainer
                );

            var snapshotDetailContainer = cui.v2
                .CreatePanel(tabContent,
                    position: new(.26f, 0, 1, 1),
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
                RenderSelectionOptions(cui, snapshotSelectionContainer, player, recordingIds, userPreference);

                var buttonIndex = 0;
                var buttonWidth = 0.12f;
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

                if (Settings.Commands.Backup.HasPermission(player))
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
            else
            {
                cui.v2
                    .CreateText(
                        container: snapshotDetailContainer,
                        position: LuiPosition.MiddleCenter,
                        offset: new(-100, -20, 100, 20),
                        fontSize: 12,
                        color: theme.OnSurface,
                        text: Localizer.Text(LangKeys.menu_no_records_found, player),
                        alignment: TextAnchor.MiddleCenter)
                    .SetTextFont(FontTypes.RobotoCondensedRegular);
            }
        }

        private static void RenderSelectionOptions(
            CUI cui,
            LUI.LuiContainer snapshotSelectionContainer,
            BasePlayer player,
            List<ChangeManagement.RecordingId> recordingIds,
            UserPreferenceData userPreference)
        {
            var theme = userPreference.Theme;

            for (int i = 0; i < recordingIds.Count; i++)
            {
                var recordId = recordingIds[i];
                var lastSave = SaveManager.GetLastSave(recordId);
                var isActive = ChangeManagement.Recordings.TryGetValue(recordId, out var recording) && recording.IsActive;

                isActive = isActive && i < 2;

                var isSelected = i == player.AutoBuildSnapshot_SelectedRecordIndex;

                int panelHeight = 45;
                int panelWidth = 205;
                int offsetY = -panelHeight * i;
                var color = isSelected ? theme.PrimaryContainer : theme.OutlineVariant;

                var selectRecordButton = CreateButton(cui, player, userPreference,
                    container: snapshotSelectionContainer,
                    position: LuiPosition.UpperLeft,
                    offset: new(5, -panelHeight + offsetY, panelWidth, -5 + offsetY),
                    color: color,
                    textColor: theme.OnPrimary,
                    textKey: LangKeys.StringEmpty,
                    commandName: nameof(CommandMenuSelectRecord),
                    commandArgs: i.ToString()
                );

                cui.v2
                    .CreateImageFromDb(
                        container: selectRecordButton,
                        position: LuiPosition.LowerLeft,
                        offset: new(2, 2, panelHeight - 7, panelHeight - 7),
                        dbName: IconToolCupboardId)
                    .SetMaterial("assets/content/ui/namefontmaterial.mat");

                if (!isSelected)
                {
                    var iconMaskColor = theme.SecondaryContainer.WithOpacity(.7f);

                    var iconMask = cui.v2
                        .CreateImageFromDb(
                            container: selectRecordButton,
                            position: LuiPosition.LowerLeft,
                            offset: new(2, 2, panelHeight - 7, panelHeight - 7),
                            dbName: IconToolCupboardId,
                            color: iconMaskColor)
                        .SetMaterial("assets/content/ui/namefontmaterial.mat");
                }

                if (!isActive)
                {
                    cui.v2
                        .CreateImageFromDb(
                            container: selectRecordButton,
                            position: LuiPosition.LowerLeft,
                            offset: new(2, 2, 16, 16),
                            dbName: IconTrashCanId,
                            color: theme.Background.WithOpacity(.95f));
                }

                float posX = recordingIds[i].Position.x;
                float posY = recordingIds[i].Position.y;
                float posZ = recordingIds[i].Position.z;

                cui.v2
                    .CreateText(
                        container: selectRecordButton,
                        position: LuiPosition.LowerLeft,
                        offset: new(panelHeight, 0, panelWidth, panelHeight - 8),
                        fontSize: 8,
                        color: theme.OnSecondaryContainer,
                        text: $"X: {posX}\nY: {posY}\nZ: {posZ}",
                        alignment: TextAnchor.UpperLeft
                    )
                    .SetTextFont(FontTypes.DroidSansMono);

                cui.v2
                    .CreateText(
                        container: selectRecordButton,
                        position: LuiPosition.LowerLeft,
                        offset: new(panelHeight * 2 + 20, 0, panelWidth, panelHeight - 8),
                        fontSize: 8,
                        color: theme.OnSecondaryContainer,
                        text: $"Entities: {lastSave.OriginalEntityCount}\nZones: {lastSave.ZoneCount}\nAuthorized: {lastSave.AuthorizedCount}",
                        alignment: TextAnchor.UpperLeft
                    )
                    .SetTextFont(FontTypes.DroidSansMono);
            }
        }
        
        private static void RenderLogsTab(
            CUI cui,
            BasePlayer player,
            LUI.LuiContainer tabButtons,
            LUI.LuiContainer tabContent,
            UserPreferenceData userPreference)
        {
            var theme = userPreference.Theme;

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

        private static string GetTabButtonColor(int index, int targetIndex, MaterialTheme theme)
        {
            return index == targetIndex ? theme.PrimaryContainer : theme.OutlineVariant;
        }

        private static string GetTabButtonTextColor(int index, int targetIndex, MaterialTheme theme)
        {
            return index == targetIndex ? theme.OnPrimaryContainer : theme.OnSurface;
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

        public static void ShowConfirmation(
            BasePlayer player,
            Action<BasePlayer> onConfirm,
            LangKeys langKey,
            object arg1 = null)
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
                    offset: new(-150, 10, 150, 40),
                    fontSize: 12,
                    color: theme.OnSurface,
                    text: Localizer.Text(langKey, player, arg1),
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
    }
}
