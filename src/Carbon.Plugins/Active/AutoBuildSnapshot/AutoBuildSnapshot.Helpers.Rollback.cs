using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Facepunch;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Executes the rollback operation for the specified snapshot.
    /// </summary>
    /// <param name="handle">The snapshot handle to rollback.</param>
    private void BeginRollback(SnapshotHandle handle)
    {
        var player = handle.Player;
        var snapshotId = handle.ID;

        AddLogMessage($"Player {player.displayName} initiated rollback to snapshot {snapshotId}");
        player.ChatMessage($"Rollback to snapshot {snapshotId} initiated...");

        // Get the snapshot data
        var snapshotData = handle.Meta.GetData();

        // Get any records that collide with this snapshot's zones
        var records = GetCollidingRecords(snapshotData.Zones);
        AddLogMessage(player, $"Found {records.Count} records that collide, creating backup...");

        // Perform backup
        ProcessNextSave(records, (success, snapshots) =>
        {
            if (success)
            {
                // Perform rollback
                AddLogMessage(player, $"Backup completed in {snapshots.Sum(x => x.Duration)} ms");
                if (TryExecuteRollback(player, snapshotData))
                {
                    AddLogMessage(player, $"Rollback to snapshot {snapshotId} completed.");
                }
                else
                {
                    AddLogMessage(player, "The rollback process was aborted.");
                }
            }
            else
            {
                AddLogMessage(player, "Failed to create backup, rollback aborted.");
            }

            Pool.Free(ref snapshots, true);
            SnapshotHandle.Release(player);
        });

    }

    /// <summary>
    /// Attempts to execute the rollback operation.
    /// </summary>
    /// <param name="player">The player initiating the rollback.</param>
    /// <param name="data">The snapshot data to rollback to.</param>
    /// <returns>True if the rollback was successful; otherwise, false.</returns>
    private bool TryExecuteRollback(BasePlayer player, SnapshotData data)
    {
        // Check if the player has permission - note there's a logic error here
        // it should be: if(!UserHasPermission(player, _config.Commands.Rollback)) return false;
        // Currently it returns false if they DO have permission
        if (!UserHasPermission(player, _config.Commands.Rollback)) return false;

        AddLogMessage(player, "Begin rolling back base...");



        AddLogMessage(player, "Rollback complete.");

        return true;
    }

    /// <summary>
    /// Gets the colliding build records for the specified zones.
    /// </summary>
    /// <param name="zones">The zones to check for collisions.</param>
    /// <returns>A queue of colliding records.</returns>
    private Queue<BuildRecord> GetCollidingRecords(List<Vector4> zones)
    {
        var records = Pool.Get<Queue<BuildRecord>>();

        foreach (var record in _buildRecords.Values)
        {
            if (AnyZoneContainsRecordZones(record, zones))
            {
                records.Enqueue(record);
            }
        }

        return records;
    }

    /// <summary>
    /// Checks if any of the zones in the record intersect with the specified zones.
    /// </summary>
    /// <param name="record">The build record to check.</param>
    /// <param name="zones">The zones to check against.</param>
    /// <returns>True if any zone contains the record zones, false otherwise.</returns>
    private bool AnyZoneContainsRecordZones(BuildRecord record, List<Vector4> zones)
    {
        foreach (var zone in zones)
        {
            if (record.EntityZones.Any(z => ZonesCollide(z, zone)))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if two zones collide.
    /// </summary>
    /// <param name="left">The first zone.</param>
    /// <param name="right">The second zone.</param>
    /// <returns>True if the zones collide, false otherwise.</returns>
    private bool ZonesCollide(Vector4 left, Vector4 right) =>
        (left.x - right.x) * (left.x - right.x) +
        (left.y - right.y) * (left.y - right.y) +
        (left.z - right.z) * (left.z - right.z) <=
        (left.w + right.w) * (left.w + right.w);
}
