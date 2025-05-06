using Carbon.Components;
using Facepunch;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // Simplify names

public partial class AutoBuildSnapshot_old
{
    /// <summary>
    /// Sends the main UI to the player.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    private LUI.LuiContainer RenderMainMenu(Components.CUI cui, BasePlayer player)
    {
        var title = _lang.GetMessage(LangKeys.ui_main_title, player);

        var container = RenderBasicLayout(cui, _mainMenuId, title, out var main, out var header);

        // Tabs at the top
        var tabsPanel = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, .88f, 1, .94f),
                offset: LuiOffset.None,
                color: "0.2 0.2 0.2 1"
            );

        if (!_currentMenuTab.TryGetValue(player.userID, out var tab))
        {
            tab = MenuTab.Records;
            _currentMenuTab[player.userID] = tab;
        }

        const string highlightColor = "0.3 0.5 0.3 1";
        const string defaultColor = "0.2 0.2 0.2 1";

        // Records tab button
        var recordsTabButton = cui.v2
            .CreateButton(
                container: tabsPanel,
                position: new(0, 0, .5f, 1),
                offset: LuiOffset.None,
                command: $"{nameof(AutoBuildSnapshot_old)}.{nameof(CommandMainMenuTabRecords)}",
                color: tab == MenuTab.Records ? highlightColor : defaultColor
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
                command: $"{nameof(AutoBuildSnapshot_old)}.{nameof(CommandMainMenuTabLogs)}",
                color: tab == MenuTab.Logs ? highlightColor : defaultColor
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
                position: new(0, .05f, 1, .88f),
                offset: LuiOffset.None,
                color: "0.15 0.15 0.15 1"
            );

        switch (tab)
        {
            case MenuTab.Records:
                RenderRecordsPanel(player, cui, contentPanel);
                break;

            case MenuTab.Logs:
                RenderLogsPanel(player, cui, contentPanel);
                break;

            default:
                throw new NotImplementedException($"Menu tab {tab} is not implemented.");
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
            position: new(0, .91f, 1, 1),
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
                position: new(0, 0, 1, .92f),
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
                var pos = record.BaseTC.ServerPosition;
                var teleportButton = cui.v2
                    .CreateButton(
                        container: recordItem,
                        position: new(.65f, .2f, .8f, .8f),
                        offset: LuiOffset.None,
                        command: $"{nameof(AutoBuildSnapshot_old)}.{nameof(CommandGlobalTeleport)} {pos.x} {pos.y} {pos.z}",
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
                        command: $"{nameof(AutoBuildSnapshot_old)}.{nameof(CommandMainMenuOpenSnapshots)} {record.NetworkID}",
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
                            command: $"{nameof(AutoBuildSnapshot_old)}.{nameof(CommandMainMenuScrollRecords)} -1",
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
                            command: $"{nameof(AutoBuildSnapshot_old)}.{nameof(CommandMainMenuScrollRecords)} 1",
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
            position: new(0, .91f, 1, 1),
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
                position: new(0, 0, 1, .92f),
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
                position: new(.85f, .91f, .98f, .98f),
                offset: LuiOffset.None,
                command: $"{nameof(AutoBuildSnapshot_old)}.{nameof(CommandMainMenuClearLogs)}",
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
