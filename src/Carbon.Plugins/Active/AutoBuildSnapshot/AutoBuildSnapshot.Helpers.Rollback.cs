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

            Pool.FreeUnmanaged(ref snapshots);
            SnapshotHandle.Release(player);
        });

    }

    private bool TryExecuteRollback(BasePlayer player, SnapshotData data)
    {
        // Check if the player has permission - note there's a logic error here
        // it should be: if(!UserHasPermission(player, _config.Commands.Rollback)) return false;
        // Currently it returns false if they DO have permission
        if (!UserHasPermission(player, _config.Commands.Rollback))
        {
            AddLogMessage(player, "You don't have permission to rollback.");
            return false;
        }

        AddLogMessage(player, "Begin rolling back base...");

        // Flatten entities into a lookup dictionary for easier access
        var entitiesLookup = Pool.Get<Dictionary<string, PersistantEntity>>();
        foreach (var entity in data.Entities.Values.SelectMany(_ => _))
        {
            entitiesLookup[entity.ID] = entity;
        }

        int entityCount = entitiesLookup.Count;
        int processed = 0;
        int created = 0;
        int removed = 0;
        int updated = 0;
        int skipped = 0;

        // Phase 1: Handle existing entities - destroy conflicting and update existing
        using var entitiesToDestroy = Pool.Get<PooledList<BaseEntity>>();

        foreach (var entityKvp in entitiesLookup)
        {
            var persistentEntity = entityKvp.Value;
            bool entityWasProcessed = false;

            // Check if this entity already exists by position/type
            if (EntityExists(persistentEntity, out var collisions))
            {
                foreach (var existingEntity in collisions)
                {
                    // If it's the same type, we might update it
                    if (existingEntity.GetType().Name == persistentEntity.Type)
                    {
                        if (TryUpdateEntity(existingEntity, persistentEntity))
                        {
                            updated++;
                            entityWasProcessed = true;
                        }
                    }
                    else
                    {
                        // Add to destroy list
                        entitiesToDestroy.Add(existingEntity);
                        removed++;
                    }
                }
            }

            if (!entityWasProcessed)
            {
                // We'll create this entity in Phase 2
            }

            processed++;

            // Log progress every 10% of entities
            if (processed % Math.Max(1, entityCount / 10) == 0)
            {
                var progressPercent = (int)(((float)processed / entityCount) * 100);
                AddLogMessage(player, $"Rollback progress: {progressPercent}% ({processed}/{entityCount})");
            }
        }

        // Destroy conflicting entities
        foreach (var entity in entitiesToDestroy)
        {
            if (entity != null && !entity.IsDestroyed)
            {
                entity.Kill();
            }
        }

        // Phase 2: Create new entities
        foreach (var entityKvp in entitiesLookup)
        {
            var persistentEntity = entityKvp.Value;

            // Skip if entity is already processed (updated)
            if (EntityExists(persistentEntity, out _))
            {
                continue;
            }

            if (TryCreateEntity(persistentEntity))
            {
                created++;
            }
            else
            {
                skipped++;
                AddLogMessage($"Failed to create entity: {persistentEntity.ID}");
            }
        }

        // Free resources
        Pool.FreeUnmanaged(ref entitiesLookup);

        AddLogMessage(player, $"Rollback complete! Created: {created}, Updated: {updated}, Removed: {removed}, Skipped: {skipped}");

        return true;
    }

    /// <summary>
    /// Scans the entity location for any collisions and returns true if it was found in the list.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <param name="collisions">The list of collisions found.</param>
    private bool EntityExists(PersistantEntity entity, out List<BaseEntity> collisions)
    {
        // Create sphere around entity and raycast baselayer mask in 3m radius (or adjust as needed)
        const float scanRadius = 3f;
        collisions = Pool.Get<List<BaseEntity>>();

        // Use a sphere check to find nearby entities
        Vis.Entities(entity.Position, scanRadius, collisions, _maskBaseEntities);

        // Filter out entities that don't match the proximity check
        for (int i = collisions.Count - 1; i >= 0; i--)
        {
            BaseEntity existing = collisions[i];

            // Remove if too far from expected position
            if (Vector3.Distance(existing.ServerPosition, entity.Position) > scanRadius)
            {
                collisions.RemoveAt(i);
                continue;
            }

            // Consider it an exact match if type and position are very close
            if (existing.GetType().Name == entity.Type &&
                Vector3.Distance(existing.ServerPosition, entity.Position) < 0.1f)
            {
                // Keep only this entity and return true, we found an exact match
                var exactMatch = existing;
                collisions.Clear();
                collisions.Add(exactMatch);
                return true;
            }
        }

        // Return true if we found any potential collisions
        return collisions.Count > 0;
    }

    /// <summary>
    /// Attempts to update an existing entity with data from the snapshot
    /// </summary>
    /// <param name="existingEntity">The existing entity to update.</param>
    /// <param name="snapshotEntity">The snapshot entity data.</param>
    /// <returns>True if the entity was updated, false otherwise.</returns>
    private bool TryUpdateEntity(BaseEntity existingEntity, PersistantEntity snapshotEntity)
    {
        try
        {
            // Update common properties if needed
            if (existingEntity.OwnerID != snapshotEntity.OwnerID)
            {
                existingEntity.OwnerID = snapshotEntity.OwnerID;
            }

            // Handle building blocks
            if (existingEntity is BuildingBlock block &&
                snapshotEntity.Properties.TryGetValue("Grade", out var gradeObj) &&
                gradeObj is int grade)
            {
                BuildingGrade.Enum blockGrade = (BuildingGrade.Enum)grade;
                if (block.grade != blockGrade)
                {
                    block.SetGrade(blockGrade);
                    block.ChangeGrade(blockGrade, true);
                }
            }

            // Handle decay entities
            if (existingEntity is DecayEntity decay &&
                snapshotEntity.Properties.TryGetValue("Health", out var healthObj) &&
                healthObj is float health)
            {
                decay.health = health;
            }

            // Handle storage containers
            if (existingEntity is StorageContainer storage &&
                snapshotEntity.Properties.TryGetValue("Items", out var itemsObj) &&
                itemsObj is PersistantItem[] items)
            {
                // Clear existing inventory
                storage.inventory.Clear();

                // Add items from snapshot
                foreach (var item in items)
                {
                    RestoreItem(storage.inventory, item);
                }
            }

            // Update skin if present
            if (snapshotEntity.Properties.TryGetValue("SkinID", out var skinObj) &&
                skinObj is ulong skinID && skinID > 0)
            {
                existingEntity.skinID = skinID;
            }

            // Send network update
            existingEntity.SendNetworkUpdate();

            return true;
        }
        catch (Exception ex)
        {
            AddLogMessage($"Error updating entity {existingEntity.net.ID}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Attempts to create a new entity from the snapshot data
    /// </summary>
    /// <param name="snapshotEntity">The snapshot entity data.</param>
    /// <returns>True if the entity was created successfully.</returns>
    private bool TryCreateEntity(PersistantEntity snapshotEntity)
    {
        try
        {
            // Create entity based on prefab ID
            var entity = GameManager.server.CreateEntity(snapshotEntity.PrefabName,
                snapshotEntity.Position, snapshotEntity.Rotation);

            if (entity == null)
            {
                AddLogMessage($"Failed to create entity with prefab: {snapshotEntity.PrefabName}");
                return false;
            }

            // Set owner
            entity.OwnerID = snapshotEntity.OwnerID;

            // Set properties based on type
            if (entity is BuildingBlock block &&
                snapshotEntity.Properties.TryGetValue("Grade", out var gradeObj) &&
                gradeObj is int grade)
            {
                block.grade = (BuildingGrade.Enum)grade;
            }

            // Handle decay entities
            if (entity is DecayEntity decay &&
                snapshotEntity.Properties.TryGetValue("Health", out var healthObj) &&
                healthObj is float health)
            {
                decay.health = health;
            }

            // Handle storage containers
            if (entity is StorageContainer storage &&
                snapshotEntity.Properties.TryGetValue("Items", out var itemsObj) &&
                itemsObj is PersistantItem[] items)
            {
                foreach (var item in items)
                {
                    RestoreItem(storage.inventory, item);
                }
            }

            // Set skin if present
            if (snapshotEntity.Properties.TryGetValue("SkinID", out var skinObj) &&
                skinObj is ulong skinID && skinID > 0)
            {
                entity.skinID = skinID;
            }

            // Spawn the entity
            entity.Spawn();

            return true;
        }
        catch (Exception ex)
        {
            AddLogMessage($"Error creating entity {snapshotEntity.ID}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Restores an item to the specified container
    /// </summary>
    /// <param name="container">The container to add the item to.</param>
    /// <param name="persistentItem">The item data.</param>
    private void RestoreItem(ItemContainer container, PersistantItem persistentItem)
    {
        if (container == null) return;

        // Create the item
        var item = ItemManager.CreateByItemID(persistentItem.ItemID, persistentItem.Amount);
        if (item == null) return;

        // Set properties
        // item.uid = persistentItem.UID; // readonly
        item.flags = persistentItem.Flags;

        // Set skin if present
        if (persistentItem.Properties.TryGetValue("SkinID", out var skinObj) &&
            skinObj is ulong skinID && skinID > 0)
        {
            item.skin = skinID;
        }

        // Set fuel if present
        if (persistentItem.Properties.TryGetValue("Fuel", out var fuelObj) &&
            fuelObj is float fuel && fuel > 0)
        {
            item.fuel = fuel;
        }

        // Add to container
        if (!item.MoveToContainer(container))
        {
            // Failed to add to container, destroy item
            item.Remove();
            return;
        }

        // Process child items if present
        if (persistentItem.Properties.TryGetValue("Items", out var itemsObj) &&
            itemsObj is PersistantItem[] childItems)
        {
            // Ensure we have a container
            if (item.contents == null)
            {
                item.contents = new ItemContainer();
                item.contents.ServerInitialize(null, childItems.Length);
                item.contents.GiveUID();
                item.contents.parent = item;
            }

            // Add child items
            foreach (var childItem in childItems)
            {
                RestoreItem(item.contents, childItem);
            }
        }
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
