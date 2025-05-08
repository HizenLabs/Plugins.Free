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
            _instance.Puts("CLosing menu");
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

            var title = Localizer.Text(LangKeys.menu_title, player);
            var btnCloseText = Localizer.Text(LangKeys.menu_close, player);
            var btnUserPrefText = Localizer.Text(LangKeys.menu_options, player);

            var parent = cui.v2
                .CreateParent(
                    parent: CUI.ClientPanels.HudMenu,
                    position: LuiPosition.Full,
                    name: MainMenuId)
                .AddCursor()
                .SetDestroy(MainMenuId);

            cui.v2
                .CreatePanel(
                    container: parent,
                    position: LuiPosition.MiddleCenter,
                    offset: new(-451, -275.65f, 451, 276),
                    color: palette.Outline
                )
                .SetMaterial(BlurAsset);

            var main = cui.v2
                .CreatePanel(
                    container: parent,
                    position: LuiPosition.MiddleCenter,
                    offset: new(-450, -275, 450, 275),
                    color: palette.Background
                )
                .SetMaterial(BlurAsset);

            var header = cui.v2
                .CreatePanel(
                    container: main,
                    position: new(0, .92f, 1, 1),
                    offset: LuiOffset.None,
                    color: palette.Surface
                );

            cui.v2.CreateText(
                    container: header,
                    position: new(.015f, 0, .5f, 1),
                    offset: new(1, 1, 0, 0),
                    color: palette.OnSurface,
                    fontSize: fontSize.Header,
                    text: title,
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

            var closeButton = cui.v2
                .CreateButton(
                    container: headerButtons,
                    position: new(.85f, 0, 1, 1),
                    offset: new(0, 0, 0, 0),
                    command: CommandPrefix + nameof(CommandMenuClose),
                    color: palette.Button
                );

            cui.v2.CreateText(
                    container: closeButton,
                    position: new(0, 0, 1, 1),
                    offset: new(0, 0, 0, 0),
                    color: palette.OnSurface,
                    fontSize: fontSize.Body,
                    text: btnCloseText,
                    alignment: TextAnchor.MiddleCenter
                );

            var userPrefButton = cui.v2
                .CreateButton(
                    container: headerButtons,
                    position: new(.7f, 0, .85f, 1),
                    offset: new(-5, 0, -5, 0),
                    command: CommandPrefix + nameof(CommandMenuSettings),
                    color: palette.Button
                );

            cui.v2.CreateText(
                    container: userPrefButton,
                    position: new(0, 0, 1, 1),
                    offset: new(0, 0, 0, 0),
                    color: palette.OnSurface,
                    fontSize: fontSize.Body,
                    text: btnUserPrefText,
                    alignment: TextAnchor.MiddleCenter
                );
        }

        /// <summary>
        /// Shows the settings menu for the user.
        /// </summary>
        /// <param name="player">The player to show the settings menu to.</param>
        public static void ShowPreference(BasePlayer player)
        {
            var title = Localizer.Text(LangKeys.menu_options_title, player);
            var fieldThemeText = Localizer.Text(LangKeys.menu_options_theme, player);
            var userData = UserPreference.For(player);
            var fields = new Dictionary<string, ModalModule.Modal.Field>
            {
                [nameof(UserPreferenceData.ColorPalette)] = ModalModule.Modal.EnumField.MakeEnum(
                    displayName: fieldThemeText,
                    options: ColorPalettes.Options,
                    required: true,
                    @default: (int)userData.ColorPaletteOption),

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
