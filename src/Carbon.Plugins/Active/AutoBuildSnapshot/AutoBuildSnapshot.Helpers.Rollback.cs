using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Facepunch;
using Cysharp.Threading.Tasks;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Executes the rollback operation for the specified snapshot.
    /// </summary>
    /// <param name="handle">The snapshot handle to rollback.</param>
    private async UniTaskVoid BeginRollbackTask(SnapshotHandle handle)
    {
        var player = handle.Player;
        var snapshotId = handle.ID;

        AddLogMessage($"Player {player.displayName} initiated rollback to snapshot {snapshotId}");
        player.ChatMessage($"Rollback to snapshot {snapshotId} initiated...");

        // Get the snapshot data
        var snapshotData = await handle.Meta.GetDataAsync();

        // Get any records that collide with this snapshot's zones
        var records = GetCollidingRecords(snapshotData.Zones);

        if (records.Count > 0)
        {
            AddLogMessage(player, $"Found {records.Count} record(s) that collide, creating backup...");

            // Perform backup
            ProcessNextSave(records, (success, snapshots) =>
            {
                if (success)
                {
                    AddLogMessage(player, $"Backup completed in {snapshots.Sum(x => x.Duration)} ms");
                    ExecuteRollbackAsync(player, snapshotData).Forget();
                }
                else
                {
                    AddLogMessage(player, "Failed to create backup, rollback aborted.");
                    SnapshotHandle.Release(player);
                }
            });
        }
        else
        {
            AddLogMessage(player, "No colliding records found, skipping backup.");
            ExecuteRollbackAsync(player, snapshotData).Forget();
        }
    }

    /// <summary>
    /// Attempts to execute the rollback operation.
    /// </summary>
    /// <param name="player">The player initiating the rollback.</param>
    /// <param name="data">The snapshot data to rollback to.</param>
    private async UniTaskVoid ExecuteRollbackAsync(BasePlayer player, SnapshotData data)
    {
        if (await TryExecuteRollbackAsync(player, data))
        {
            AddLogMessage(player, $"Rollback to snapshot {data.ID} completed.");
        }
        else
        {
            AddLogMessage(player, "The rollback process was aborted.");
        }

        SnapshotHandle.Release(player);
    }

    /// <summary>
    /// Attempts to execute the rollback operation.
    /// </summary>
    /// <param name="player">The player initiating the rollback.</param>
    /// <param name="data">The snapshot data to rollback to.</param>
    /// <returns>True if the rollback was successful; otherwise, false.</returns>
    private async UniTask<bool> TryExecuteRollbackAsync(BasePlayer player, SnapshotData data)
    {
        AddLogMessage(player, "Begin rolling back base...");

        using var entitiesToKill = Pool.Get<PooledList<BaseEntity>>();
        using var entitiesToCreate = Pool.Get<PooledList<PersistantEntity>>();
        using var outEntitiesToUpdate = Pool.Get<PooledList<BaseEntity>>();
        if (!await TryPrepareEntitiesAsync(data.Entities.Values.SelectMany(_ => _), entitiesToKill, entitiesToCreate, outEntitiesToUpdate))
        {
            AddLogMessage(player, "Failed to find target entities to cleanup.");
            return false;
        }

        AddLogMessage(player, $"Found {entitiesToKill.Count} entities to destroy, {entitiesToCreate.Count} entities to create, and {outEntitiesToUpdate.Count} entities to update.");

        // Destroy all entitiesToKill
        foreach (var entity in entitiesToKill)
        {
            var id = GetPersistanceID(entity);
            AddLogMessage($" Destroying entity: {entity.GetType()} ({entity.PrefabName} | ID: {id})");

            // entity.Kill();
        }

        // Spawn all entitiesToSpawn, then add them to entitesToUpdate
        foreach (var entity in entitiesToCreate)
        {
            AddLogMessage($" Spawning entity: {entity.Type} ({entity.PrefabName} | ID: {entity.ID})");

            // GameManager.server.CreateEntity(entity.PrefabName, entity.Position, entity.Rotation);
        }

        // Update all entitiesToUpdate
        foreach (var entity in outEntitiesToUpdate)
        {
            var id = GetPersistanceID(entity);
            AddLogMessage($" Updating entity: {entity.GetType()} ({entity.PrefabName} | ID: {id})");
        }

        AddLogMessage(player, "Rollback complete.");

        return true;
    }

    /// <summary>
    /// Prepares the entities for rollback by determining which entities to kill, create, and update.
    /// </summary>
    /// <param name="input">The input entities to prepare.</param>
    /// <param name="outEntitiesToKill">The list to populate of entities to kill.</param>
    /// <param name="outEntitiesToCreate">The list to populate of entities to create.</param>
    /// <param name="outEntitiesToUpdate">The list to populate of entities to update.</param>
    /// <returns>True if the preparation was successful; otherwise, false.</returns>
    private async UniTask<bool> TryPrepareEntitiesAsync(
        IEnumerable<PersistantEntity> input,
        List<BaseEntity> outEntitiesToKill,
        List<PersistantEntity> outEntitiesToCreate,
        List<BaseEntity> outEntitiesToUpdate)
    {
        var trackingIds = Pool.Get<HashSet<string>>();
        foreach (var entity in input)
        {
            trackingIds.Add(entity.ID);
        }

        foreach (var entity in input)
        {
            if (!TryFindEntity(entity, trackingIds, outEntitiesToKill, out var currentEntity))
            {
                return false;
            }

            if (currentEntity == null)
            {
                outEntitiesToCreate.Add(entity);
            }
            else
            {
                outEntitiesToUpdate.Add(currentEntity);
            }
        }

        Pool.FreeUnmanaged(ref trackingIds);

        return true;
    }

    private bool TryFindEntity(PersistantEntity entity, HashSet<string> trackingIds, List<BaseEntity> outEntitiesToKill, out BaseEntity currentEntity)
    {
        const float checkRadius = 6;
        using var foundEntities = Pool.Get<PooledList<BaseEntity>>();
        Vis.Entities(entity.Position, checkRadius, foundEntities, _maskBaseEntities);

        currentEntity = null;
        foreach (var found in foundEntities)
        {
            var foundID = GetPersistanceID(found);
            if (entity.ID == foundID)
            {
                currentEntity = found;
            }
            else if (!trackingIds.Contains(foundID))
            {
                outEntitiesToKill.Add(found);
            }
        }

        return true;
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
