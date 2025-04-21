using Carbon.Components;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Carbon.Plugins;

#pragma warning disable IDE0001 // Simplify names

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Sends the snapshots menu to the player.
    /// </summary>
    /// <param name="player">The player to show the menu to.</param>
    /// <param name="record">The building record being viewed, or null if it's a search.</param>
    /// <param name="snapshots">The list of snapshots for the record.</param>
    private LUI.LuiContainer RenderSnapshotsMenu(Components.CUI cui, BasePlayer player, BuildRecord record, List<System.Guid> snapshots)
    {
        var title = record == null
            ? "Snapshot results"
            : $"Snapshots for Building at {record.BaseTC.ServerPosition:F1}";

        var container = RenderBasicLayout(cui, title, out var main, out var header);

        if (record != null)
        {
            // Back button
            var backButton = cui.v2
                .CreateButton(
                    container: header,
                    position: new(.8f, .15f, .93f, .85f),
                    offset: new(4, 0, 4, 0),
                    command: $"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsNavigateBack)}",
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
        }

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

        // Create the snapshot list
        CreateSnapshotsList(cui, leftPanel, player, snapshots);

        // Right panel (snapshot details)
        var rightPanel = cui.v2
            .CreatePanel(
                container: contentPanel,
                position: new(.35f, 0, 1, 1),
                offset: new(5, 10, -10, -10),
                color: "0.15 0.15 0.15 1"
            );

        // Create the snapshot details area
        CreateSnapshotsDetail(cui, rightPanel, player, snapshots);

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
                command: $"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsShowZones)}",
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
                command: $"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsRollback)}",
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

        // TODO: Show undo button if a rollback was completed.
        bool showUndoButton = false;
        if (showUndoButton)
        {
            // Undo button
            var undoButton = cui.v2
                .CreateButton(
                    container: buttonPanel,
                    position: new(.7f, .2f, .95f, .8f),
                    offset: new(5, 0, -10, 0),
                    command: $"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsUndoRollback)}",
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

        return container;
    }

    private void CreateSnapshotsList(Components.CUI cui, Components.LUI.LuiContainer leftPanel, BasePlayer player, List<System.Guid> snapshots)
    {
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

            return;
        }

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
                    command: $"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsSelect)} {snapshotId}",
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
                        command: $"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsScroll)} -1",
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
                        command: $"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsScroll)} 1",
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

    private void CreateSnapshotsDetail(Components.CUI cui, Components.LUI.LuiContainer rightPanel, BasePlayer player, List<System.Guid> snapshots)
    {
        if (snapshots.Count == 0
            || !_currentSelectedSnapshot.TryGetValue(player.userID, out var selectedSnapshot)
            || !_snapshotMetaData.TryGetValue(selectedSnapshot, out var snapshotData))
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
            return;
        }

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
                    command: $"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsTeleportToBuilding)} {building.Key}",
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
}
