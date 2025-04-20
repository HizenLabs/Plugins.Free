using Carbon.Components;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // Simplify names

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Shows a confirmation dialog to the player.
    /// </summary>
    /// <param name="player">The player to show the dialog to.</param>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message to display in the dialog.</param>
    /// <param name="confirmCommand">The command to execute if the player confirms.</param>
    private LUI.LuiContainer RenderConfirmationDialog(Components.CUI cui, BasePlayer player, string title, string message, string confirmCommand)
    {
        // Create the base container with cursor
        var container = cui.v2
            .CreateParent(
                parent: CUI.ClientPanels.Overlay,
                position: LuiPosition.Full,
                name: _confirmationDialogId)
            .AddCursor();

        // Semi-transparent overlay
        cui.v2
            .CreatePanel(
                container: container,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "0 0 0 0.7"
            );

        // Dialog panel
        var dialogPanel = cui.v2
            .CreatePanel(
                container: container,
                position: LuiPosition.MiddleCenter,
                offset: new(-200, -125, 200, 125),
                color: "0.1 0.1 0.1 0.95"
            );

        // Dialog border
        cui.v2
            .CreatePanel(
                container: dialogPanel,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "0.3 0.3 0.3 1"
            )
            .SetOutline("0.5 0.5 0.5 1", new(2, 2));

        // Dialog content
        var contentPanel = cui.v2
            .CreatePanel(
                container: dialogPanel,
                position: new(0.01f, 0.01f, 0.99f, 0.99f),
                offset: LuiOffset.None,
                color: "0.15 0.15 0.15 1"
            );

        // Title bar
        var titleBar = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0.85f, 1, 1),
                offset: LuiOffset.None,
                color: "0.2 0.2 0.3 1"
            );

        // Title
        cui.v2.CreateText(
            container: titleBar,
            position: new(0, 0, 1, 1),
            offset: new(15, 0, -15, 0),
            color: "1 1 1 0.95",
            fontSize: 16,
            text: title,
            alignment: TextAnchor.MiddleCenter
        ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Message area
        var messageArea = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0.2f, 1, 0.85f),
                offset: new(15, 10, -15, -10),
                color: "0 0 0 0"
            );

        // Message text
        cui.v2.CreateText(
            container: messageArea,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 0.9",
            fontSize: 14,
            text: message,
            alignment: TextAnchor.MiddleCenter
        );

        // Buttons area
        var buttonsArea = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0.03f, 1, 0.2f),
                offset: LuiOffset.None,
                color: "0 0 0 0"
            );

        // Confirm button
        var confirmBtn = cui.v2
            .CreateButton(
                container: buttonsArea,
                position: new(0.15f, 0.2f, 0.45f, 0.8f),
                offset: LuiOffset.None,
                command: confirmCommand,
                color: "0.3 0.5 0.3 1"
            );

        // Confirm text
        cui.v2.CreateText(
            container: confirmBtn,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 0.95",
            fontSize: 14,
            text: "Confirm",
            alignment: TextAnchor.MiddleCenter
        );

        // Cancel button
        var cancelBtn = cui.v2
            .CreateButton(
                container: buttonsArea,
                position: new(0.55f, 0.2f, 0.85f, 0.8f),
                offset: LuiOffset.None,
                command: $"{nameof(AutoBuildSnapshot)}.confirm.cancel",
                color: "0.5 0.3 0.3 1"
            );

        // Cancel text
        cui.v2.CreateText(
            container: cancelBtn,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 0.95",
            fontSize: 14,
            text: "Cancel",
            alignment: TextAnchor.MiddleCenter
        );

        return container;
    }
}
