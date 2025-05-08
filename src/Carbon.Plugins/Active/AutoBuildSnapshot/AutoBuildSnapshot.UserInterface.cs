using Facepunch;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Handles all user interface functionality for the plugin.
    /// </summary>
    private static class UserInterface
    {
        private const string mainMenuId = "abs.mainmenu";
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
                Hide(player);
            }

            Pool.FreeUnmanaged(ref _playerMenuState);
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
        public static void Toggle(BasePlayer player)
        {
            if (!_playerMenuState.TryGetValue(player.userID, out var isMenuActive))
            {
                isMenuActive = false;
            }

            if (isMenuActive)
            {
                Hide(player);
            }
            else
            {
                Show(player);
            }
        }

        /// <summary>
        /// Closes the main menu for the user.
        /// </summary>
        /// <param name="player">The player to close the menu for.</param>
        private static void Hide(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, mainMenuId);

            _playerMenuState[player.userID] = false;
        }

        /// <summary>
        /// Shows the main menu for the user.
        /// </summary>
        /// <param name="player">The player to show the menu to.</param>
        private static void Show(BasePlayer player)
        {
            _playerMenuState[player.userID] = true;
        }
    }
}
