using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Facepunch;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Generates an id for the entity based on prefab and location that should persist between server restarts.
    /// </summary>
    /// <param name="entity">The entity to generate the id for.</param>
    /// <returns>The generated id.</returns>
    public static string GetPersistanceID<T>(T entity)
        where T : BaseEntity =>
        GetPersistanceID(entity.GetType(), entity.ServerPosition);

    /// <summary>
    /// Generates an id for the entity based on prefab and location that should persist between server restarts.
    /// </summary>
    /// <param name="type">The type of the entity.</param>
    /// <param name="position">The position of the entity.</param>
    /// <returns>The generated id.</returns>
    public static string GetPersistanceID(Type type, Vector3 position) =>
        GetPersistanceID(type.Name, position);

    /// <summary>
    /// Generates an id for the entity based on prefab and location that should persist between server restarts.
    /// </summary>
    /// <param name="typeName">The type name of the entity.</param>
    /// <param name="position">The position of the entity.</param>
    /// <returns>The generated id.</returns>
    public static string GetPersistanceID(string typeName, Vector3 position) =>
        GetPersistanceID(typeName, position.x, position.y, position.z);

    /// <summary>
    /// Generates an id for the entity based on prefab and location that should persist between server restarts.
    /// </summary>
    /// <param name="typeName">The type name of the entity.</param>
    /// <param name="x">The x coordinate of the entity.</param>
    /// <param name="y">The y coordinate of the entity.</param>
    /// <param name="z">The z coordinate of the entity.</param>
    /// <returns>The generated id.</returns>
    public static string GetPersistanceID(string typeName, float x, float y, float z) =>
        $"{typeName}({x:F2},{y:F2},{z:F2})";

    /// <summary>
    /// Checks if the entity has a valid id and isn't destroyed.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    private static bool ValidEntity(BaseNetworkable entity) =>
        entity != null && entity.net != null && entity.net.ID.IsValid && !entity.IsDestroyed;

    /// <summary>
    /// Creates a temporary sphere entity for the player at the specified position.
    /// </summary>
    /// <param name="player">The player to create the entity for.</param>
    /// <param name="position">The position of the entity.</param>
    /// <param name="density">The density of the sphere.</param>
    private static void CreateTempSphere(
        BasePlayer player,
        Vector3 position,
        float width,
        int density = 1)
    {
        if (!_instance.UserHasPermission(player, _config.Commands.AdminPermission)) return;

        for (int i = 0; i < density; i++)
        {
            CreateTempEntity(player, prefabSphere, position, Quaternion.identity, entity =>
            {
                var sphere = entity.GetComponent<SphereEntity>();
                sphere.lerpRadius = width;
                sphere.lerpSpeed = 100;
            });
        }
    }

    /// <summary>
    /// Creates a temporary, client-side entity for the player.
    /// </summary>
    /// <param name="player">The player to create the entity for.</param>
    /// <param name="prefabName">The prefab name of the entity.</param>
    /// <param name="position">The position of the entity.</param>
    /// <param name="entityBuilder">An optional action to modify the entity.</param>
    private static void CreateTempEntity(
        BasePlayer player, 
        string prefabName, 
        Vector3 position, 
        Quaternion rotation = default,
        Action<BaseEntity> entityBuilder = null)
    {
        if (!_instance.UserHasPermission(player, _config.Commands.AdminPermission)) return;

        var entity = GameManager.server.CreateEntity(prefabName, position);
        entityBuilder?.Invoke(entity);

        entity.Spawn();

        if (!_tempEntities.TryGetValue(player.userID, out var playerEntities))
        {
            playerEntities = Pool.Get<List<BaseEntity>>();
            _tempEntities[player.userID] = playerEntities;
        }

        playerEntities.Add(entity);
    }

    /// <summary>
    /// Kills all temporary client entities.
    /// </summary>
    /// <param name="playerID">Optionally, the player ID to kill entities for. Kills all temps if not specified.</param>
    private static void KillTempEntities(ulong playerID = 0)
    {
        if (_tempEntities == null || _tempEntities.Values.Count == 0)
            return;

        if (playerID == 0)
        {
            foreach (var entities in _tempEntities.Values)
            {
                KillEntities(entities);
            }
        }
        else if (_tempEntities.TryGetValue(playerID, out var entities))
        {
            KillEntities(entities);
        }
    }

    /// <summary>
    /// Kills the specified entities.
    /// </summary>
    /// <param name="entities">The entities to kill.</param>
    private static void KillEntities(List<BaseEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity?.Kill();
            entity?.SendNetworkUpdate();
        }

        entities.Clear();
    }

    /// <summary>
    /// Checks if the player has any of the specified permissions.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="permissions">The permissions to check for.</param>
    /// <returns>True if the player has any of the specified permissions, false otherwise.</returns>
    private bool UserHasAnyPermission(BasePlayer player, params string[] permissions) =>
        permissions.Any(perm => permission.UserHasPermission(player.UserIDString, perm));

    /// <summary>
    /// Frees the unmanaged resources used by the dictionary list.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to free.</param>
    private static void FreeDictionaryList<TKey, TValue>(ref Dictionary<TKey, List<TValue>> dict)
    {
        foreach (var key in dict.Keys)
        {
            var list = dict[key];
            Pool.FreeUnmanaged(ref list);
        }

        Pool.FreeUnmanaged(ref dict);
    }

    /// <summary>
    /// Tries to get the entity that the player is looking at.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="target">The target entity.</param>
    /// <returns>True if the player is looking at an entity, false otherwise.</returns>
    private bool TryGetPlayerTargetEntity(BasePlayer player, out BaseEntity target, float maxDistance = _defaultMaxTargetDistance)
    {
        // Only scan within 10 blocks.

        Ray ray = new(player.eyes.position, player.eyes.HeadForward());
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, _maskBaseEntities))
        {
            target = hit.GetEntity();
            return target;
        }

        target = null;
        return false;
    }

    /// <summary>
    /// Tries to get the coordinates of the target the player is looking at.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="position">The target coordinates.</param>
    /// <returns>True if the target coordinates were found, false otherwise.</returns>
    private bool TryGetPlayerTargetCoordinates(BasePlayer player, out Vector3 position, float maxDistance = _defaultMaxTargetDistance)
    {
        // Start from the player's eye position
        Vector3 eyePosition = player.eyes.position;
        Vector3 eyeDirection = player.eyes.HeadForward();

        // Create a ray pointing forward and slightly downward
        Vector3 direction = new Vector3(eyeDirection.x, -0.3f, eyeDirection.z).normalized;
        Ray ray = new(eyePosition, direction);

        // Cast the ray and get the hit point
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, _maskDefault | _maskGround))
        {
            position = hit.point;
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Tries to get the snapshot IDs at the specified position.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <param name="snapshotIds">The list to store the snapshot IDs.</param>
    /// <returns>True if snapshot IDs were found, false otherwise.</returns>
    private bool TryGetSnapshotIdsAtPosition(Vector3 position, List<System.Guid> snapshotIds)
    {
        var zones = _zoneSnapshotIndex
            .Where(idx => ZoneContains(idx.Key, position))
            .SelectMany(idx => idx.Value)
            .Distinct();

        snapshotIds.AddRange(zones);

        return snapshotIds.Count > 0;
    }

    /// <summary>
    /// Checks if the zone contains the specified coordinate.
    /// </summary>
    /// <param name="zone">The zone to check.</param>
    /// <param name="coordinate">The coordinate to check.</param>
    /// <returns>True if the zone contains the coordinate, false otherwise.</returns>
    private static bool ZoneContains(Vector4 zone, Vector3 coordinate) =>
        (coordinate - (Vector3)zone).sqrMagnitude <= zone.w * zone.w;

    /// <summary>
    /// Gets the snapshot state for the specified snapshot ID.
    /// </summary>
    /// <param name="snapshotId">The snapshot ID to check.</param>
    /// <returns>The snapshot state.</returns>
    private SnapshotState GetSnapshotState(BasePlayer player, System.Guid snapshotId)
    {
        if (_snapshotHandles.TryGetValue(snapshotId, out var handle))
        {
            if (handle.PlayerUserID != player.userID)
            {
                return SnapshotState.Locked;
            }

            return handle.State;
        }

        return SnapshotState.Idle;
    }

    /// <summary>
    /// Formats the relative time for display.
    /// </summary>
    /// <param name="timeSpan">The time span to format.</param>
    /// <returns>The formatted relative time string.</returns>
    private static string FormatRelativeTime(System.TimeSpan timeSpan)
    {
        List<string> parts = Pool.Get<List<string>>();

        // Add days if they exist
        if (timeSpan.Days > 0)
            parts.Add($"{timeSpan.Days} day{(timeSpan.Days != 1 ? "s" : "")}");

        // Add hours if they exist
        if (timeSpan.Hours > 0)
            parts.Add($"{timeSpan.Hours} hour{(timeSpan.Hours != 1 ? "s" : "")}");

        // Add minutes if they exist
        if (timeSpan.Minutes > 0)
            parts.Add($"{timeSpan.Minutes} minute{(timeSpan.Minutes != 1 ? "s" : "")}");

        // Add seconds if they exist (or if no other time units exist)
        if (timeSpan.Seconds > 0 || parts.Count == 0)
            parts.Add($"{timeSpan.Seconds} second{(timeSpan.Seconds != 1 ? "s" : "")}");

        // Join with commas
        var result = string.Join(", ", parts);

        Pool.FreeUnmanaged(ref parts);

        return result;
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
}
