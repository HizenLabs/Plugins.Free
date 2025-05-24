using Carbon.Base;
using Carbon.Modules;
using Cysharp.Threading.Tasks;
using HizenLabs.Extensions.UserPreference.UI;
using Oxide.Game.Rust.Cui;
using System;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private const string CommandPrefix = nameof(AutoBuildSnapshot) + ".";

    /// <summary>
    /// Handles the command to toggle the menu for the AutoBuildSnapshot plugin.
    /// </summary>
    /// <param name="player">The player who initiated the command.</param>
    /// <param name="command">The command that was executed.</param>
    /// <param name="args">The arguments passed to the command.</param>
    private void CommandToggleMenu(BasePlayer player, string command, string[] args)
    {
        if (!CheckPermission(player, Settings.Commands.Rollback)) return;

        UserInterface.ToggleMenu(player);
    }

    /// <summary>
    /// Handles the command to create a backup of the current state of a base.
    /// </summary>
    /// <param name="player">The player who initiated the command.</param>
    /// <param name="command">The command that was executed.</param>
    /// <param name="args">The arguments passed to the command.</param>
    private void CommandBackup(BasePlayer player, string command, string[] args)
    {
        if (!CheckPermission(player, Settings.Commands.Rollback)) return;
        if (!TryGetArgument(player, args, out ulong recordingId)) return;

        if (!ChangeManagement.Recordings.TryGetValue(recordingId, out var recording))
        {
            Localizer.ChatMessage(player, LangKeys.error_recording_not_found, recordingId);
            return;
        }

        recording.AttemptSaveAsync(player).ContinueWith(CommandBackup_Complete).Forget();
    }

    /// <summary>
    /// Handles the completion of a backup attempt.
    /// </summary>
    /// <param name="saveAttempt">The result of the save attempt.</param>
    private void CommandBackup_Complete(ChangeManagement.SaveAttempt saveAttempt)
    {
        if (saveAttempt.Success)
        {
            Localizer.ChatMessage(saveAttempt.Player, LangKeys.message_backup_success, saveAttempt.RecordingId, saveAttempt.Duration);
        }
        else if (saveAttempt.Exception != null)
        {
            Localizer.ChatMessage(saveAttempt.Player, LangKeys.error_backup_failed_exception, saveAttempt.RecordingId, saveAttempt.Exception.Message);
        }
        else
        {
            Localizer.ChatMessage(saveAttempt.Player, LangKeys.error_backup_failed, saveAttempt.RecordingId);
        }
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
        if (!TryGetArgument(player, args, out Guid backupId)) return;

        throw new NotImplementedException();
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

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuSettings))]
    private void CommandMenuSettings(BasePlayer player, string command, string[] args)
    {
        UserPreferenceUI.Show(this, player, onConfirm: () => UserInterface.ShowMenu(player));
    }

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuSwitchTab))]
    private void CommandMenuSwitchTab(BasePlayer player, string command, string[] args)
    {
        if (!CheckPermission(player, Settings.Commands.Rollback)) return;
        if (!TryGetArgument(player, args, out int tabIndex)) return;

        UserInterface.SwitchTab(player, tabIndex);
    }

    [ProtectedCommand(CommandPrefix + nameof(CommandMenuClearLogs))]
    private void CommandMenuClearLogs(BasePlayer player, string command, string[] args)
    {
        if (!Settings.Commands.HasAdminPermission(player)) return;

        UserInterface.ShowConfirmation(player, CommandMenuClearLogs_OnConfirm, LangKeys.menu_confirm_clear_logs);
    }

    private void CommandMenuClearLogs_OnConfirm(BasePlayer player)
    {
        Helpers.ClearLogs();

        UserInterface.ShowMenu(player);
    }

    [ProtectedCommand(CommandPrefix + nameof(CommandHandleOnConfirm))]
    private void CommandHandleOnConfirm(BasePlayer player, string command, string[] args)
    {
        var onConfirm = player.AutoBuildSnapshot_OnConfirm;

        ConfirmClose(player);

        onConfirm?.Invoke();
    }

    [ProtectedCommand(CommandPrefix + nameof(CommandHandleOnCancel))]
    private void CommandHandleOnCancel(BasePlayer player, string command, string[] args)
    {
        ConfirmClose(player);
    }

    private void ConfirmClose(BasePlayer player)
    {
        CuiHelper.DestroyUi(player, UserInterface.ConfirmMenuId);

        player.AutoBuildSnapshot_OnConfirm = null;
    }

    /// <summary>
    /// Checks if the player has the required permissions to execute a command.
    /// </summary>
    /// <param name="player">The player who initiated the command.</param>
    /// <param name="command">The command settings to check against.</param>
    /// <returns>True if the player has permission, false otherwise.</returns>
    private static bool CheckPermission(BasePlayer player, AutoBuildSnapshotConfig.CommandSetting command)
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
}
