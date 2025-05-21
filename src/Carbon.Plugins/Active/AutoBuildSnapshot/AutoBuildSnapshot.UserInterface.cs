using Carbon.Components;
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
        private const string MainMenuId = "abs.mainmenu";

        private static Dictionary<ulong, bool> _playerMenuState;
        private static Dictionary<ulong, int> _playerTabIndex;

        /// <summary>
        /// Initializes the user interface for the plugin.
        /// </summary>
        public static void Init()
        {
            _playerMenuState = Pool.Get<Dictionary<ulong, bool>>();
            _playerTabIndex = Pool.Get<Dictionary<ulong, int>>();
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

            if (_playerMenuState != null)
            {
                Pool.FreeUnmanaged(ref _playerMenuState);
            }

            if (_playerTabIndex != null)
            {
                Pool.FreeUnmanaged(ref _playerTabIndex);
            }
        }

        /// <summary>
        /// Handles player disconnection events.
        /// </summary>
        /// <param name="player">The player that disconnected.</param>
        public static void HandleDisconnect(BasePlayer player)
        {
            if (_playerMenuState != null && _playerMenuState.ContainsKey(player.userID))
            {
                _playerMenuState.Remove(player.userID);
            }

            if (_playerTabIndex != null && _playerTabIndex.ContainsKey(player.userID))
            {
                _playerTabIndex.Remove(player.userID);
            }
        }

        /// <summary>
        /// Toggles the main menu for the user.
        /// </summary>
        /// <param name="player">The player to toggle the menu for.</param>
        public static void ToggleMenu(BasePlayer player)
        {
            if (_playerMenuState == null || !player) return;

            if (!_playerMenuState.TryGetValue(player.userID, out var isMenuActive))
            {
                isMenuActive = false;
            }

            if (isMenuActive)
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
            if (_playerTabIndex == null || !player) return;

            if (!_playerTabIndex.TryGetValue(player.userID, out var currentIndex))
            {
                currentIndex = 0;
            }

            if (currentIndex != targetIndex)
            {
                _playerTabIndex[player.userID] = targetIndex;
            }

            ShowMenu(player);
        }

        /// <summary>
        /// Closes the main menu for the user.
        /// </summary>
        /// <param name="player">The player to close the menu for.</param>
        public static void HideMenu(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, MainMenuId);

            if (_playerMenuState != null)
            {
                _playerMenuState[player.userID] = false;
            }
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

            _playerMenuState[player.userID] = true;
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

            if (!_playerTabIndex.TryGetValue(player.userID, out var tabIndex))
            {
                tabIndex = 0;
                _playerTabIndex[player.userID] = tabIndex;
            }

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
                    color: theme.SurfaceContainer
                );


            var headerHeight = 0.08f;
            var headerStartY = 1 - headerHeight;
            var header = cui.v2
                .CreatePanel(
                    container: container,
                    position: new(0, headerStartY, 1, 1),
                    offset: LuiOffset.None,
                    color: theme.Background
                );

            cui.v2.CreateText(
                    container: header,
                    position: new(.015f, 0, .5f, 1),
                    offset: new(1, 1, 0, 0),
                    color: theme.OnBackground,
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
                color: theme.Background,
                textColor: theme.OnBackground,
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
                color: theme.Background,
                textColor: theme.OnBackground,
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
                commandArgs:"1"
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

            var contentButtonWidth = 0.12f;
            bool backButtonEnabled = false;
            if (backButtonEnabled)
            {
                CreateButton(cui, player, userPreference,
                    container: tabButtons,
                    position: new(1 - contentButtonWidth, 0, 1, 1),
                    offset: LuiOffset.None,
                    color: theme.Tertiary,
                    textColor: theme.OnTertiary,
                    textKey: LangKeys.menu_content_back,
                    commandName: nameof(CommandMenuSettings)
                );
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
                color: theme.Secondary,
                textColor: theme.OnSecondary,
                textKey: LangKeys.menu_content_clear,
                commandName: nameof(CommandMenuSettings)
            );

            var logsPanel = cui.v2
                .CreatePanel(tabContent,
                    position: new(0, 0, 1, 1),
                    offset: new(12, 12, -12, 0),
                    color: theme.Surface
                );

            using var logs = Helpers.GetLogs();

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
            return index == targetIndex ? theme.Primary : theme.OutlineVariant;
        }

        private static string GetTabButtonTextColor(int index, int targetIndex, MaterialTheme theme)
        {
            return index == targetIndex ? theme.OnPrimary : theme.OnSurface;
        }

        private static void CreateButton(
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

            var userPrefButton = cui.v2
                .CreateButton(
                    container: container,
                    position: position,
                    offset: offset,
                    command: commandName,
                    color: color
                );

            cui.v2.CreateText(
                    container: userPrefButton,
                    position: new(0, 0, 1, 1),
                    offset: new(0, 0, 0, 0),
                    color: textColor,
                    fontSize: fontSize,
                    text: buttonText,
                    alignment: TextAnchor.MiddleCenter)
                .SetTextFont(fontType);
        }
    }
}
