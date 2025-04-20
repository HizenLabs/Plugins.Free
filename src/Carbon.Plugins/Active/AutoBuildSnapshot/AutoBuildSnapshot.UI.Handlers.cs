using Oxide.Game.Rust.Cui;
using System;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // Simplify names

public partial class AutoBuildSnapshot
{
    #region Main Menu Navigation

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.menu.tab.records")]
    private void CommandSwitchToRecordsTab(BasePlayer player) =>
        RefreshMenuWithTab(player, MenuTab.Records);

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.menu.tab.logs")]
    private void CommandSwitchToLogsTab(BasePlayer player) =>
        RefreshMenuWithTab(player, MenuTab.Logs);

    #endregion

    #region Main Menu Record Actions

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.scroll.records")]
    private void CommandScrollRecords(BasePlayer player, string command, string[] args)
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

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.teleport")]
    private void CommandTeleportToRecord(BasePlayer player, string command, string[] args)
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

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots")]
    private void CommandShowSnapshots(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0) return;

        if (ulong.TryParse(args[0], out ulong recordId) && _buildRecords.TryGetValue(recordId, out var record))
        {
            // Switch to snapshots mode
            OpenSnapshotsMenu(player, record);
        }
    }

    #endregion

    #region Main Menu Log Actions

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.logs.clear")]
    private void CommandClearLogs(BasePlayer player)
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
    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.menu.snapshots.back")]
    private void CommandSnapshotsBack(BasePlayer player)
    {
        CuiHelper.DestroyUi(player, _snapshotMenuId);
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.select")]
    private void CommandSelectSnapshot(BasePlayer player, string command, string[] args)
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

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.scroll")]
    private void CommandScrollSnapshots(BasePlayer player, string command, string[] args)
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

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.teleport")]
    private void CommandSnapTeleportToBuilding(BasePlayer player, string command, string[] args)
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

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.showzones")]
    private void CommandShowSnapZones(BasePlayer player)
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

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.rollback")]
    private void CommandRollbackSnapshot(BasePlayer player)
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
                $"{nameof(AutoBuildSnapshot)}.confirm.rollback {snapshotId}"
            );
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.snapshots.undo")]
    private void CommandUndoRollback(BasePlayer player)
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
            $"{nameof(AutoBuildSnapshot)}.confirm.undo"
        );
    }

    #endregion

    #region Confirmation Dialog Actions

    // Command handlers for confirmation dialog
    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.confirm.cancel")]
    private void CommandCancelConfirmation(BasePlayer player)
    {
        CloseConfirmationDialog(player);
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.confirm.rollback")]
    private void CommandConfirmRollback(BasePlayer player, string command, string[] args)
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

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.confirm.undo")]
    private void CommandConfirmUndo(BasePlayer player)
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
