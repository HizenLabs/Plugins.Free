using Carbon.Components;
using Facepunch;
using System.Collections.Generic;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // Simplify names

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Sends the main UI to the player.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    private LUI.LuiContainer RenderMainMenu(Components.CUI cui, BasePlayer player)
    {
        // Create the base container with cursor
        var container = cui.v2
            .CreateParent(
                parent: CUI.ClientPanels.HudMenu,
                position: LuiPosition.Full,
                name: _mainMenuId)
            .AddCursor();

        // Main panel
        var main = cui.v2
            .CreatePanel(
                container: container,
                position: LuiPosition.MiddleCenter,
                offset: new(-450, -275, 450, 275),
                color: "0 0 0 .95"
            );

        // Header
        var header = cui.v2
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
                text: _lang.GetMessage(LangKeys.ui_main_title, player),
                alignment: TextAnchor.MiddleLeft
            )
            .SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Close button
        var closeButton = cui.v2
            .CreateButton(
                container: header,
                position: new(.965f, .15f, .99f, .85f),
                offset: new(4, 0, 4, 0),
                command: $"{nameof(AutoBuildSnapshot)}.menu.close",
                color: ".6 .2 .2 .9"
            );

        cui.v2.CreateImageFromDb(
            container: closeButton,
            position: new(.2f, .2f, .8f, .8f),
            offset: LuiOffset.None,
            dbName: "close",
            color: "1 1 1 .5"
        );

        // Tabs at the top
        var tabsPanel = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, .90f, 1, .94f),
                offset: LuiOffset.None,
                color: "0.2 0.2 0.2 1"
            );

        // Records tab button
        var recordsTabButton = cui.v2
            .CreateButton(
                container: tabsPanel,
                position: new(0, 0, .5f, 1),
                offset: LuiOffset.None,
                command: $"{nameof(AutoBuildSnapshot)}.menu.tab.records",
                color: "0.3 0.3 0.3 1"
            );

        cui.v2.CreateText(
            container: recordsTabButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 .8",
            fontSize: 14,
            text: "Tracked Buildings",
            alignment: TextAnchor.MiddleCenter
        );

        // Logs tab button
        var logsTabButton = cui.v2
            .CreateButton(
                container: tabsPanel,
                position: new(.5f, 0, 1, 1),
                offset: LuiOffset.None,
                command: $"{nameof(AutoBuildSnapshot)}.menu.tab.logs",
                color: "0.2 0.2 0.2 1"
            );

        cui.v2.CreateText(
            container: logsTabButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 .8",
            fontSize: 14,
            text: "Logs",
            alignment: TextAnchor.MiddleCenter
        );

        // Content area
        var contentPanel = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, .05f, 1, .90f),
                offset: LuiOffset.None,
                color: "0.15 0.15 0.15 1"
            );

        if (_currentMenuTab.TryGetValue(player.userID, out var tab))
        {
            switch (tab)
            {
                case MenuTab.Records:
                    RenderRecordsPanel(player, cui, contentPanel);
                    break;
                case MenuTab.Logs:
                    RenderLogsPanel(player, cui, contentPanel);
                    break;
            }
        }
        else
        {
            // Default to records tab
            _currentMenuTab[player.userID] = MenuTab.Records;
            RenderRecordsPanel(player, cui, contentPanel);
        }

        return container;
    }

    /// <summary>
    /// Shows the records list in the content panel.
    /// </summary>
    /// <param name="player">The player viewing the UI.</param>
    /// <param name="cui">The CUI instance.</param>
    /// <param name="contentPanel">The content panel to fill.</param>
    private void RenderRecordsPanel(BasePlayer player, Components.CUI cui, Components.LUI.LuiContainer contentPanel)
    {
        // Title
        cui.v2.CreateText(
            container: contentPanel,
            position: new(0, .95f, 1, 1),
            offset: new(10, 0, -10, 0),
            color: "1 1 1 .8",
            fontSize: 14,
            text: "Building Records",
            alignment: TextAnchor.MiddleLeft
        ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Records list area with scrolling
        var recordsScrollContainer = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0, 1, .95f),
                offset: new(10, 10, -10, -5),
                color: "0 0 0 0"
            );

        var recordsList = Pool.Get<List<BuildRecord>>();
        recordsList.AddRange(_buildRecords.Values);

        if (recordsList.Count == 0)
        {
            // No records message
            cui.v2.CreateText(
                container: recordsScrollContainer,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "1 1 1 .5",
                fontSize: 14,
                text: "There are currently no records being tracked.",
                alignment: TextAnchor.MiddleCenter
            );
        }
        else
        {
            float itemHeight = 0.08f;
            float totalHeight = itemHeight * recordsList.Count;
            float visibleHeight = 1f;
            bool needsScrolling = totalHeight > visibleHeight;

            // Calculate visible records
            int visibleRecordsCount = Mathf.FloorToInt(visibleHeight / itemHeight);
            int topIndex = _playerRecordScrollIndex.TryGetValue(player.userID, out int index) ? index : 0;

            // Clamp the top index
            topIndex = Mathf.Clamp(topIndex, 0, Mathf.Max(0, recordsList.Count - visibleRecordsCount));
            _playerRecordScrollIndex[player.userID] = topIndex;

            // Records list
            for (int i = topIndex; i < Mathf.Min(topIndex + visibleRecordsCount, recordsList.Count); i++)
            {
                var record = recordsList[i];
                float yMin = 1f - (i - topIndex + 1) * itemHeight;
                float yMax = 1f - (i - topIndex) * itemHeight;

                // Record item background (alternate colors)
                var recordItem = cui.v2
                    .CreatePanel(
                        container: recordsScrollContainer,
                        position: new(0, yMin, 1, yMax),
                        offset: LuiOffset.None,
                        color: i % 2 == 0 ? "0.2 0.2 0.2 0.5" : "0.25 0.25 0.25 0.5"
                    );

                // TC position text
                cui.v2.CreateText(
                    container: recordItem,
                    position: new(0, 0, .6f, 1),
                    offset: new(10, 0, 0, 0),
                    color: "1 1 1 .8",
                    fontSize: 12,
                    text: $"TC: {record.BaseTC.ServerPosition:F1} | Status: {record.State}",
                    alignment: TextAnchor.MiddleLeft
                );

                // Teleport button
                var teleportButton = cui.v2
                    .CreateButton(
                        container: recordItem,
                        position: new(.65f, .2f, .8f, .8f),
                        offset: LuiOffset.None,
                        command: $"{nameof(AutoBuildSnapshot)}.teleport {record.NetworkID}",
                        color: "0.3 0.5 0.3 1"
                    );

                cui.v2.CreateText(
                    container: teleportButton,
                    position: LuiPosition.Full,
                    offset: LuiOffset.None,
                    color: "1 1 1 1",
                    fontSize: 12,
                    text: "Teleport",
                    alignment: TextAnchor.MiddleCenter
                );

                // Snapshots button
                var snapshotsButton = cui.v2
                    .CreateButton(
                        container: recordItem,
                        position: new(.82f, .2f, .97f, .8f),
                        offset: LuiOffset.None,
                        command: $"{nameof(AutoBuildSnapshot)}.snapshots {record.NetworkID}",
                        color: "0.3 0.3 0.5 1"
                    );

                cui.v2.CreateText(
                    container: snapshotsButton,
                    position: LuiPosition.Full,
                    offset: LuiOffset.None,
                    color: "1 1 1 1",
                    fontSize: 12,
                    text: "Snapshots",
                    alignment: TextAnchor.MiddleCenter
                );
            }

            // Add scrolling controls if needed
            if (needsScrolling)
            {
                // Scroll up button
                if (topIndex > 0)
                {
                    var scrollUpButton = cui.v2
                        .CreateButton(
                            container: contentPanel,
                            position: new(.96f, .9f, .99f, .95f),
                            offset: LuiOffset.None,
                            command: $"{nameof(AutoBuildSnapshot)}.scroll.records -1",
                            color: "0.3 0.3 0.3 1"
                        );

                    cui.v2.CreateText(
                        container: scrollUpButton,
                        position: LuiPosition.Full,
                        offset: LuiOffset.None,
                        color: "1 1 1 1",
                        fontSize: 14,
                        text: "▲",
                        alignment: TextAnchor.MiddleCenter
                    );
                }

                // Scroll down button
                if (topIndex < recordsList.Count - visibleRecordsCount)
                {
                    var scrollDownButton = cui.v2
                        .CreateButton(
                            container: contentPanel,
                            position: new(.96f, .05f, .99f, .1f),
                            offset: LuiOffset.None,
                            command: $"{nameof(AutoBuildSnapshot)}.scroll.records 1",
                            color: "0.3 0.3 0.3 1"
                        );

                    cui.v2.CreateText(
                        container: scrollDownButton,
                        position: LuiPosition.Full,
                        offset: LuiOffset.None,
                        color: "1 1 1 1",
                        fontSize: 14,
                        text: "▼",
                        alignment: TextAnchor.MiddleCenter
                    );
                }
            }
        }

        Pool.FreeUnmanaged(ref recordsList);
    }

    /// <summary>
    /// Shows the logs panel in the content area.
    /// </summary>
    /// <param name="player">The player viewing the UI.</param>
    /// <param name="cui">The CUI instance.</param>
    /// <param name="contentPanel">The content panel to fill.</param>
    private void RenderLogsPanel(BasePlayer player, Components.CUI cui, Components.LUI.LuiContainer contentPanel)
    {
        // Title
        cui.v2.CreateText(
            container: contentPanel,
            position: new(0, .95f, 1, 1),
            offset: new(10, 0, -10, 0),
            color: "1 1 1 .8",
            fontSize: 14,
            text: "Plugin Log Messages",
            alignment: TextAnchor.MiddleLeft
        ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Log area
        var logsPanel = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0, 1, .95f),
                offset: new(10, 10, -10, -5),
                color: "0.1 0.1 0.1 0.5"
            );

        // Get logs (This would need to be implemented in your plugin to track logs)
        List<string> logs = GetRecentLogs(20); // Get 20 most recent log entries

        if (logs.Count == 0)
        {
            // No logs message
            cui.v2.CreateText(
                container: logsPanel,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "1 1 1 .5",
                fontSize: 14,
                text: "No log messages available.",
                alignment: TextAnchor.MiddleCenter
            );
        }
        else
        {
            float itemHeight = 0.05f;
            float startY = 1f;

            // Display logs in reverse chronological order (newest at top)
            for (int i = 0; i < logs.Count; i++)
            {
                float yMin = startY - (i + 1) * itemHeight;
                float yMax = startY - i * itemHeight;

                cui.v2.CreateText(
                    container: logsPanel,
                    position: new(0, yMin, 1, yMax),
                    offset: new(5, 0, -5, 0),
                    color: "1 1 1 .7",
                    fontSize: 12,
                    text: logs[i],
                    alignment: TextAnchor.MiddleLeft
                );
            }
        }

        // Clear logs button
        var clearButton = cui.v2
            .CreateButton(
                container: contentPanel,
                position: new(.85f, .95f, .98f, .98f),
                offset: LuiOffset.None,
                command: $"{nameof(AutoBuildSnapshot)}.logs.clear",
                color: "0.5 0.3 0.3 1"
            );

        cui.v2.CreateText(
            container: clearButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 1",
            fontSize: 12,
            text: "Clear Logs",
            alignment: TextAnchor.MiddleCenter
        );
    }
}
