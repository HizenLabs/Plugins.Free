using Carbon.Components;
using Facepunch;
using Oxide.Game.Rust.Cui;
using Oxide.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // fully qualifying method params due to issues with codegen

public partial class AutoBuildSnapshot
{
    #region Navigation, Rollback, Logs

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

    /// <summary>
    /// Opens the snapshots menu for a specific building record.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    /// <param name="record">The building record to show snapshots for.</param>
    private void OpenSnapshotsMenu(BasePlayer player, BuildRecord record)
    {
        _currentBuildRecord[player.userID] = record.NetworkID;

        var snapshots = Pool.Get<List<Guid>>();
        GetSnapshotsForRecord(snapshots, record);

        if (snapshots.Count > 0 && !_currentSelectedSnapshot.ContainsKey(player.userID))
        {
            _currentSelectedSnapshot[player.userID] = snapshots[0];
        }

        NavigateMenu(player, MenuLayer.Snapshots, true, record, snapshots);

        Pool.FreeUnmanaged(ref snapshots);
    }

    /// <summary>
    /// Gets the list of snapshots for the given record.
    /// </summary>
    /// <param name="record">The building record to get snapshots for.</param>
    /// <returns>A list of snapshot IDs for the record.</returns>
    private void GetSnapshotsForRecord(List<System.Guid> results, BuildRecord record)
    {
        List<Guid> result = new();

        // Try to find snapshots by the persistent ID
        if (_buildingIDToSnapshotIndex.TryGetValue(record.PersistentID, out var snapshots))
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
    }

    /// <summary>
    /// Closes the confirmation dialog.
    /// </summary>
    /// <param name="player">The player to close the dialog for.</param>
    private void CloseConfirmationDialog(BasePlayer player)
    {
        CuiHelper.DestroyUi(player, _confirmationDialogId);
    }

    /// <summary>
    /// Navigates to the specified layer.
    /// </summary>
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

        var previousLayer = currentLayer;

        if (appendLayer)
        {
            currentLayer |= targetLayer;
        }
        else
        {
            currentLayer = targetLayer;
        }

        if (previousLayer.HasFlag(MenuLayer.ConfirmationDialog) && !currentLayer.HasFlag(MenuLayer.ConfirmationDialog))
        {
            CuiHelper.DestroyUi(player, _confirmationDialogId);
        }

        if (previousLayer.HasFlag(MenuLayer.Snapshots) && !currentLayer.HasFlag(MenuLayer.Snapshots))
        {
            CuiHelper.DestroyUi(player, _snapshotMenuId);
        }

        if (previousLayer.HasFlag(MenuLayer.MainMenu) && !currentLayer.HasFlag(MenuLayer.MainMenu))
        {
            CuiHelper.DestroyUi(player, _mainMenuId);
        }

        if (targetLayer == MenuLayer.Closed)
            return;

        using var cui = CreateCUI();

        string panelId;
        switch (targetLayer)
        {
            case MenuLayer.MainMenu:
                panelId = RenderMainMenu(cui, player).name;
                break;

            case MenuLayer.Snapshots:
                if (args.Length < 2)
                    throw new ArgumentException($"{nameof(NavigateMenu)}: Expected at least 2 arguments for {nameof(MenuLayer.Snapshots)}");

                if (args[0] is not BuildRecord record)
                    throw new ArgumentException($"{nameof(NavigateMenu)}: Expected BuildRecord as first argument for {nameof(MenuLayer.Snapshots)}");

                if (args[1] is not List<Guid> snapshots)
                    throw new ArgumentException($"{nameof(NavigateMenu)}: Expected List<Guid> as second argument for {nameof(MenuLayer.Snapshots)}");

                panelId = RenderSnapshotsMenu(cui, player, record, snapshots).name;
                break;

            case MenuLayer.ConfirmationDialog:
                if (args.Length < 3)
                    throw new ArgumentException($"{nameof(NavigateMenu)}: Expected at least 3 arguments for {nameof(MenuLayer.ConfirmationDialog)}");

                if (args[0] is not string title)
                    throw new ArgumentException($"{nameof(NavigateMenu)}: Expected string as first argument for {nameof(MenuLayer.ConfirmationDialog)}");

                if (args[1] is not string message)
                    throw new ArgumentException($"{nameof(NavigateMenu)}: Expected string as second argument for {nameof(MenuLayer.ConfirmationDialog)}");

                if (args[2] is not string command)
                    throw new ArgumentException($"{nameof(NavigateMenu)}: Expected string as third argument for {nameof(MenuLayer.ConfirmationDialog)}");

                panelId = RenderConfirmationDialog(cui, player, title, message, command).name;
                break;

            default:
                throw new NotSupportedException($"Unsupported menu layer: {targetLayer}");
        }

        if (previousLayer.HasFlag(targetLayer))
        {
            CuiHelper.DestroyUi(player, panelId);
        }

        cui.v2.SendUi(player);

        _previousMenuState[player.userID] = previousLayer;
        _playerMenuStates[player.userID] = currentLayer;
    }

    /// <summary>
    /// Executes the rollback operation for the specified snapshot.
    /// </summary>
    /// <param name="player">The player who initiated the rollback.</param>
    /// <param name="snapshotId">The ID of the snapshot to rollback to.</param>
    private void ExecuteRollback(BasePlayer player, System.Guid snapshotId)
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

    private List<string> GetRecentLogs(int count)
    {
        return _logMessages.Take(Mathf.Min(count, _logMessages.Count)).ToList();
    }

    private void ClearLogMessages()
    {
        _logMessages.Clear();
        AddLogMessage("Log cleared");
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

    #endregion

    #region Enums

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

    private enum MenuTab
    {
        Records,
        Logs
    }

    #endregion

    #region Debugging

    [ChatCommand($"debug")]
    private void CommandUIDebug(BasePlayer player, string command, string[] args)
    {
        Vector3 targetCoords;
        if (TryGetPlayerTargetEntity(player, out var target))
        {
            // Found target, get zones from target coords
            targetCoords = target.ServerPosition;
        }
        else if (!TryGetPlayerTargetCoordinates(player, out targetCoords))
        {
            // No target found, get coords from target
            player.ChatMessage(_lang.GetMessage(LangKeys.error_target_missing, player));
            return;
        }

    }

    #endregion
}
