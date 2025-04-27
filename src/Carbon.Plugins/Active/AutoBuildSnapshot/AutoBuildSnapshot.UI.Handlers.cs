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
        if (!UserHasPermission(player, _config.Commands.AdminPermission)) return;

        if (args.Length < 3) return;

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
        if (!UserHasPermission(player, _config.Commands.AdminPermission)) return;

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
            else
            {
                OpenSnapshotsMenu(player, null, true);
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
            else
            {
                OpenSnapshotsMenu(player, null, true);
            }
        }
    }

    #endregion

    #region Snapshot Actions

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsPreviewZones)}")]
    private void CommandSnapshotsPreviewZones(BasePlayer player)
    {
        if (!UserHasPermission(player, _config.Commands.AdminPermission)) return;

        if (TryGetSelectedSnapshotHandle(player, out var handle))
        {
            handle.PreviewZones();

            NavigateMenu(player, MenuLayer.Snapshots, false, true, null, null);
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsPreviewRollback)}")]
    private void CommandSnapshotsPreviewRollback(BasePlayer player)
    {
        if (!UserHasPermission(player, _config.Commands.AdminPermission)) return;

        if (TryGetSelectedSnapshotHandle(player, out var handle))
        {
            handle.PreviewRollback();

            NavigateMenu(player, MenuLayer.Snapshots, false, true, null, null);
        }
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsRollback)}")]
    private void CommandSnapshotsRollback(BasePlayer player)
    {
        if (!UserHasPermission(player, _config.Commands.AdminPermission)) return;

        if (TryGetSelectedSnapshotHandle(player, out var handle)
            && handle.TryConfirmationLock(player, out var confirmationCode))
        {
            NavigateMenu(player, MenuLayer.ConfirmationDialog, true, false,
                "Confirm Rollback",
                $"Are you sure you want to rollback to the snapshot from {handle.Meta.TimestampUTC:yyyy-MM-dd HH:mm:ss}?",
                $"{nameof(AutoBuildSnapshot)}.{nameof(CommandConfirmationRollback)} {confirmationCode}"
            );
        }
    }

    #endregion

    #region Confirmation Dialog Actions

    // Command handlers for confirmation dialog
    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandConfirmationCancel)}")]
    private void CommandConfirmationCancel(BasePlayer player)
    {
        if (TryGetSelectedSnapshotHandle(player, out var handle))
        {
            handle.TryConfirmationCancel(player);
        }

        CloseConfirmationDialog(player);
    }

    [ProtectedCommand($"{nameof(AutoBuildSnapshot)}.{nameof(CommandConfirmationRollback)}")]
    private void CommandConfirmationRollback(BasePlayer player, string command, string[] args)
    {
        if (args.Length == 0)
            return;

        CloseConfirmationDialog(player);

        if (!UserHasPermission(player, _config.Commands.Rollback)) return;

        if (!Guid.TryParse(args[0], out Guid confirmationCode))
        {
            player.ChatMessage("Confirmation expired or invalid.");
            return;
        }

        if (!TryGetSelectedSnapshotHandle(player, out var handle)
            || !handle.TryBeginRollback(player, confirmationCode))
        {
            player.ChatMessage("Failed to begin rollback.");
            return;
        }
    }

    #endregion

    #region Helpers

    private bool UserHasPermission(BasePlayer player, string permissionName)
    {
        if (!permission.UserHasPermission(player.UserIDString, permissionName))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return false;
        }

        return true;
    }

    private bool UserHasPermission(BasePlayer player, AutoBuildSnapshotConfig.CommandSetting command)
    {
        if (!_config.Commands.UserHasPermission(player, command, this))
        {
            player.ChatMessage(_lang.GetMessage(LangKeys.error_no_permission, player));
            return false;
        }

        return true;
    }

    private bool TryGetSelectedSnapshotHandle(BasePlayer player, out SnapshotHandle handle)
    {
        if (!_currentSelectedSnapshot.TryGetValue(player.userID, out var snapshotId))
        {
            player.ChatMessage("You must select a snapshot before doing that.");

            handle = null;
            return false;
        }

        if (!_snapshotMetaData.TryGetValue(snapshotId, out var meta))
        {
            player.ChatMessage("Invalid snapshot selected.");

            handle = null;
            return false;
        }

        if (!SnapshotHandle.TryTake(meta, player, out handle))
        {
            if (handle != null)
            {
                player.ChatMessage($"Snapshot is already in use by '{handle.Player.displayName}' (State: {handle.State})");
                player.ChatMessage($" Last activity: {FormatRelativeTime(handle.TimeSinceModified)} ago");
                player.ChatMessage($" Expiration: {handle.Expiration:yyyy-MM-dd HH:mm:ss} (in {FormatRelativeTime(handle.Remaining)})");
            }
            else
            {
                // This should not happen. We either create or retrieve existing handle.
                throw new Exception("Handle should not be null.");
            }

            return false;
        }

        return true;
    }

    #endregion
}
