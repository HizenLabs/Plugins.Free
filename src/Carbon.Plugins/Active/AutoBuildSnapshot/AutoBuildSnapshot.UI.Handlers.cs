using Oxide.Game.Rust.Cui;
using System;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    #region Global Menu Actions

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandGlobalMenuClose)}")]
    private void CommandGlobalMenuClose(BasePlayer player) =>
        NavigateMenu(player, MenuLayer.Closed);

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

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandMainMenuTeleportToRecord)}")]
    private void CommandMainMenuTeleportToRecord(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0) return;

        if (ulong.TryParse(args[0], out ulong recordId) && _buildRecords.TryGetValue(recordId, out var record))
        {
            // Check admin permission
            if (!permission.UserHasPermission(player.UserIDString, _config.Commands.AdminPermission))
            {
                player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
                return;
            }

            // Teleport player to the TC position
            player.Teleport(record.BaseTC.ServerPosition + new Vector3(0, 1, 0));
            player.SendNetworkUpdate();

            player.ChatMessage($"Teleported to TC at {record.BaseTC.ServerPosition:F1}");
        }
    }

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

            NavigateMenu(player, MenuLayer.MainMenu, false, MenuTab.Records);
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

        NavigateMenu(player, MenuLayer.MainMenu, false, MenuTab.Logs);
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
                OpenSnapshotsMenu(player, record);
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
                OpenSnapshotsMenu(player, record);
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
            NavigateMenu(player, MenuLayer.ConfirmationDialog, false,
                "Confirm Rollback",
                $"Are you sure you want to rollback to the snapshot from {_snapshotMetaData[snapshotId].TimestampUTC:yyyy-MM-dd HH:mm:ss}?",
                $"{nameof(AutoBuildSnapshot)}.{nameof(CommandConfirmationRollback)} {snapshotId}"
            );
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsUndoRollback)}")]
    private void CommandSnapshotsUndoRollback(BasePlayer player)
    {
        // Check admin permission
        if (!_config.Commands.UserHasPermission(player, _config.Commands.Rollback, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return;
        }

        NavigateMenu(player, MenuLayer.ConfirmationDialog, false,
            "Confirm Undo",
            "Are you sure you want to undo the last rollback operation?",
            $"{nameof(AutoBuildSnapshot)}.{nameof(CommandConfirmationUndo)}"
        );
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsTeleportToBuilding)}")]
    private void CommandSnapshotsTeleportToBuilding(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0)
            return;

        // Check admin permission
        if (!permission.UserHasPermission(player.UserIDString, _config.Commands.AdminPermission))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return;
        }

        // Get the position from the building metadata
        if (_currentSelectedSnapshot.TryGetValue(player.userID, out Guid snapshotId) &&
            _snapshotMetaData.TryGetValue(snapshotId, out var meta) &&
            meta.LinkedBuildings.TryGetValue(args[0], out var buildingMeta))
        {
            // Teleport player to the building position
            player.Teleport(buildingMeta.Position + new Vector3(0, 1, 0));
            player.SendNetworkUpdate();

            player.ChatMessage($"Teleported to building at {buildingMeta.Position:F1}");
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

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandConfirmationUndo)}")]
    private void CommandConfirmationUndo(BasePlayer player)
    {
        // Check admin permission
        if (!_config.Commands.UserHasPermission(player, _config.Commands.Rollback, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            CloseConfirmationDialog(player);
            return;
        }

        CloseConfirmationDialog(player);

        // Execute undo (implementation would be in your plugin)
        ExecuteUndo(player);
    }

    #endregion
}
