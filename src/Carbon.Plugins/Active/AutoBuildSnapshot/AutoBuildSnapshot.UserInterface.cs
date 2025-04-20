using Carbon.Components;
using Facepunch;
using Oxide.Game.Rust.Cui;
using Oxide.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // fully qualifying method params due to issues with codegen

public partial class AutoBuildSnapshot
{
    #region Drawing

    #region Main Menu

    /// <summary>
    /// Sends the main UI to the player.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    private void RenderMainMenu(Components.CUI cui, BasePlayer player)
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

        var recordsList = _buildRecords.Values.ToList();

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

    #endregion

    #region Snapshots Menu

    /// <summary>
    /// Sends the snapshots menu to the player.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    /// <param name="record">The building record being viewed.</param>
    /// <param name="snapshots">The list of snapshots for the record.</param>
    private void RenderSnapshotsMenu(Components.CUI cui, BasePlayer player, BuildRecord record, List<System.Guid> snapshots)
    {
        // Create the base container with cursor
        var container = cui.v2
            .CreateParent(
                parent: CUI.ClientPanels.HudMenu,
                position: LuiPosition.Full,
                name: _snapshotMenuId)
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
                position: new(.015f, 0.01f, .8f, .95f),
                offset: new(0, 0, 0, 0),
                color: "1 1 1 .8",
                fontSize: 18,
                text: $"Snapshots for Building at {record.BaseTC.ServerPosition:F1}",
                alignment: TextAnchor.MiddleLeft
            )
            .SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Back button
        var backButton = cui.v2
            .CreateButton(
                container: header,
                position: new(.8f, .15f, .93f, .85f),
                offset: new(4, 0, 4, 0),
                command: $"{nameof(AutoBuildSnapshot)}.menu.snapshots.back",
                color: "0.3 0.3 0.6 0.9"
            );

        cui.v2.CreateText(
            container: backButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 .9",
            fontSize: 14,
            text: "Back",
            alignment: TextAnchor.MiddleCenter
        );

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

        // Content area - split into left and right panels
        var contentPanel = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, .08f, 1, .94f),
                offset: LuiOffset.None,
                color: "0.15 0.15 0.15 0"
            );

        // Left panel (snapshot list)
        var leftPanel = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(0, 0, .35f, 1),
                offset: new(10, 10, -5, -10),
                color: "0.15 0.15 0.15 1"
            );

        // Title for snapshot list
        cui.v2.CreateText(
            container: leftPanel,
            position: new(0, .95f, 1, 1),
            offset: new(10, 0, -10, 0),
            color: "1 1 1 .8",
            fontSize: 14,
            text: "Available Snapshots",
            alignment: TextAnchor.MiddleLeft
        ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

        // Snapshot list
        var snapshotList = cui.v2
            .CreatePanel(
                container: leftPanel,
                position: new(0, 0, 1, .95f),
                offset: new(5, 5, -5, -5),
                color: "0 0 0 0"
            );

        if (snapshots.Count == 0)
        {
            // No snapshots message
            cui.v2.CreateText(
                container: snapshotList,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "1 1 1 .5",
                fontSize: 14,
                text: "No snapshots available for this building.",
                alignment: TextAnchor.MiddleCenter
            );
        }
        else
        {
            // List snapshots
            float itemHeight = 0.08f;
            int visibleCount = Mathf.Min(snapshots.Count, 10);

            // Get scroll position
            if (!_snapshotScrollIndex.TryGetValue(player.userID, out int scrollIndex))
            {
                _snapshotScrollIndex[player.userID] = 0;
                scrollIndex = 0;
            }

            // Clamp scroll index
            int maxScroll = Mathf.Max(0, snapshots.Count - visibleCount);
            scrollIndex = Mathf.Clamp(scrollIndex, 0, maxScroll);
            _snapshotScrollIndex[player.userID] = scrollIndex;

            // Get current selection
            Guid selectedId = Guid.Empty;
            _currentSelectedSnapshot.TryGetValue(player.userID, out selectedId);

            // Display snapshots
            for (int i = scrollIndex; i < scrollIndex + visibleCount && i < snapshots.Count; i++)
            {
                Guid snapshotId = snapshots[i];
                float yMin = 1f - (i - scrollIndex + 1) * itemHeight;
                float yMax = 1f - (i - scrollIndex) * itemHeight;

                bool isSelected = snapshotId == selectedId;

                // Background
                var itemBg = cui.v2
                    .CreatePanel(
                        container: snapshotList,
                        position: new(0, yMin, 1, yMax),
                        offset: new(0, 2, 0, -2),
                        color: isSelected ? "0.2 0.4 0.6 0.8" : (i % 2 == 0 ? "0.2 0.2 0.2 0.5" : "0.25 0.25 0.25 0.5")
                    );

                // Get metadata
                string displayText = "Unknown Snapshot";
                if (_snapshotMetaData.TryGetValue(snapshotId, out var meta))
                {
                    displayText = $"{meta.TimestampUTC:yyyy-MM-dd HH:mm} ({meta.Entities} entities)";
                }

                // Button to select snapshot
                cui.v2
                    .CreateButton(
                        container: itemBg,
                        position: LuiPosition.Full,
                        offset: LuiOffset.None,
                        command: $"{nameof(AutoBuildSnapshot)}.snapshots.select {snapshotId}",
                        color: "1 0 0 1"
                    )
                    .AddCursor();

                // Snapshot text
                cui.v2.CreateText(
                    container: itemBg,
                    position: new(0, 0, 1, 1),
                    offset: new(10, 0, -10, 0),
                    color: "1 1 1 .9",
                    fontSize: 12,
                    text: displayText,
                    alignment: TextAnchor.MiddleLeft
                );
            }

            // Add scroll buttons if needed
            if (snapshots.Count > visibleCount)
            {
                // Up button (if not at top)
                if (scrollIndex > 0)
                {
                    var upButton = cui.v2
                        .CreateButton(
                            container: leftPanel,
                            position: new(.9f, .95f, .98f, .99f),
                            offset: LuiOffset.None,
                            command: $"{nameof(AutoBuildSnapshot)}.snapshots.scroll -1",
                            color: "0.3 0.3 0.3 1"
                        );

                    cui.v2.CreateText(
                        container: upButton,
                        position: LuiPosition.Full,
                        offset: LuiOffset.None,
                        color: "1 1 1 1",
                        fontSize: 12,
                        text: "▲",
                        alignment: TextAnchor.MiddleCenter
                    );
                }

                // Down button (if not at bottom)
                if (scrollIndex < maxScroll)
                {
                    var downButton = cui.v2
                        .CreateButton(
                            container: leftPanel,
                            position: new(.9f, .01f, .98f, .05f),
                            offset: LuiOffset.None,
                            command: $"{nameof(AutoBuildSnapshot)}.snapshots.scroll 1",
                            color: "0.3 0.3 0.3 1"
                        );

                    cui.v2.CreateText(
                        container: downButton,
                        position: LuiPosition.Full,
                        offset: LuiOffset.None,
                        color: "1 1 1 1",
                        fontSize: 12,
                        text: "▼",
                        alignment: TextAnchor.MiddleCenter
                    );
                }
            }
        }

        // Right panel (snapshot details)
        var rightPanel = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(.35f, 0, 1, 1),
                offset: new(5, 10, -10, -10),
                color: "0.15 0.15 0.15 1"
            );

        // Show snapshot details if one is selected
        if (snapshots.Count > 0 && _currentSelectedSnapshot.TryGetValue(player.userID, out Guid selectedSnapshot) &&
            _snapshotMetaData.TryGetValue(selectedSnapshot, out var snapshotData))
        {
            // Snapshot details section
            var detailsHeader = cui.v2
                .CreatePanel(
                    container: rightPanel,
                    position: new(0, .9f, 1, 1),
                    offset: new(0, 0, 0, 0),
                    color: "0.2 0.2 0.3 1"
                );

            cui.v2.CreateText(
                container: detailsHeader,
                position: LuiPosition.Full,
                offset: new(10, 0, -10, 0),
                color: "1 1 1 1",
                fontSize: 14,
                text: "Snapshot Details",
                alignment: TextAnchor.MiddleLeft
            ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

            // Details content panel
            var detailsContent = cui.v2
                .CreatePanel(
                    container: rightPanel,
                    position: new(0, .3f, 1, .9f),
                    offset: new(10, 5, -10, -5),
                    color: "0.1 0.1 0.1 0.5"
                );

            // Timestamp
            cui.v2.CreateText(
                container: detailsContent,
                position: new(0, .85f, 1, 1),
                offset: new(5, 0, -5, 0),
                color: "1 1 1 .9",
                fontSize: 14,
                text: $"Timestamp: {snapshotData.TimestampUTC:yyyy-MM-dd HH:mm:ss} UTC",
                alignment: TextAnchor.MiddleLeft
            );

            // Entities count
            cui.v2.CreateText(
                container: detailsContent,
                position: new(0, .75f, 1, .85f),
                offset: new(5, 0, -5, 0),
                color: "1 1 1 .9",
                fontSize: 14,
                text: $"Total Entities: {snapshotData.Entities}",
                alignment: TextAnchor.MiddleLeft
            );

            // Linked buildings count
            cui.v2.CreateText(
                container: detailsContent,
                position: new(0, .65f, 1, .75f),
                offset: new(5, 0, -5, 0),
                color: "1 1 1 .9",
                fontSize: 14,
                text: $"Linked Buildings: {snapshotData.LinkedBuildings.Count}",
                alignment: TextAnchor.MiddleLeft
            );

            // Authorized users section
            cui.v2.CreateText(
                container: detailsContent,
                position: new(0, .55f, 1, .65f),
                offset: new(5, 0, -5, 0),
                color: "1 1 1 .9",
                fontSize: 14,
                text: $"Authorized Users: {snapshotData.AuthorizedPlayers.Count}",
                alignment: TextAnchor.MiddleLeft
            );

            // Display authorized users
            int authCount = Mathf.Min(snapshotData.AuthorizedPlayers.Count, 3); // Show up to 3 users
            for (int i = 0; i < authCount; i++)
            {
                var user = snapshotData.AuthorizedPlayers[i];
                cui.v2.CreateText(
                    container: detailsContent,
                    position: new(0.1f, .55f - ((i + 1) * 0.08f), 1, .55f - (i * 0.08f)),
                    offset: new(5, 0, -5, 0),
                    color: "0.9 0.9 0.9 .8",
                    fontSize: 12,
                    text: $"• {user.UserName} ({user.UserID})",
                    alignment: TextAnchor.MiddleLeft
                );
            }

            // Show "more users" if there are more than we displayed
            if (snapshotData.AuthorizedPlayers.Count > authCount)
            {
                int remainingUsers = snapshotData.AuthorizedPlayers.Count - authCount;
                cui.v2.CreateText(
                    container: detailsContent,
                    position: new(0.1f, .55f - ((authCount + 1) * 0.08f), 1, .55f - (authCount * 0.08f)),
                    offset: new(5, 0, -5, 0),
                    color: "0.7 0.7 0.7 .7",
                    fontSize: 12,
                    text: $"• And {remainingUsers} more user{(remainingUsers > 1 ? "s" : "")}...",
                    alignment: TextAnchor.MiddleLeft
                );
            }

            // Linked buildings section
            var linkedBuildingsPanel = cui.v2
                .CreatePanel(
                    container: rightPanel,
                    position: new(0, .1f, 1, .3f),
                    offset: new(10, 5, -10, -5),
                    color: "0.1 0.1 0.1 0.5"
                );

            cui.v2.CreateText(
                container: linkedBuildingsPanel,
                position: new(0, .85f, 1, 1),
                offset: new(5, 0, -5, 0),
                color: "1 1 1 .9",
                fontSize: 14,
                text: "Linked Buildings:",
                alignment: TextAnchor.MiddleLeft
            ).SetTextFont(CUI.Handler.FontTypes.RobotoCondensedBold);

            // List linked buildings
            int buildingCount = Mathf.Min(snapshotData.LinkedBuildings.Count, 3); // Show up to 3 buildings
            int buildingIndex = 0;

            foreach (var building in snapshotData.LinkedBuildings)
            {
                if (buildingIndex >= buildingCount) break;

                // Building container
                var buildingItem = cui.v2
                    .CreatePanel(
                        container: linkedBuildingsPanel,
                        position: new(0, .85f - ((buildingIndex + 1) * 0.2f), 1, .85f - (buildingIndex * 0.2f)),
                        offset: new(5, 2, -5, -2),
                        color: buildingIndex % 2 == 0 ? "0.15 0.15 0.15 0.7" : "0.18 0.18 0.18 0.7"
                    );

                // Building info
                cui.v2.CreateText(
                    container: buildingItem,
                    position: new(0, 0, .7f, 1),
                    offset: new(5, 0, -5, 0),
                    color: "1 1 1 .9",
                    fontSize: 12,
                    text: $"Position: {building.Value.Position:F1} | Entities: {building.Value.Entities}",
                    alignment: TextAnchor.MiddleLeft
                );

                // Teleport button
                var teleportButton = cui.v2
                    .CreateButton(
                        container: buildingItem,
                        position: new(.7f, .2f, .95f, .8f),
                        offset: LuiOffset.None,
                        command: $"{nameof(AutoBuildSnapshot)}.snapshots.teleport {building.Key}",
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

                buildingIndex++;
            }

            // Show "more buildings" if there are more than we displayed
            if (snapshotData.LinkedBuildings.Count > buildingCount)
            {
                int remainingBuildings = snapshotData.LinkedBuildings.Count - buildingCount;
                cui.v2.CreateText(
                    container: linkedBuildingsPanel,
                    position: new(0, .85f - ((buildingCount + 1) * 0.2f), 1, .85f - (buildingCount * 0.2f)),
                    offset: new(5, 0, -5, 0),
                    color: "0.7 0.7 0.7 .7",
                    fontSize: 12,
                    text: $"And {remainingBuildings} more building{(remainingBuildings > 1 ? "s" : "")}...",
                    alignment: TextAnchor.MiddleLeft
                );
            }
        }
        else
        {
            // No snapshot selected message
            cui.v2.CreateText(
                container: rightPanel,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "1 1 1 .5",
                fontSize: 14,
                text: "Select a snapshot to view details.",
                alignment: TextAnchor.MiddleCenter
            );
        }

        // Bottom buttons panel
        var buttonPanel = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, 0, 1, .08f),
                offset: new(10, 5, -10, -5),
                color: "0.2 0.2 0.2 1"
            );

        // Show Zones button
        var showZonesButton = cui.v2
            .CreateButton(
                container: buttonPanel,
                position: new(0, .2f, .25f, .8f),
                offset: new(10, 0, -5, 0),
                command: $"{nameof(AutoBuildSnapshot)}.snapshots.showzones",
                color: "0.3 0.3 0.6 1"
            );

        cui.v2.CreateText(
            container: showZonesButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 1",
            fontSize: 14,
            text: "Show Zones",
            alignment: TextAnchor.MiddleCenter
        );

        // Rollback button
        var rollbackButton = cui.v2
            .CreateButton(
                container: buttonPanel,
                position: new(.35f, .2f, .6f, .8f),
                offset: new(5, 0, -5, 0),
                command: $"{nameof(AutoBuildSnapshot)}.snapshots.rollback",
                color: "0.6 0.3 0.3 1"
            );

        cui.v2.CreateText(
            container: rollbackButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 1",
            fontSize: 14,
            text: "Rollback",
            alignment: TextAnchor.MiddleCenter
        );

        // Undo button
        var undoButton = cui.v2
            .CreateButton(
                container: buttonPanel,
                position: new(.7f, .2f, .95f, .8f),
                offset: new(5, 0, -10, 0),
                command: $"{nameof(AutoBuildSnapshot)}.snapshots.undo",
                color: "0.5 0.5 0.3 1"
            );

        cui.v2.CreateText(
            container: undoButton,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 1",
            fontSize: 14,
            text: "Undo",
            alignment: TextAnchor.MiddleCenter
        );
    }

    /// <summary>
    /// Gets the list of snapshots for the given record.
    /// </summary>
    /// <param name="record">The building record to get snapshots for.</param>
    /// <returns>A list of snapshot IDs for the record.</returns>
    private List<Guid> GetSnapshotsForRecord(BuildRecord record)
    {
        List<Guid> result = new();

        // Try to find snapshots by the persistent ID
        if (_buildingIDToSnapshotIndex.TryGetValue(record.PersistentID, out var snapshots))
        {
            result.AddRange(snapshots);
        }

        // Sort by timestamp (newest first)
        result.Sort((a, b) =>
        {
            if (_snapshotMetaData.TryGetValue(a, out var metaA) &&
                _snapshotMetaData.TryGetValue(b, out var metaB))
            {
                return metaB.TimestampUTC.CompareTo(metaA.TimestampUTC);
            }
            return 0;
        });

        return result;
    }

    #endregion

    #region Confirmation Dialog

    /// <summary>
    /// Shows a confirmation dialog to the player.
    /// </summary>
    /// <param name="player">The player to show the dialog to.</param>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message to display in the dialog.</param>
    /// <param name="confirmCommand">The command to execute if the player confirms.</param>
    private void RenderConfirmationDialog(Components.CUI cui, BasePlayer player, string title, string message, string confirmCommand)
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
    }

    #endregion

    #endregion

    #region Interface Buttons / Commands

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

    #endregion

    #region Navigation, Rollback, Logs

    /// <summary>
    /// Helper method to refresh the main menu with the specified tab.
    /// </summary>
    /// <param name="player">The player to refresh the menu for.</param>
    /// <param name="tab">The tab to show.</param>
    private void RefreshMenuWithTab(BasePlayer player, MenuTab tab)
    {
        _currentMenuTab[player.userID] = tab;

        NavigateMenu(player, MenuLayer.MainMenu);
    }

    /// <summary>
    /// Opens the snapshots menu for a specific building record.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    /// <param name="record">The building record to show snapshots for.</param>
    private void OpenSnapshotsMenu(BasePlayer player, BuildRecord record)
    {
        _currentBuildRecord[player.userID] = record.NetworkID;

        List<Guid> snapshots = GetSnapshotsForRecord(record);
        if (snapshots.Count > 0 && !_currentSelectedSnapshot.ContainsKey(player.userID))
        {
            _currentSelectedSnapshot[player.userID] = snapshots[0];
        }

        NavigateMenu(player, MenuLayer.Snapshots, true, record, snapshots);
    }

    /// <summary>
    /// Closes the confirmation dialog.
    /// </summary>
    /// <param name="player">The player to close the dialog for.</param>
    private void CloseConfirmationDialog(BasePlayer player)
    {
        CuiHelper.DestroyUi(player, _confirmationDialogId);
    }

    /// <summary>
    /// Navigates to the specified layer.
    /// </summary>
    private void NavigateMenu(
        BasePlayer player,
        MenuLayer targetLayer,
        bool appendLayer = false,
        params object[] args)
    {
        if (targetLayer == MenuLayer.Closed)
        {
            // don't do this, it's not supported
            if (appendLayer)
            {
                throw new Exception($"Cannot append layer '{nameof(MenuLayer.Closed)}'");
            }
        }
        else
        {
            // also don't do this, it's more complicated to navigate multiple layers in one go
            if ((targetLayer & (targetLayer - 1)) != 0)
            {
                throw new Exception("Cannot process multiple layers");
            }
        }

        if (!_playerMenuStates.TryGetValue(player.userID, out var currentLayer))
            currentLayer = MenuLayer.Closed;

        if (targetLayer == currentLayer)
        {
            if (targetLayer == MenuLayer.MainMenu && args.Length > 0 && args[0] is MenuTab newTab)
            {
                if (_currentMenuTab.TryGetValue(player.userID, out var currentTab) && currentTab == newTab)
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        _previousMenuState[player.userID] = currentLayer;

        if (appendLayer)
        {
            currentLayer |= targetLayer;
        }
        else
        {
            currentLayer = targetLayer;
        }

        if (currentLayer == MenuLayer.ConfirmationDialog && !targetLayer.HasFlag(MenuLayer.ConfirmationDialog))
        {
            CuiHelper.DestroyUi(player, _confirmationDialogId);
        }

        if (currentLayer == MenuLayer.Snapshots && !targetLayer.HasFlag(MenuLayer.Snapshots))
        {
            CuiHelper.DestroyUi(player, _snapshotMenuId);
        }

        if (currentLayer == MenuLayer.MainMenu && !targetLayer.HasFlag(MenuLayer.MainMenu))
        {
            CuiHelper.DestroyUi(player, _mainMenuId);
        }

        if (targetLayer == MenuLayer.Closed)
            return;

        using var cui = CreateCUI();

        switch (targetLayer)
        {
            case MenuLayer.MainMenu:
                RenderMainMenu(cui, player);
                break;

            case MenuLayer.Snapshots:
            case MenuLayer.ConfirmationDialog:
            default:
                throw new NotSupportedException($"Unsupported menu layer: {targetLayer}");
        }

        cui.v2.SendUi(player);

        _playerMenuStates[player.userID] = currentLayer;
    }

    /// <summary>
    /// Executes the rollback operation for the specified snapshot.
    /// </summary>
    /// <param name="player">The player who initiated the rollback.</param>
    /// <param name="snapshotId">The ID of the snapshot to rollback to.</param>
    private void ExecuteRollback(BasePlayer player, System.Guid snapshotId)
    {
        // This method would contain your implementation of the rollback process
        // For now, it just logs the action and notifies the player
        AddLogMessage($"Player {player.displayName} initiated rollback to snapshot {snapshotId}");
        player.ChatMessage($"Rollback to snapshot {snapshotId} initiated. This feature is not yet fully implemented.");

        // In a complete implementation, you would:
        // 1. Load the snapshot data
        // 2. Create a backup of the current state as an "undo" point
        // 3. Perform the rollback by removing/adding entities according to the snapshot
        // 4. Update all relevant tracking information
    }

    /// <summary>
    /// Executes the undo operation for the last rollback.
    /// </summary>
    /// <param name="player">The player who initiated the undo.</param>
    private void ExecuteUndo(BasePlayer player)
    {
        // This method would contain your implementation of the undo process
        // For now, it just logs the action and notifies the player
        AddLogMessage($"Player {player.displayName} initiated undo of last rollback");
        player.ChatMessage("Undo of last rollback initiated. This feature is not yet fully implemented.");

        // In a complete implementation, you would:
        // 1. Check if there is an undo point available
        // 2. If so, load the undo data and restore the previous state
        // 3. Update all relevant tracking information
    }

    private List<string> GetRecentLogs(int count)
    {
        return _logMessages.Take(Mathf.Min(count, _logMessages.Count)).ToList();
    }

    private void ClearLogMessages()
    {
        _logMessages.Clear();
        AddLogMessage("Log cleared");
    }

    // Helper methods to manage logs
    private void AddLogMessage(string message)
    {
        Puts(message);

        // Add timestamp to message
        string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";

        // Add to log list with a cap (e.g., 100 messages)
        _logMessages.Insert(0, timestampedMessage);
        if (_logMessages.Count > 100)
        {
            _logMessages.RemoveAt(_logMessages.Count - 1);
        }
    }

    #endregion

    #region Enums

    /// <summary>
    /// Represents the menu state for the player.
    /// </summary>
    [Flags]
    private enum MenuLayer
    {
        /// <summary>
        /// The menu is closed.
        /// </summary>
        Closed = 0,

        MainMenu = 1 << 0,

        Snapshots = 1 << 1,

        ConfirmationDialog = 1 << 2,
    }

    private enum MenuTab
    {
        Records,
        Logs
    }

    #endregion

    #region Debugging

    [ChatCommand($"debug")]
    private void CommandUIDebug(BasePlayer player, string command, string[] args)
    {
        Vector3 targetCoords;
        if (TryGetPlayerTargetEntity(player, out var target))
        {
            // Found target, get zones from target coords
            targetCoords = target.ServerPosition;
        }
        else if (!TryGetPlayerTargetCoordinates(player, out targetCoords))
        {
            // No target found, get coords from target
            player.ChatMessage(_lang.GetMessage(LangKeys.error_target_missing, player));
            return;
        }

    }

    #endregion
}
