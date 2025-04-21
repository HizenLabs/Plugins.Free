using Oxide.Game.Rust.Cui;
using System;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    #region Global Menu Actions

    /// <summary>
    /// Closes the entire menu.
    /// </summary>
    /// <param name="player">The player to close the menu for.</param>
    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandGlobalMenuClose)}")]
    private void CommandGlobalMenuClose(BasePlayer player) =>
        NavigateMenu(player, MenuLayer.Closed);

    /// <summary>
    /// Teleports the player to the specified record or building.
    /// </summary>
    /// <param name="player">The player to teleport.</param>
    /// <param name="command">The command that was executed.</param>
    /// <param name="args">The command arguments (target type and ID).</param>
    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandGlobalTeleport)}")]
    private void CommandGlobalTeleport(BasePlayer player, string command, string[] args)
    {
        // Check admin permission
        if (!permission.UserHasPermission(player.UserIDString, _config.Commands.AdminPermission))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return;
        }

        //============================ MAIN MENU ========================
        if (args.Length < 3)
            return;

        if (float.TryParse(args[0], out float x) &&
            float.TryParse(args[1], out float y) &&
            float.TryParse(args[2], out float z))
        {
            var pos = new Vector3(x, y + 2f, z);
            player.Teleport(pos);
            player.SendNetworkUpdate();
            player.ChatMessage($"Teleported to TC at {x:F1}, {y:F1}, {z:F1}");
        }
    }

    #endregion

    #region Main Menu Navigation

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandMainMenuTabRecords)}")]
    private void CommandMainMenuTabRecords(BasePlayer player) =>
        RefreshMenuWithTab(player, MenuTab.Records);

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandMainMenuTabLogs)}")]
    private void CommandMainMenuTabLogs(BasePlayer player) =>
        RefreshMenuWithTab(player, MenuTab.Logs);

    #endregion

    #region Main Menu Actions

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandMainMenuOpenSnapshots)}")]
    private void CommandMainMenuOpenSnapshots(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0) return;

        if (ulong.TryParse(args[0], out ulong recordId) && _buildRecords.TryGetValue(recordId, out var record))
        {
            // Switch to snapshots mode
            OpenSnapshotsMenu(player, record);
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandMainMenuScrollRecords)}")]
    private void CommandMainMenuScrollRecords(BasePlayer player, string command, string[] args)
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

            NavigateMenu(player, MenuLayer.MainMenu, false, true, MenuTab.Records);
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandMainMenuClearLogs)}")]
    private void CommandMainMenuClearLogs(BasePlayer player)
    {
        // Check admin permission
        if (!permission.UserHasPermission(player.UserIDString, _config.Commands.AdminPermission))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return;
        }

        ClearLogMessages();

        NavigateMenu(player, MenuLayer.MainMenu, false, true, MenuTab.Logs);
    }

    #endregion

    #region Snapshot Navigation

    // Command handlers for the snapshots UI
    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsNavigateBack)}")]
    private void CommandSnapshotsNavigateBack(BasePlayer player)
    {
        CuiHelper.DestroyUi(player, _snapshotMenuId);
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsSelect)}")]
    private void CommandSnapshotsSelect(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0)
            return;

        if (Guid.TryParse(args[0], out Guid snapshotId) && _snapshotMetaData.ContainsKey(snapshotId))
        {
            _currentSelectedSnapshot[player.userID] = snapshotId;

            if (_currentBuildRecord.TryGetValue(player.userID, out ulong recordId) &&
                _buildRecords.TryGetValue(recordId, out var record))
            {
                OpenSnapshotsMenu(player, record, true);
            }
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsScroll)}")]
    private void CommandSnapshotsScroll(BasePlayer player, string command, string[] args)
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
                OpenSnapshotsMenu(player, record, true);
            }
        }
    }

    #endregion

    #region Snapshot Actions

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsShowZones)}")]
    private void CommandSnapshotsShowZones(BasePlayer player)
    {
        // Check admin permission
        if (!permission.UserHasPermission(player.UserIDString, _config.Commands.AdminPermission))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return;
        }

        /*
        if (_currentSelectedSnapshot.TryGetValue(player.userID, out var snapshotId))
        {
            // TODO: Create visualization for the zones
            // Create timer callback to remove the zones after a certain time
        }
        */
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsPreviewRollback)}")]
    private void CommandSnapshotsPreviewRollback(BasePlayer player)
    {
        // Check admin permission
        if (!permission.UserHasPermission(player.UserIDString, _config.Commands.AdminPermission))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return;
        }

        /*
        if (_currentSelectedSnapshot.TryGetValue(player.userID, out var snapshotId))
        {
            // TODO: Create visualization for the zones
            // Create timer callback to remove the zones after a certain time
        }
        */
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsRollback)}")]
    private void CommandSnapshotsRollback(BasePlayer player)
    {
        // Check admin permission
        if (!_config.Commands.UserHasPermission(player, _config.Commands.Rollback, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return;
        }

        if (_currentSelectedSnapshot.TryGetValue(player.userID, out Guid snapshotId))
        {
            NavigateMenu(player, MenuLayer.ConfirmationDialog, true, false,
                "Confirm Rollback",
                $"Are you sure you want to rollback to the snapshot from {_snapshotMetaData[snapshotId].TimestampUTC:yyyy-MM-dd HH:mm:ss}?",
                $"{nameof(AutoBuildSnapshot)}.{nameof(CommandConfirmationRollback)} {snapshotId}"
            );
        }
    }

    #endregion

    #region Confirmation Dialog Actions

    // Command handlers for confirmation dialog
    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandConfirmationCancel)}")]
    private void CommandConfirmationCancel(BasePlayer player)
    {
        CloseConfirmationDialog(player);
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandConfirmationRollback)}")]
    private void CommandConfirmationRollback(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0)
            return;

        // Check admin permission
        if (!_config.Commands.UserHasPermission(player, _config.Commands.Rollback, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
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

    #endregion
}
