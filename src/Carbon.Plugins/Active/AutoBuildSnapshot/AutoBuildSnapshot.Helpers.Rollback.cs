using System.Collections.Generic;
using System.Linq;
using Facepunch;
using Cysharp.Threading.Tasks;
using System;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private class BuildRollback : ExecutionBase
    {
        private SnapshotHandle _handle;

        public override void EnterPool()
        {
            base.EnterPool();

            Pool.Free(ref _handle);
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
        protected override async UniTask ProcessAsync()
        {
            var player = _handle.Player;
            var snapshotId = _handle.ID;

            _instance.AddLogMessage($"Player {player.displayName} initiated rollback to snapshot {snapshotId}");
            player.ChatMessage($"Rollback to snapshot {snapshotId} initiated...");

            // Get the snapshot data
            var snapshotData = await _handle.Meta.GetDataAsync();

            // Get any records that collide with this snapshot's zones
            var records = await _instance.GetCollidingRecordsAsync(snapshotData.Zones);

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
                _instance.AddLogMessage($"Destroying entity: {GetPersistanceID(entity)}");
                entity.Kill();

                await YieldStep();
            }

            // Spawn all entitiesToSpawn, then add them to entitesToUpdate
            foreach (var create in entitiesToCreate)
            {
                var entity = GameManager.server.CreateEntity(create.PrefabName, create.Position, create.Rotation);
                if (TryUpdateEntity(entity, create))
                {
                    _instance.AddLogMessage($"Creating entity: {create.ID})");
                    entity.Spawn();
                }
                else
                {
                    _instance.AddLogMessage($"Failed to create entity: {create.ID}");
                    entity.Kill();
                }
            }

            var entityLookup = Pool.Get<Dictionary<string, PersistantEntity>>();
            foreach (var entity in data.Entities.Values.SelectMany(_ => _))
            {
                entityLookup[entity.ID] = entity;
            }

            // Update all entitiesToUpdate
            foreach (var entity in outEntitiesToUpdate)
            {
                var entityID = GetPersistanceID(entity);
                if (entityLookup.TryGetValue(entityID, out var update))
                {
                    if (TryUpdateEntity(entity, update))
                    {
                        _instance.AddLogMessage($"Updating entity: {entityID}");
                    }
                    else
                    {
                        _instance.AddLogMessage($"Failed to update entity: {entityID}");
                    }
                }
                
            }

            Pool.FreeUnmanaged(ref entityLookup);

            _instance.AddLogMessage(player, "Rollback complete.");

            return true;
        }

        private bool TryUpdateEntity(BaseEntity entity, PersistantEntity data)
        {
            try
            {
                data.CopyTo(entity);
            }
            catch (Exception ex)
            {
                _instance.AddLogMessage($"Exception while trying to update: {ex}");
                return false;
            }

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

                await YieldStep();
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
