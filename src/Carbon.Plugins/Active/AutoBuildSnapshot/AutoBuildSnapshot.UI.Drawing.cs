using Carbon.Components;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // Simplify names

/// <summary>
/// Consolidating common drawing functions for all the menus.
/// </summary>
public partial class AutoBuildSnapshot
{
    private LUI.LuiContainer RenderBasicLayout(Components.CUI cui, string title, out LUI.LuiContainer main, out LUI.LuiContainer header)
    {
        // Create the base container with cursor
        var container = cui.v2
            .CreateParent(
                parent: CUI.ClientPanels.HudMenu,
                position: LuiPosition.Full,
                name: _mainMenuId)
            .AddCursor();

        // Main panel
        main = cui.v2
            .CreatePanel(
                container: container,
                position: LuiPosition.MiddleCenter,
                offset: new(-450, -275, 450, 275),
                color: "0 0 0 .95"
            );

        // Header
        header = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, .94f, 1, 1),
                offset: LuiOffset.None,
                color: "0 0 0 .9"
            );

        // Title
        cui.v2.CreateText(
                container: header,
                position: new(.015f, 0.01f, .99f, .95f),
                offset: new(0, 0, 0, 0),
                color: "1 1 1 .8",
                fontSize: 18,
                text: title,
                alignment: TextAnchor.MiddleLeft
            )
            .SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Close button
        var closeButton = cui.v2
            .CreateButton(
                container: header,
                position: new(.965f, .15f, .99f, .85f),
                offset: new(4, 0, 4, 0),
                command: $"{nameof(AutoBuildSnapshot)}.{nameof(CommandGlobalMenuClose)}",
                color: ".6 .2 .2 .9"
            );

        cui.v2.CreateImageFromDb(
            container: closeButton,
            position: new(.2f, .2f, .8f, .8f),
            offset: LuiOffset.None,
            dbName: "close",
            color: "1 1 1 .5"
        );

        return container;
    }
}
