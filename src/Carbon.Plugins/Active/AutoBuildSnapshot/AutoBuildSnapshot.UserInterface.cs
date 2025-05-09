using Carbon.Components;
using Carbon.Modules;
using Facepunch;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using UnityEngine;

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
            var userData = UserPreference.For(player);
            var palette = userData.ColorPalette;

            var headFontSize = userData.HeaderFontSize;
            var headFontType = userData.HeaderFontType;
            GetFontScaling(userData.HeaderFontSize, userData.HeaderFontTypeOption, out var headScaleH, out var headScaleW);

            var bodyFontSize = userData.BodyFontSize;
            var bodyFontType = userData.BodyFontType;
            GetFontScaling(userData.BodyFontSize, userData.BodyFontTypeOption, out var bodyScaleH, out var bodyScaleW);

            var backgroundColor = userData.BackgroundOption switch
            {
                BackgroundOptions.Translucent => palette.Transparent,
                BackgroundOptions.Blur => palette.Blur,
                _ => palette.BackgroundBase + " 1.0"
            };

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
                    color: palette.Outline
                );

            var container = cui.v2
                .CreatePanel(
                    container: parent,
                    position: LuiPosition.MiddleCenter,
                    offset: new(-450, -275, 450, 275),
                    color: backgroundColor
                );

            if (userData.ColorPaletteOption == ColorPaletteOptions.Light)
            {
                backgroundColor = palette.BackgroundBase + " 0.4";
            }
            else
            {
                backgroundColor = palette.BackgroundBase + " 0.9";
            }

            if (userData.BackgroundOption == BackgroundOptions.Blur)
            {
                outline.SetMaterial(BlurAsset);
                container.SetMaterial(BlurAsset);

                container = cui.v2
                    .CreatePanel(
                        container: parent,
                        position: LuiPosition.MiddleCenter,
                        offset: new(-450, -275, 450, 275),
                        color: backgroundColor
                    );
            }
            else if (userData.BackgroundOption == BackgroundOptions.Translucent)
            {
                outline.SetMaterial(BlurAsset);
                container.SetMaterial(BlurAsset);


                container = cui.v2
                    .CreatePanel(
                        container: parent,
                        position: LuiPosition.MiddleCenter,
                        offset: new(-450, -275, 450, 275),
                        color: backgroundColor
                    );
            }

            var headerHeight = 0.08f * headScaleH;
            var headerStartY = 1 - headerHeight;
            var header = cui.v2
                .CreatePanel(
                    container: container,
                    position: new(0, headerStartY, 1, 1),
                    offset: LuiOffset.None,
                    color: palette.Primary
                );

            cui.v2.CreateText(
                    container: header,
                    position: new(.015f, 0, .5f, 1),
                    offset: new(1, 1, 0, 0),
                    color: palette.OnPrimary,
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
                    color: palette.Transparent
                );

            var closeButtonWidth = 0.18f * headScaleW;
            var closeButtonX = 1 - closeButtonWidth;
            CreateButton(cui, player, userData,
                container: headerButtons,
                position: new(closeButtonX, 0, 1, 1),
                offset: LuiOffset.None,
                color: palette.Primary,
                textColor: palette.OnPrimary,
                textKey: LangKeys.menu_close,
                isHeader: true,
                commandName: nameof(CommandMenuClose)
            );

            var optionsButtonWidth = 0.2f * headScaleW;
            var optionsButtonX = closeButtonX - optionsButtonWidth;
            CreateButton(cui, player, userData,
                container: headerButtons,
                position: new(optionsButtonX, 0, closeButtonX, 1),
                offset: LuiOffset.None,
                color: palette.Primary,
                textColor: palette.OnPrimary,
                textKey: LangKeys.menu_options,
                isHeader: true,
                commandName: nameof(CommandMenuSettings)
            );

            var content = cui.v2
                .CreatePanel(
                    container: container,
                    position: new(0, 0, 1, headerStartY),
                    offset: LuiOffset.None,
                    color: palette.Transparent
                );

            // watermark
            cui.v2
                .CreateText(
                    container: content,
                    position: LuiPosition.Full,
                    offset: new(0, 1, 0, 0),
                    fontSize: 8,
                    color: palette.Watermark,
                    text: $"AutoBuildSnapshot v{_instance.Version} by hizenxyz",
                    alignment: TextAnchor.LowerCenter)
                .SetTextFont(CUI.Handler.FontTypes.DroidSansMono);

            var tabButtonsHeight = 0.08f * bodyScaleH;
            var tabButtonsStartY = 1 - tabButtonsHeight;
            var tabButtons = cui.v2
                .CreatePanel(
                    container: content,
                    position: new(0, tabButtonsStartY, 1, 1),
                    offset: new(5, 7, -5, -7),
                    color: palette.Transparent
                );

            var contentButtonWidth = 0.12f * headScaleW;
            var homeButtonX = 0f;
            CreateButton(cui, player, userData,
                container: tabButtons,
                position: new(homeButtonX, 0, contentButtonWidth, 1),
                offset: LuiOffset.None,
                color: GetTabButtonColor(tabIndex, 0, palette),
                textColor: GetTabButtonTextColor(tabIndex, 0, palette),
                textKey: LangKeys.menu_tab_home,
                commandName: nameof(CommandMenuSwitchTab),
                commandArgs: "0"
            );

            var logsButtonX = contentButtonWidth;
            CreateButton(cui, player, userData,
                container: tabButtons,
                position: new(logsButtonX, 0, contentButtonWidth + logsButtonX, 1),
                offset: new(5, 0, 0, 0),
                color: GetTabButtonColor(tabIndex, 1, palette),
                textColor: GetTabButtonTextColor(tabIndex, 1, palette),
                textKey: LangKeys.menu_tab_logs,
                commandName: nameof(CommandMenuSwitchTab),
                commandArgs:"1"
            );

            switch (tabIndex)
            {
                case 1:
                    RenderLogsTab(cui, player, tabButtons, content, userData, headScaleW, headScaleH, bodyScaleW, bodyScaleH);
                    break;

                case 0:
                default:
                    RenderHomeTab(cui, player, tabButtons, content, userData, headScaleW, headScaleH, bodyScaleW, bodyScaleH);
                    break;
            }
        }

        private static void RenderHomeTab(
            CUI cui,
            BasePlayer player,
            LUI.LuiContainer tabButtons,
            LUI.LuiContainer content,
            UserPreferenceData userData,
            float headScaleW,
            float headScaleH,
            float bodyScaleW,
            float bodyScaleH)
        {
            var palette = userData.ColorPalette;

            var contentButtonWidth = 0.12f * headScaleW;
            bool backButtonEnabled = false;
            if (backButtonEnabled)
            {
                CreateButton(cui, player, userData,
                    container: tabButtons,
                    position: new(1 - contentButtonWidth, 0, 1, 1),
                    offset: LuiOffset.None,
                    color: palette.Tertiary,
                    textColor: palette.OnTertiary,
                    textKey: LangKeys.menu_content_back,
                    commandName: nameof(CommandMenuSettings)
                );
            }
        }

        private static void RenderLogsTab(
            CUI cui,
            BasePlayer player,
            LUI.LuiContainer tabButtons,
            LUI.LuiContainer container,
            UserPreferenceData userData,
            float headScaleW,
            float headScaleH,
            float bodyScaleW,
            float bodyScaleH)
        {
            var palette = userData.ColorPalette;

            var clearButtonWidth = 0.12f * headScaleW;
            CreateButton(cui, player, userData,
                container: tabButtons,
                position: new(1 - clearButtonWidth, 0, 1, 1),
                offset: LuiOffset.None,
                color: palette.Secondary,
                textColor: palette.OnSecondary,
                textKey: LangKeys.menu_content_clear,
                commandName: nameof(CommandMenuSettings)
            );
        }

        private static string GetTabButtonColor(int index, int targetIndex, ColorPalette palette)
        {
            return index == targetIndex ? palette.HighlightItem : palette.InactiveItem;
        }

        private static string GetTabButtonTextColor(int index, int targetIndex, ColorPalette palette)
        {
            return index == targetIndex ? palette.OnHighlightItem : palette.OnInactiveItem;
        }

        /// <summary>
        /// Calculates the scaling factors for font size and type.
        /// </summary>
        /// <param name="fontSize">The font size to scale.</param>
        /// <param name="fontType">The font type to scale.</param>
        /// <param name="heightScale">The height scaling factor.</param>
        /// <param name="widthScale">The width scaling factor.</param>
        private static void GetFontScaling(float fontSize, FontTypeOptions fontType, out float heightScale, out float widthScale)
        {
            float baseFontSize = 18f;
            heightScale = fontSize / baseFontSize;

            heightScale = Mathf.Clamp(heightScale, 0.85f, 1.4f);

            widthScale = heightScale;
            if (fontType == FontTypeOptions.DroidSans)
            {
                widthScale *= 1.05f;
            }
        }

        private static void CreateButton(
            CUI cui,
            BasePlayer player,
            UserPreferenceData userData,
            LUI.LuiContainer container,
            LuiPosition position,
            LuiOffset offset,
            string color,
            string textColor,
            LangKeys textKey,
            string commandName,
            string commandArgs = null,
            bool isHeader = false)
        {
            var buttonText = Localizer.Text(textKey, player);
            var fontSize = isHeader ? userData.HeaderFontSize : userData.BodyFontSize;
            var fontType = isHeader ? userData.HeaderFontType : userData.BodyFontType;

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

        /// <summary>
        /// Shows the settings menu for the user.
        /// </summary>
        /// <param name="player">The player to show the settings menu to.</param>
        public static void ShowPreference(BasePlayer player)
        {
            var title = Localizer.Text(LangKeys.menu_options_title, player);
            var fieldThemeText = Localizer.Text(LangKeys.menu_options_mode, player);
            var userData = UserPreference.For(player);
            var fields = new Dictionary<string, ModalModule.Modal.Field>
            {
                ["theme"] = ModalModule.Modal.EnumField.MakeEnum(
                    displayName: Localizer.Text(LangKeys.menu_options_theme, player),
                    options: Themes.Options,
                    required: true,
                    @default: 0,
                    isReadOnly: true),

                [nameof(UserPreferenceData.ColorPalette)] = ModalModule.Modal.EnumField.MakeEnum(
                    displayName: fieldThemeText,
                    options: ColorPalettes.ColorOptions,
                    required: true,
                    @default: (int)userData.ColorPaletteOption),

                [nameof(UserPreferenceData.ContrastOption)] = ModalModule.Modal.EnumField.MakeEnum(
                    displayName: Localizer.Text(LangKeys.menu_options_contrast, player),
                    options: ColorPalettes.ContrastOptions,
                    required: true,
                    @default: (int)userData.ContrastOption),

                [nameof(UserPreferenceData.BackgroundOption)] = ModalModule.Modal.EnumField.MakeEnum(
                    displayName: Localizer.Text(LangKeys.menu_options_background, player),
                    options: Backgrounds.Options,
                    required: true,
                    @default: (int)userData.BackgroundOption),

                [nameof(UserPreferenceData.HeaderFontType)] = ModalModule.Modal.EnumField.MakeEnum(
                    displayName: Localizer.Text(LangKeys.menu_options_header_fonttype, player),
                    options: FontTypes.Options,
                    required: true,
                    @default: (int)userData.HeaderFontTypeOption),

                [nameof(UserPreferenceData.HeaderFontSize)] = ModalModule.Modal.EnumField.MakeEnum(
                    displayName: Localizer.Text(LangKeys.menu_options_header_fontsize, player),
                    options: FontSizes.Options,
                    required: true,
                    @default: (int)userData.HeaderFontSizeOption),

                [nameof(UserPreferenceData.BodyFontType)] = ModalModule.Modal.EnumField.MakeEnum(
                    displayName: Localizer.Text(LangKeys.menu_options_body_fonttype, player),
                    options: FontTypes.Options,
                    required: true,
                    @default: (int)userData.BodyFontTypeOption),

                [nameof(UserPreferenceData.BodyFontSize)] = ModalModule.Modal.EnumField.MakeEnum(
                    displayName: Localizer.Text(LangKeys.menu_options_body_fontsize, player),
                    options: FontSizes.Options,
                    required: true,
                    @default: (int)userData.BodyFontSizeOption),
            };

            var modalModule = Base.BaseModule.GetModule<ModalModule>();
            modalModule.Open(player, title, fields, ShowPreference_Confirm);
        }

        /// <summary>
        /// Handles the confirmation of user preference updates.
        /// </summary>
        /// <param name="player">The player who confirmed the preferences.</param>
        /// <param name="modal">The modal instance containing the updated preferences.</param>
        public static void ShowPreference_Confirm(BasePlayer player, ModalModule.Modal modal)
        {
            UserPreference.Update(player, modal);

            ShowMenu(player); // refresh the menu
        }
    }
}
