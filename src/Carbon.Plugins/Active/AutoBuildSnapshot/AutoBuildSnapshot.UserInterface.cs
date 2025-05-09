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

        /// <summary>
        /// Initializes the user interface for the plugin.
        /// </summary>
        public static void Init()
        {
            _playerMenuState = Pool.Get<Dictionary<ulong, bool>>();
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
            var fontSize = userData.FontSize;
            var fontType = userData.FontType;

            float baseFontSize = FontSizes.Medium.Body;
            float heightScale = userData.FontSize.Body / baseFontSize;

            heightScale = Mathf.Clamp(heightScale, 0.85f, 1.2f);

            var widthScale = heightScale;
            if (userData.FontTypeOption == FontTypeOptions.DroidSans)
            {
                widthScale *= 1.05f;
            }

            var backgroundColor = userData.BackgroundOption switch
            {
                BackgroundOptions.Translucent => palette.Transparent,
                BackgroundOptions.Blur => palette.Blur,
                _ => palette.BackgroundBase + " 1.0"
            };

            var headerText = Localizer.Text(LangKeys.menu_title, player, _instance.Version);

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

            var headerHeight = 0.08f * heightScale;
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
                    fontSize: fontSize.Header,
                    text: headerText,
                    alignment: TextAnchor.MiddleLeft
                )
                .SetTextFont(fontType.Header);

            var headerButtons = cui.v2
                .CreatePanel(
                    container: header,
                    position: new(.5f, 0, 1, 1),
                    offset: new(0, 5, -5, -5),
                    color: palette.Transparent
                );

            var closeButtonWidth = 0.15f * widthScale;
            var closeButtonX = 1 - closeButtonWidth;
            CreateButton(cui, player, userData,
                container: headerButtons,
                position: new(closeButtonX, 0, 1, 1),
                offset: LuiOffset.None,
                textKey: LangKeys.menu_close,
                commandName: nameof(CommandMenuClose)
            );

            var optionsButtonWidth = 0.2f * widthScale;
            var optionsButtonX = closeButtonX - optionsButtonWidth;
            CreateButton(cui, player, userData,
                container: headerButtons,
                position: new(optionsButtonX, 0, closeButtonX, 1),
                offset: LuiOffset.None,
                textKey: LangKeys.menu_options,
                commandName: nameof(CommandMenuSettings)
            );

            var content = cui.v2
                .CreatePanel(
                    container: container,
                    position: new(0, 0, 1, headerStartY),
                    offset: LuiOffset.None,
                    color: palette.Transparent
                );

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
        }

        private static void CreateButton(
            CUI cui,
            BasePlayer player,
            UserPreferenceData userData,
            LUI.LuiContainer container,
            LuiPosition position,
            LuiOffset offset,
            LangKeys textKey,
            string commandName)
        {
            var buttonText = Localizer.Text(textKey, player);
            if (!commandName.StartsWith(CommandPrefix))
            {
                commandName = CommandPrefix + commandName;
            }

            var userPrefButton = cui.v2
                .CreateButton(
                    container: container,
                    position: position,
                    offset: offset,
                    command: commandName,
                    color: userData.ColorPalette.Primary
                );

            cui.v2.CreateText(
                    container: userPrefButton,
                    position: new(0, 0, 1, 1),
                    offset: new(0, 0, 0, 0),
                    color: userData.ColorPalette.OnPrimary,
                    fontSize: userData.FontSize.Body,
                    text: buttonText,
                    alignment: TextAnchor.MiddleCenter)
                .SetTextFont(userData.FontType.Body);
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

                [nameof(UserPreferenceData.FontSize)] = ModalModule.Modal.EnumField.MakeEnum(
                    displayName: Localizer.Text(LangKeys.menu_options_fontsize, player),
                    options: FontSizes.Options,
                    required: true,
                    @default: (int)userData.FontSizeOption),

                [nameof(UserPreferenceData.FontType)] = ModalModule.Modal.EnumField.MakeEnum(
                    displayName: Localizer.Text(LangKeys.menu_options_fonttype, player),
                    options: FontTypes.Options,
                    required: true,
                    @default: (int)userData.FontTypeOption)
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
