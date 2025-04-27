using Carbon.Components;
using System.Collections.Generic;
using System;
using UnityEngine;
using Facepunch;
using System.Linq;

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
        // Check if we need to create a new snapshots list
        if (snapshots == null)
        {
            // Create a new list
            snapshots = Pool.Get<List<System.Guid>>();

            // Try to get existing list from player cache
            if (_snapshotGuids.TryGetValue(player.userID, out var existingList))
            {
                // Copy items from existing list
                snapshots.AddRange(existingList);

                // Free the existing list
                Pool.FreeUnmanaged(ref existingList);
            }
        }
        else
        {
            // We already have a snapshots list to use
            // Check if it's different from what's stored
            if (_snapshotGuids.TryGetValue(player.userID, out var existingList) && existingList != snapshots)
            {
                // Free the existing list since we're replacing it
                Pool.FreeUnmanaged(ref existingList);
            }
        }

        // Store our list (either the one that was passed in, or our new one)
        _snapshotGuids[player.userID] = snapshots;

        var container = RenderBasicLayout(cui, _snapshotMenuId, "Snapshot results", out var main, out var header);

        if (record != null)
        {
            // Back button
            var backButton = cui.v2
                .CreateButton(
                    container: header,
                    position: new(.88f, .15f, .962f, .85f),
                    offset: LuiOffset.None,
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

        if (snapshots.Count > 0)
        {
            if (_currentSelectedSnapshot.TryGetValue(player.userID, out var selectedSnapshot))
            {
                TryGetSelectedSnapshotHandle(player, out var handle);
                if (handle != null)
                {
                    CreateSnapshotsDetail(cui, player, rightPanel, handle);

                    CreateActionButtons(cui, player, main, handle);
                }
                else
                {
                    cui.v2.CreateText(
                        container: rightPanel,
                        position: LuiPosition.Full,
                        offset: LuiOffset.None,
                        color: "1 1 1 .5",
                        fontSize: 14,
                        text: "Failed to load snapshot data.",
                        alignment: TextAnchor.MiddleCenter
                    );
                }
            }
            else
            {
                cui.v2.CreateText(
                    container: rightPanel,
                    position: LuiPosition.Full,
                    offset: LuiOffset.None,
                    color: "1 1 1 .5",
                    fontSize: 14,
                    text: "No snapshot selected.",
                    alignment: TextAnchor.MiddleCenter
                );
            }
        }
        else
        {
            cui.v2.CreateText(
                container: rightPanel,
                position: LuiPosition.Full,
                offset: LuiOffset.None,
                color: "1 1 1 .5",
                fontSize: 14,
                text: "Could not find any snapshots.",
                alignment: TextAnchor.MiddleCenter
            );
        }

        return container;
    }

    private void CreateSnapshotsList(Components.CUI cui, Components.LUI.LuiContainer leftPanel, BasePlayer player, List<System.Guid> snapshotIds)
    {
        const int maxVisibleScrollItems = 12;

        // Snapshot list
        var snapshotList = cui.v2
            .CreatePanel(
                container: leftPanel,
                position: new(0, 0, 1, .95f),
                offset: new(5, 5, -5, -5),
                color: "0 0 0 0"
            );

        if (snapshotIds.Count == 0)
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

        // Get scroll position
        if (!_snapshotScrollIndex.TryGetValue(player.userID, out int scrollIndex))
        {
            _snapshotScrollIndex[player.userID] = 0;
            scrollIndex = 0;
        }

        var snapshots = Pool.Get<List<BuildSnapshotMetaData>>();
        snapshots.AddRange(_snapshotMetaData
            .Where(x => snapshotIds.Contains(x.Key))
            .Select(x => x.Value)
            .OrderByDescending(x => x.TimestampUTC));

        var visibleLowerItem = scrollIndex + 1;
        var visibleUpperItem = scrollIndex + maxVisibleScrollItems;

        // List snapshots
        float itemHeight = 0.08f;
        int visibleCount = Mathf.Min(snapshots.Count, maxVisibleScrollItems);

        // Clamp scroll index
        int maxScroll = Mathf.Max(0, snapshotIds.Count - maxVisibleScrollItems);
        scrollIndex = Mathf.Clamp(scrollIndex, 0, maxScroll);
        _snapshotScrollIndex[player.userID] = scrollIndex;

        Puts($"Max scroll: {maxScroll} | Scroll index: {scrollIndex}");

        // Get current selection
        Guid selectedId = Guid.Empty;
        _currentSelectedSnapshot.TryGetValue(player.userID, out selectedId);
        
        if (!snapshotIds.Contains(selectedId))
        {
            selectedId = snapshots[scrollIndex].ID;
            _currentSelectedSnapshot[player.userID] = selectedId;
        }

        for (int i = scrollIndex; i < scrollIndex + visibleCount && i < snapshots.Count; i++)
        {
            // Get metadata
            var meta = snapshots[i];
            float yMin = 1f - (i - scrollIndex + 1) * itemHeight;
            float yMax = 1f - (i - scrollIndex) * itemHeight;

            bool isSelected = meta.ID == selectedId;

            var displayText = $"{meta.TimestampUTC:yyyy-MM-dd HH:mm} ({meta.Entities} entities)";

            // Button to select snapshot
            var snapshotButton = cui.v2
                .CreateButton(
                    container: snapshotList,
                    position: new(0, yMin, 1, yMax),
                    offset: new(0, 2, 0, -2),
                    command: $"{nameof(AutoBuildSnapshot)}.{nameof(CommandSnapshotsSelect)} {meta.ID}",
                    color: isSelected ? "0.2 0.4 0.6 0.8" : (i % 2 == 0 ? "0.2 0.2 0.2 0.5" : "0.25 0.25 0.25 0.5")
                )
                .AddCursor();

            // Snapshot text
            cui.v2.CreateText(
                container: snapshotButton,
                    position: LuiPosition.Full,
                offset: new(10, 0, -10, 0),
                color: "1 1 1 .9",
                fontSize: 12,
                text: displayText,
                alignment: TextAnchor.MiddleLeft
            );
        }

        // Add scroll buttons if needed
        if (snapshotIds.Count > visibleCount)
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

    private void CreateSnapshotsDetail(
        Components.CUI cui,
        BasePlayer player, 
        Components.LUI.LuiContainer rightPanel, 
        SnapshotHandle handle
    )
    {
        const int maxVisibleAuthUsers = 6;

        // Snapshot details section
        var detailsHeader = cui.v2
            .CreatePanel(
                container: rightPanel,
                position: new(0, .9f, 1, 1),
                offset: new(0, 0, 0, 0),
                color: "0.2 0.2 0.3 1"
            );

        string extra = string.Empty;

        string title = "Snapshot Details";
        if (handle.PlayerUserID != player.userID)
        {
            title += $" (in use by {handle.Player.displayName} | {handle.State} {FormatRelativeTime(handle.TimeSinceModified)} ago)";
        }

        cui.v2.CreateText(
            container: detailsHeader,
            position: LuiPosition.Full,
            offset: new(10, 0, -10, 0),
            color: "1 1 1 1",
            fontSize: 14,
            text: title,
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
            text: $"Timestamp: {handle.Meta.TimestampUTC:yyyy-MM-dd HH:mm:ss} UTC",
            alignment: TextAnchor.MiddleLeft
        );

        // Entities count
        cui.v2.CreateText(
            container: detailsContent,
            position: new(0, .75f, 1, .85f),
            offset: new(5, 0, -5, 0),
            color: "1 1 1 .9",
            fontSize: 14,
            text: $"Total Entities: {handle.Meta.Entities}",
            alignment: TextAnchor.MiddleLeft
        );

        // Linked buildings count
        cui.v2.CreateText(
            container: detailsContent,
            position: new(0, .65f, 1, .75f),
            offset: new(5, 0, -5, 0),
            color: "1 1 1 .9",
            fontSize: 14,
            text: $"Linked Buildings: {handle.Meta.LinkedBuildings.Count}",
            alignment: TextAnchor.MiddleLeft
        );

        // Authorized users section
        cui.v2.CreateText(
            container: detailsContent,
            position: new(0, .55f, 1, .65f),
            offset: new(5, 0, -5, 0),
            color: "1 1 1 .9",
            fontSize: 14,
            text: $"Authorized Users: {handle.Meta.AuthorizedPlayers.Count}",
            alignment: TextAnchor.MiddleLeft
        );

        // Display authorized users
        int authCount = Mathf.Min(handle.Meta.AuthorizedPlayers.Count, maxVisibleAuthUsers);
        for (int i = 0; i < authCount; i++)
        {
            var user = handle.Meta.AuthorizedPlayers[i];
            cui.v2.CreateText(
                container: detailsContent,
                position: new(0.03f, .55f - ((i + 1) * 0.08f), 1, .55f - (i * 0.08f)),
                offset: new(0, 0, 0, 0),
                color: "0.9 0.9 0.9 .8",
                fontSize: 12,
                text: $"• {user.UserName} ({user.UserID})",
                alignment: TextAnchor.MiddleLeft
            );
        }

        // Show "more users" if there are more than we displayed
        if (handle.Meta.AuthorizedPlayers.Count > authCount)
        {
            int remainingUsers = handle.Meta.AuthorizedPlayers.Count - authCount;
            cui.v2.CreateText(
                container: detailsContent,
                position: new(0.03f, .55f - ((authCount + 1) * 0.08f), 1, .55f - (authCount * 0.08f)),
                offset: new(0, 0, 0, 0),
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
                position: new(0, .01f, 1, .3f),
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
        int buildingCount = Mathf.Min(handle.Meta.LinkedBuildings.Count, 3); // Show up to 3 buildings
        int buildingIndex = 0;

        foreach (var building in handle.Meta.LinkedBuildings)
        {
            if (buildingIndex >= buildingCount) break;

            // Building container
            var buildingItem = cui.v2
                .CreatePanel(
                    container: linkedBuildingsPanel,
                    position: new(0, .85f - ((buildingIndex + 1) * 0.3f), 1, .85f - (buildingIndex * 0.3f)),
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
            var pos = building.Value.Position;
            var teleportButton = cui.v2
                .CreateButton(
                    container: buildingItem,
                    position: new(.7f, .2f, .95f, .8f),
                    offset: LuiOffset.None,
                    command: $"{nameof(AutoBuildSnapshot)}.{nameof(CommandGlobalTeleport)} {pos.x} {pos.y} {pos.z}",
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
        if (handle.Meta.LinkedBuildings.Count > buildingCount)
        {
            int remainingBuildings = handle.Meta.LinkedBuildings.Count - buildingCount;
            cui.v2.CreateText(
                container: linkedBuildingsPanel,
                position: new(0, .85f - ((buildingCount + 1) * 0.3f), 1, .85f - (buildingCount * 0.3f)),
                offset: new(5, 0, -5, 0),
                color: "0.7 0.7 0.7 .7",
                fontSize: 12,
                text: $"And {remainingBuildings} more building{(remainingBuildings > 1 ? "s" : "")}...",
                alignment: TextAnchor.MiddleLeft
            );
        }
    }


    private void CreateActionButtons(
        Components.CUI cui,
        BasePlayer player,
        Components.LUI.LuiContainer main,
        SnapshotHandle handle
    )
    {
        // Bottom buttons panel
        var buttonPanel = cui.v2
            .CreatePanel(
                container: main,
                position: new(0, 0, 1, .08f),
                offset: new(10, 5, -10, -5),
                color: "0.2 0.2 0.2 1"
            );

        CreateActionButton(cui, player, handle, "Rollback", 0,
            buttonPanel,
            nameof(CommandSnapshotsRollback),
            _config.Commands.Rollback,
            SnapshotState.ProcessRollback,
            "0.6 0.3 0.3 1"
        );

        /*
        CreateActionButton(cui, player, handle, "Preview Zones", 1,
            buttonPanel,
            nameof(CommandSnapshotsPreviewZones),
            _config.Commands.PreviewZones,
            SnapshotState.PreviewZones,
            "0.408 0.435 0.706 1.0"
        );

        CreateActionButton(cui, player, handle, "Preview Rollback", 2,
            buttonPanel,
            nameof(CommandSnapshotsPreviewRollback),
            _config.Commands.PreviewRollback,
            SnapshotState.PreviewRollback,
            "0.275 0.514 0.651 1.0"
        );
        */
    }

    private void CreateActionButton(
        Components.CUI cui,
        BasePlayer player,
        SnapshotHandle handle,
        string title,
        int index,
        Components.LUI.LuiContainer buttonPanel,
        string commandName,
        AutoBuildSnapshotConfig.CommandSetting commandSetting,
        SnapshotState state,
        string color
    )
    {
        float offX = -.2f * index;
        LuiPosition position = new(.8f + offX + .005f, .2f, 1 + offX - .005f, .8f);

        LUI.LuiContainer button;
        if (handle.PlayerUserID == player.userID
            && UserHasPermission(player, commandSetting)
            && !handle.State.HasFlag(state))
        {
            button = cui.v2
                .CreateButton(
                    container: buttonPanel,
                    position: position,
                    offset: LuiOffset.None,
                    command: $"{nameof(AutoBuildSnapshot)}.{commandName} {handle.Meta.ID}",
                    color: color
                );
        }
        else
        {
            button = cui.v2
                .CreatePanel(
                    container: buttonPanel,
                    position: position,
                    offset: LuiOffset.None,
                    color: ".5 .5 .5 1"
                );
        }

        cui.v2.CreateText(
            container: button,
            position: LuiPosition.Full,
            offset: LuiOffset.None,
            color: "1 1 1 1",
            fontSize: 14,
            text: title,
            alignment: TextAnchor.MiddleCenter
        );
    }
}
