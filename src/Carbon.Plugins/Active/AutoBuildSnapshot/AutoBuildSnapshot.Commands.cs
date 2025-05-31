using Cysharp.Threading.Tasks;
using HizenLabs.Extensions.UserPreference.UI;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private const string CommandPrefix = nameof(AutoBuildSnapshot) + ".";

    #region Covalence Commands

    /// <summary>
    /// Handles the command to toggle the menu for the AutoBuildSnapshot plugin.
    /// </summary>
    /// <param name="player">The player who initiated the command.</param>
    /// <param name="command">The command that was executed.</param>
    /// <param name="args">The arguments passed to the command.</param>
    private void CommandToggleMenu(BasePlayer player, string command, string[] args)
    {
        if (!CheckPermission(player, Settings.Commands.ToggleMenu)) return;

        UserInterface.ToggleMenu(player);
    }

    /*
    /// <summary>
    /// Handles the command to create a backup of the current state of a base.
    /// </summary>
    /// <param name="player">The player who initiated the command.</param>
    /// <param name="command">The command that was executed.</param>
    /// <param name="args">The arguments passed to the command.</param>
    private void CommandBackup(BasePlayer player, string command, string[] args)
    {
        if (!CheckPermission(player, Settings.Commands.Backup)) return;
        if (!TryGetArgument(player, args, out ChangeManagement.RecordingId recordingId)) return;

        // Open backup menu for nearest base

        throw new NotImplementedException();
    }

    /// <summary>
    /// Handles the command to restore a backup to a previous state.
    /// </summary>
    /// <param name="player">The player who initiated the command.</param>
    /// <param name="command">The command that was executed.</param>
    /// <param name="args">The arguments passed to the command.</param>
    private void CommandRollback(BasePlayer player, string command, string[] args)
    {
        if (!CheckPermission(player, Settings.Commands.Rollback)) return;

        // Open rollback menu for nearest base

        throw new NotImplementedException();
    }
    */

    #endregion

    #region Menu Commands

    #region Global

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuSettings))]
    private void CommandMenuSettings(BasePlayer player, string command, string[] args)
    {
        UserPreferenceUI.Show(this, player, onConfirm: () => UserInterface.ShowMenu(player));
    }

    /// <summary>
    /// Handles the command to close the menu for the AutoBuildSnapshot plugin.
    /// </summary>
    /// <param name="player">The player who initiated the command.</param>
    /// <param name="command">The command that was executed.</param>
    /// <param name="args">The arguments passed to the command.</param>
    [ProtectedCommand(CommandPrefix + nameof(CommandMenuClose))]
    private void CommandMenuClose(BasePlayer player, string command, string[] args)
    {
        // no need to check perms here as we're just closing the menu
        UserInterface.HideMenu(player);
    }

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuSwitchTab))]
    private void CommandMenuSwitchTab(BasePlayer player, string command, string[] args)
    {
        if (!CheckPermission(player, Settings.Commands.ToggleMenu)) return;
        if (!TryGetArgument(player, args, out int tabIndex)) return;

        UserInterface.SwitchTab(player, tabIndex);
    }

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuConfirmConfirm))]
    private void CommandMenuConfirmConfirm(BasePlayer player, string command, string[] args)
    {
        var onConfirm = player.AutoBuildSnapshot_OnConfirm;

        ConfirmClose(player);

        onConfirm?.Invoke();
    }

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuConfirmCancel))]
    private void CommandMenuConfirmCancel(BasePlayer player, string command, string[] args)
    {
        ConfirmClose(player);
    }

    private static void ConfirmClose(BasePlayer player)
    {
        CuiHelper.DestroyUi(player, UserInterface.ConfirmMenuId);

        player.AutoBuildSnapshot_OnConfirm = null;
    }

    #endregion

    #region Home

    #region Record Navigation

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuSelectRecord))]
    private void CommandMenuSelectRecord(BasePlayer player, string command, string[] args)
    {
        if (!CheckPermission(player, Settings.Commands.ToggleMenu)) return;
        if (!TryGetArgument(player, args, out int selectionId)) return;

        player.AutoBuildSnapshot_SelectedRecordIndex = selectionId;

        // Reset snapshot selection when changing record
        player.AutoBuildSnapshot_SelectedSnapshotIndex = 0;

        UserInterface.ShowMenu(player);
    }

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuSelectSnapshot))]
    private void CommandMenuSelectSnapshot(BasePlayer player, string command, string[] args)
    {
        if (!CheckPermission(player, Settings.Commands.ToggleMenu)) return;
        if (!TryGetArgument(player, args, out int selectionId)) return;

        player.AutoBuildSnapshot_SelectedSnapshotIndex = selectionId;

        UserInterface.ShowMenu(player);
    }

    #endregion

    #region Teleport

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuTeleport))]
    private void CommandMenuTeleport(BasePlayer player, string command, string[] args)
    {
        if (!Settings.Commands.HasAdminPermission(player)) return;
        if (!TryGetSelectedRecordingId(player, out var recordingId)) return;

        var meta = SaveManager.GetLastSave(recordingId);

        Vector3 offsetY = new(0, .5f, 0);
        var destination = Helpers.GetPosition(meta.OriginPosition, meta.OriginRotation, 1.5f);
        destination = Helpers.FindNearestTeleportPosition(destination + offsetY, .5f, 12f, 4, 25) - offsetY;

        UserInterface.ShowConfirmation(player, player => CommandMenuTeleport_OnConfirm(player, destination), LangKeys.menu_content_teleport_confirm, destination);
    }

    private void CommandMenuTeleport_OnConfirm(BasePlayer player, Vector3 destination)
    {
        Localizer.ChatMessage(player, LangKeys.menu_content_teleport_message, destination);
        player.Teleport(destination);

        // Refresh the menu and update the section index to the first (nearest) one
        player.AutoBuildSnapshot_SelectedRecordIndex = 0;
        UserInterface.ShowMenu(player);
    }

    #endregion

    #region Backup

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuBackup))]
    private void CommandMenuBackup(BasePlayer player, string command, string[] args)
    {
        if (!Settings.Commands.Backup.HasPermission(player)) return;
        if (!TryGetSelectedRecordingId(player, out var recordingId)) return;

        UserInterface.ShowConfirmation(player, player => CommandMenuBackup_OnConfirm(player, recordingId), LangKeys.menu_content_backup_confirm, recordingId.Position);
    }

    private void CommandMenuBackup_OnConfirm(BasePlayer player, ChangeManagement.RecordingId recordingId)
    {
        if (!ChangeManagement.Recordings.TryGetValue(recordingId, out var recording))
        {
            Localizer.ChatMessage(player, LangKeys.error_record_not_found, recordingId);
        }

        recording.AttemptSaveAsync(player).Forget();
    }

    #endregion

    #region Rollback

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuRollback))]
    private void CommandMenuRollback(BasePlayer player, string command, string[] args)
    {
        if (!Settings.Commands.Rollback.HasPermission(player)) return;
        if (!TryGetSelectedRecordingId(player, out var recordingId)) return;

        if (player.AutoBuildSnapshot_SnapshotOptions is not List<MetaInfo> snapshotsMeta
            || snapshotsMeta.Count == 0
            || snapshotsMeta.Count <= player.AutoBuildSnapshot_SelectedSnapshotIndex)
        {
            Localizer.ChatMessage(player, LangKeys.error_invalid_snapshot);
            return;
        }

        var meta = snapshotsMeta[player.AutoBuildSnapshot_SelectedSnapshotIndex];
        if (meta.RecordId != recordingId)
        {
            Localizer.ChatMessage(player, LangKeys.error_record_snapshot_mismatch);

            // Reset the selected indices
            player.AutoBuildSnapshot_SelectedRecordIndex = 0;
            player.AutoBuildSnapshot_SelectedSnapshotIndex = 0;

            UserInterface.ShowMenu(player);
            return;
        }

        UserInterface.ShowConfirmation(player, CommandMenuRollback_OnConfirm, LangKeys.menu_content_rollback_confirm, recordingId.Position, meta.TimeStamp);
    }

    private void CommandMenuRollback_OnConfirm(BasePlayer player)
    {
        _instance.Puts("Rollback not yet implemented.");
    }

    #endregion

    #endregion

    #region Logs

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuClearLogs))]
    private void CommandMenuClearLogs(BasePlayer player, string command, string[] args)
    {
        if (!Settings.Commands.HasAdminPermission(player)) return;

        UserInterface.ShowConfirmation(player, CommandMenuClearLogs_OnConfirm, LangKeys.menu_content_clear_confirm);
    }

    private void CommandMenuClearLogs_OnConfirm(BasePlayer player)
    {
        Helpers.ClearLogs();

        UserInterface.ShowMenu(player);
    }

    #endregion

    #endregion

    #region Helpers

    /// <summary>
    /// Checks if the player has the required permissions to execute a command.
    /// </summary>
    /// <param name="player">The player who initiated the command.</param>
    /// <param name="command">The command settings to check against.</param>
    /// <returns>True if the player has permission, false otherwise.</returns>
    private static bool CheckPermission(BasePlayer player, AutoBuildSnapshotConfig.CommandPermission command)
    {
        if (!command.HasPermission(player))
        {
            Localizer.ChatMessage(player, LangKeys.error_no_permission);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Attempts to parse an argument from the command line into a specified type.
    /// </summary>
    /// <typeparam name="TArg">The type to parse the argument into.</typeparam>
    /// <param name="player">The player who initiated the command.</param>
    /// <param name="args">The arguments passed to the command.</param>
    /// <param name="value">The parsed value if successful.</param>
    /// <param name="index">The index of the argument to parse.</param>
    /// <returns>True if the argument was successfully parsed, false otherwise.</returns>
    private static bool TryGetArgument<TArg>(BasePlayer player, string[] args, out TArg value, int index = 0)
    {
        if (!CheckArgumentLength(player, args, index + 1))
        {
            value = default;
            return false;
        }

        if (!Helpers.TryParse(args[index], out value))
        {
            Localizer.ChatMessage(player, LangKeys.error_command_arg_parse_fail, index, typeof(TArg).Name, args[index]);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the arguments provided by the player are valid for the command.
    /// </summary>
    /// <param name="player">The player who initiated the command.</param>
    /// <param name="args">The arguments passed to the command.</param>
    /// <param name="minimumArgs">The minimum number of arguments.</param>
    /// <returns>True if the arguments are valid, false otherwise.</returns>
    private static bool CheckArgumentLength(BasePlayer player, string[] args, int minimumArgs = 1)
    {
        if (args.Length < minimumArgs)
        {
            Localizer.ChatMessage(player, LangKeys.error_command_args_length, minimumArgs, args.Length);
            return false;
        }

        return true;
    }

    private bool TryGetSelectedRecordingId(BasePlayer player, out ChangeManagement.RecordingId recordingId)
    {
        recordingId = default;

        var selectedIndex = player.AutoBuildSnapshot_SelectedRecordIndex;
        if (selectedIndex < 0)
        {
            Localizer.ChatMessage(player, LangKeys.error_no_record_selected);
            return false;
        }

        if (player.AutoBuildSnapshot_RecordOptions is not List<ChangeManagement.RecordingId> recordingIds)
        {
            Localizer.ChatMessage(player, LangKeys.error_no_record_selected);
            return false;
        }

        if (selectedIndex >= recordingIds.Count)
        {
            Localizer.ChatMessage(player, LangKeys.error_no_record_selected);
            return false;
        }

        recordingId = recordingIds[selectedIndex];
        return true;
    }

    #endregion
}
