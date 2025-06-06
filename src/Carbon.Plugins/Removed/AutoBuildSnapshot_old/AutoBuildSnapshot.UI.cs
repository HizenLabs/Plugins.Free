﻿using Carbon.Components;
using Facepunch;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // fully qualifying method params due to issues with codegen

public partial class AutoBuildSnapshot_old
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

        NavigateMenu(player, MenuLayer.MainMenu, hasUpdate: true);
    }

    /// <summary>
    /// Opens the snapshots menu for a specific building record.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    /// <param name="record">The building record to show snapshots for.</param>
    private void OpenSnapshotsMenu(BasePlayer player, BuildRecord record, bool isRefresh = false)
    {
        List<Guid> snapshots = null;
        if (record != null)
        {
            _currentBuildRecord[player.userID] = record.NetworkID;

            snapshots = GetSnapshotsForRecord(record);
        }

        NavigateMenu(player, MenuLayer.Snapshots, !isRefresh, isRefresh, record, snapshots);
    }

    /// <summary>
    /// Gets the list of snapshots for the given record.
    /// </summary>
    /// <param name="record">The building record to get snapshots for.</param>
    /// <returns>A list of snapshot IDs for the record.</returns>
    private List<System.Guid> GetSnapshotsForRecord(BuildRecord record)
    {
        var result = Pool.Get<List<Guid>>();

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

        return result;
    }

    /// <summary>
    /// Closes the confirmation dialog.
    /// </summary>
    /// <param name="player">The player to close the dialog for.</param>
    private void CloseConfirmationDialog(BasePlayer player)
    {
        CuiHelper.DestroyUi(player, _confirmationDialogId);

        if (!_playerMenuStates.TryGetValue(player.userID, out var currentLayer))
        {
            // we should never get here, but catch it just in case
            UpdateMenuState(player, MenuLayer.Closed, MenuLayer.Closed);
            return;
        }
        
        UpdateMenuState(player, currentLayer, currentLayer & ~MenuLayer.ConfirmationDialog);
    }

    /// <summary>
    /// Navigates to the specified layer.
    /// </summary>
    private void NavigateMenu(
        BasePlayer player,
        MenuLayer targetLayer,
        bool appendLayer = false,
        bool hasUpdate = false,
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

        if (!hasUpdate && targetLayer == currentLayer)
        {
            return;
        }

        var previousLayer = currentLayer;

        if (hasUpdate)
        {
            if (appendLayer)
            {
                throw new Exception($"Cannot append layer when {nameof(hasUpdate)} is true");
            }

            if (!currentLayer.HasFlag(targetLayer))
            {
                throw new Exception("Cannot update layer if it does not already exist in the stack");
            }
        }
        else
        {
            if (appendLayer)
            {
                currentLayer |= targetLayer;
            }
            else
            {
                currentLayer = targetLayer;
            }
        }

        HandleUiDestruction(MenuLayer.ConfirmationDialog, _menuLayerIdLookup[MenuLayer.ConfirmationDialog], player, previousLayer, currentLayer, targetLayer, hasUpdate);
        HandleUiDestruction(MenuLayer.Snapshots, _menuLayerIdLookup[MenuLayer.Snapshots], player, previousLayer, currentLayer, targetLayer, hasUpdate);
        HandleUiDestruction(MenuLayer.MainMenu, _menuLayerIdLookup[MenuLayer.MainMenu], player, previousLayer, currentLayer, targetLayer, hasUpdate);

        if (targetLayer == MenuLayer.Closed)
        {
            UpdateMenuState(player, previousLayer, currentLayer);
            return;
        }

        using var cui = CreateCUI();

        LUI.LuiContainer container;
        switch (targetLayer)
        {
            case MenuLayer.MainMenu:
                container = RenderMainMenu(cui, player);
                break;

            case MenuLayer.Snapshots:
                if (args.Length < 2)
                    throw new ArgumentException($"{nameof(NavigateMenu)}: Expected at least 2 arguments for {nameof(MenuLayer.Snapshots)}");

                if (args[0] != null && args[0] is not BuildRecord)
                    throw new ArgumentException($"{nameof(NavigateMenu)}: Expected BuildRecord as first argument for {nameof(MenuLayer.Snapshots)}");

                if (args[1] != null && args[1] is not List<Guid>)
                    throw new ArgumentException($"{nameof(NavigateMenu)}: Expected List<Guid> as second argument for {nameof(MenuLayer.Snapshots)}");

                var record = args[0] as BuildRecord;
                var snapshots = args[1] as List<Guid>;
                container = RenderSnapshotsMenu(cui, player, record, snapshots);
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

                container = RenderConfirmationDialog(cui, player, title, message, command);
                break;

            default:
                throw new NotSupportedException($"Unsupported menu layer: {targetLayer}");
        }

        // some edge cases happen where menus are already open
        // e.g; when a player selects snapshot and presses enter
        // this will re-select the snapshot. in this case, just refresh.
        if (previousLayer.HasFlag(targetLayer))
        {
            hasUpdate = true;
        }

        if (hasUpdate)
        {
            container.SetDestroy(_menuLayerIdLookup[targetLayer]);
        }

        cui.v2.SendUi(player);

        UpdateMenuState(player, previousLayer, currentLayer);
    }

    private void HandleUiDestruction(
        MenuLayer layer,
        string uiId,
        BasePlayer player,
        MenuLayer previousLayer, 
        MenuLayer currentLayer, 
        MenuLayer targetLayer, 
        bool hasUpdate)
    {
        if (hasUpdate && targetLayer == layer)
        {
            return;
        }

        if (previousLayer.HasFlag(layer) && !currentLayer.HasFlag(layer))
        {
            CuiHelper.DestroyUi(player, uiId);
        }
    }

    private void UpdateMenuState(BasePlayer player, MenuLayer previousLayer, MenuLayer targetLayer)
    {
        _previousMenuState[player.userID] = previousLayer;
        _playerMenuStates[player.userID] = targetLayer;
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

    /// <summary>
    /// Adds a log message but also sends it to the player.
    /// </summary>
    private void AddLogMessage(BasePlayer player, string message)
    {
        player.ChatMessage(message);
        AddLogMessage(message);
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
}
