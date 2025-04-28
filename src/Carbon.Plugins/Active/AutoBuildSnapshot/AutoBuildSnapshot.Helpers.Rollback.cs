using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Facepunch;
using Cysharp.Threading.Tasks;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private class BuildRollback : Pool.IPooled
    {
        private SnapshotHandle _handle;

        public void EnterPool()
        {
            Pool.Free(ref _handle);
        }

        public void LeavePool()
        {
        }

        public static BuildRollback Create(SnapshotHandle handle)
        {
            var rollback = Pool.Get<BuildRollback>();
            rollback.Init(handle);
            return rollback;
        }

        private void Init(SnapshotHandle handle)
        {
            _handle = handle;
        }

        /// <summary>
        /// Executes the rollback operation.
        /// </summary>
        public async UniTaskVoid BeginRollbackTask()
        {
            var player = _handle.Player;
            var snapshotId = _handle.ID;

            _instance.AddLogMessage($"Player {player.displayName} initiated rollback to snapshot {snapshotId}");
            player.ChatMessage($"Rollback to snapshot {snapshotId} initiated...");

            // Get the snapshot data
            var snapshotData = await _handle.Meta.GetDataAsync();

            // Get any records that collide with this snapshot's zones
            var records = _instance.GetCollidingRecords(snapshotData.Zones);

            if (records.Count > 0)
            {
                _instance.AddLogMessage(player, $"Found {records.Count} record(s) that collide, creating backup...");

                // Perform backup
                _instance.ProcessNextSave(records, (success, snapshots) =>
                {
                    if (success)
                    {
                        _instance.AddLogMessage(player, $"Backup completed in {snapshots.Sum(x => x.Duration)} ms");
                        ExecuteRollbackAsync(player, snapshotData).Forget();
                    }
                    else
                    {
                        _instance.AddLogMessage(player, "Failed to create backup, rollback aborted.");
                        SnapshotHandle.Release(player, ref _handle);
                    }
                });
            }
            else
            {
                _instance.AddLogMessage(player, "No colliding records found, skipping backup.");
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
                _instance.AddLogMessage(player, $"Rollback to snapshot {data.ID} completed.");
            }
            else
            {
                _instance.AddLogMessage(player, "The rollback process was aborted.");
            }

            SnapshotHandle.Release(player, ref _handle);
        }

        /// <summary>
        /// Attempts to execute the rollback operation.
        /// </summary>
        /// <param name="player">The player initiating the rollback.</param>
        /// <param name="data">The snapshot data to rollback to.</param>
        /// <returns>True if the rollback was successful; otherwise, false.</returns>
        private async UniTask<bool> TryExecuteRollbackAsync(BasePlayer player, SnapshotData data)
        {
            _instance.AddLogMessage(player, "Begin rolling back base...");

            using var entitiesToKill = Pool.Get<PooledHashSet<BaseEntity>>();
            using var entitiesToCreate = Pool.Get<PooledHashSet<PersistantEntity>>();
            using var outEntitiesToUpdate = Pool.Get<PooledHashSet<BaseEntity>>();
            if (!await TryPrepareEntitiesAsync(data.Entities.Values.SelectMany(_ => _), entitiesToKill, entitiesToCreate, outEntitiesToUpdate))
            {
                _instance.AddLogMessage(player, "Failed to find target entities to cleanup.");
                return false;
            }

            _instance.AddLogMessage(player, $"Found {entitiesToKill.Count} entities to destroy, {entitiesToCreate.Count} entities to create, and {outEntitiesToUpdate.Count} entities to update.");

            // Destroy all entitiesToKill
            foreach (var entity in entitiesToKill)
            {
                var id = GetPersistanceID(entity);
                _instance.AddLogMessage($" Destroying entity: {entity.GetType()} ({entity.PrefabName} | ID: {id})");

                // entity.Kill();
            }

            // Spawn all entitiesToSpawn, then add them to entitesToUpdate
            foreach (var entity in entitiesToCreate)
            {
                _instance.AddLogMessage($" Spawning entity: {entity.Type} ({entity.PrefabName} | ID: {entity.ID})");

                // GameManager.server.CreateEntity(entity.PrefabName, entity.Position, entity.Rotation);
            }

            // Update all entitiesToUpdate
            foreach (var entity in outEntitiesToUpdate)
            {
                var id = GetPersistanceID(entity);
                _instance.AddLogMessage($" Updating entity: {entity.GetType()} ({entity.PrefabName} | ID: {id})");
            }

            _instance.AddLogMessage(player, "Rollback complete.");

            return true;
        }

        /// <summary>
        /// Prepares the entities for rollback by determining which entities to kill, create, and update.
        /// </summary>
        /// <param name="rollbackEntities">The input entities to prepare.</param>
        /// <param name="outEntitiesToKill">The list to populate of entities to kill.</param>
        /// <param name="outEntitiesToCreate">The list to populate of entities to create.</param>
        /// <param name="outEntitiesToUpdate">The list to populate of entities to update.</param>
        /// <returns>True if the preparation was successful; otherwise, false.</returns>
        private async UniTask<bool> TryPrepareEntitiesAsync(
            IEnumerable<PersistantEntity> rollbackEntities,
            PooledHashSet<BaseEntity> outEntitiesToKill,
            PooledHashSet<PersistantEntity> outEntitiesToCreate,
            PooledHashSet<BaseEntity> outEntitiesToUpdate)
        {
            using var trackingIds = Pool.Get<PooledHashSet<string>>();
            foreach (var entity in rollbackEntities)
            {
                trackingIds.Add(entity.ID);
            }

            foreach (var entity in rollbackEntities)
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

            return true;
        }

        private bool TryFindEntity(PersistantEntity entity, HashSet<string> trackingIds, HashSet<BaseEntity> outEntitiesToKill, out BaseEntity currentEntity)
        {
            using var foundEntities = Pool.Get<PooledList<BaseEntity>>();
            Vis.Entities(entity.CenterPosition, entity.CollisionRadius, foundEntities, _maskBaseEntities);

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
    }
}
