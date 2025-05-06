using Facepunch;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Carbon.Plugins;

[Info("CopyPasteTests", "hizenxyz", "0.0.1")]
[Description("CopyPasteTests")]
internal class CopyPasteTests : CarbonPlugin
{

    // Base entity masks -- established on Init
    private static int _maskDefault;
    private static int _maskGround;
    private static int _maskDeployed;
    private static int _maskConstruction;
    private static int _maskVehicle;
    private static int _maskHarvestable;
    private static int _maskBaseEntities;

    const string binFile = @"E:\Development\Rust\LocalServers\Carbon.Development\server\carbon\data\CopyPasteTests.bin";

    void Init()
    {
        _maskDefault = LayerMask.GetMask("Default");
        _maskGround = LayerMask.GetMask("Ground", "Terrain", "World");
        _maskDeployed = LayerMask.GetMask("Deployed");
        _maskConstruction = LayerMask.GetMask("Construction");
        _maskVehicle = LayerMask.GetMask("Vehicle Detailed", "Vehicle World", "Vehicle Large");
        _maskHarvestable = LayerMask.GetMask("Harvestable");
        _maskBaseEntities = _maskDefault | _maskDeployed | _maskConstruction | _maskVehicle | _maskHarvestable;
    }

    [ChatCommand("copy")]
    private void CopyCommand(BasePlayer player, string command, string[] args)
    {
        if (!TryGetTargetBuilding(player, out var building))
        {
            return;
        }

        var entities = Pool.Get<List<BaseEntity>>();
        GetEntitiesInBuilding(player, building, entities);

        CopyEntities(player, entities, player.ServerPosition, player.ServerRotation);

        Pool.FreeUnmanaged(ref entities);
    }

    private void GetEntitiesInBuilding(BasePlayer player, BuildingManager.Building building, List<BaseEntity> entities)
    {
        foreach (var block in building.buildingBlocks)
        {
            const float privRadius = 10f;

            var colliders = Physics.OverlapSphere(block.transform.position, privRadius, _maskBaseEntities);
            foreach (var collider in colliders)
            {
                var entity = collider.GetComponentInParent<BaseEntity>();
                if (entity != null && !entities.Contains(entity))
                {
                    entities.Add(entity);
                }
            }
        }
    }

    [ChatCommand("paste")]
    private void PasteCommand(BasePlayer player, string command, string[] args)
    {
        if (!File.Exists(binFile))
        {
            player.ChatMessage("No data to paste.");
            return;
        }

        try
        {
            // Get file size
            var fileInfo = new FileInfo(binFile);
            int fileSize = (int)fileInfo.Length;

            // Rent a buffer from the shared pool
            var buffer = BufferStream.Shared.ArrayPool.Rent(fileSize);

            try
            {
                // Read file data into the buffer
                using (var fileStream = File.OpenRead(binFile))
                {
                    fileStream.Read(buffer, 0, fileSize);
                }

                // Initialize stream with the buffer
                using var stream = Pool.Get<BufferStream>().Initialize(buffer, fileSize);

                // Deserialize from the stream
                using var copyPasteEntityInfo = CopyPasteEntityInfo.Deserialize(stream);

                using var req = Pool.Get<PasteRequest>();
                req.origin = player.ServerPosition;

                req.resources = true;
                req.npcs = true;
                req.vehicles = true;
                req.deployables = true;
                req.foundationsOnly = false;
                req.buildingBlocksOnly = false;
                req.snapToTerrain = false;
                req.players = false;

                var entities = ConVar.CopyPaste.PasteEntities(copyPasteEntityInfo, new(req), player.userID);
                if (entities == null || entities.Count == 0)
                {
                    player.ChatMessage("No entities to paste.");
                    return;
                }

                player.ChatMessage($"Pasted {entities.Count} entities.");
            }
            finally
            {
                // Always return the buffer to the pool
                BufferStream.Shared.ArrayPool.Return(buffer);
            }
        }
        catch (Exception ex)
        {
            player.ChatMessage($"Error during paste: {ex.Message}");
            Debug.LogError($"Paste error: {ex}");
        }
    }

    private void CopyEntities(BasePlayer player, List<BaseEntity> entities, Vector3 originPos, Quaternion originRot)
    {
        try
        {
            ConVar.CopyPaste.OrderEntitiesForSave(entities);
            using var copyPasteEntityInfo = Pool.Get<CopyPasteEntityInfo>();
            copyPasteEntityInfo.entities = Pool.Get<List<Entity>>();

            Transform transform = new GameObject("Align").transform;
            transform.position = originPos;
            transform.rotation = originRot;
            foreach (BaseEntity entity in entities)
            {
                if (!entity.isClient && entity.enableSaving)
                {
                    BaseEntity baseEntity = entity.parentEntity.Get(serverside: true);
                    if (baseEntity != null && (!entities.Contains(baseEntity) || !baseEntity.enableSaving))
                    {
                        Debug.LogWarning("Skipping " + entity.ShortPrefabName + " as it is parented to an entity not included in the copy (it would become orphaned)");
                    }
                    else
                    {
                        ConVar.CopyPaste.SaveEntity(entity, copyPasteEntityInfo, baseEntity, transform);
                    }
                }
            }

            UnityEngine.Object.Destroy(transform.gameObject);

            using var buffer = Pool.Get<BufferStream>().Initialize();  // Ensure proper initialization
            copyPasteEntityInfo.ToProto(buffer);

            var segment = buffer.GetBuffer();
            player.ChatMessage($"Writing {segment.Count} bytes to file, entities: {copyPasteEntityInfo.entities?.Count ?? 0}");

            // Let's also check the first few bytes
            if (segment.Count > 0)
            {
                player.ChatMessage($"First 3 bytes: {segment.Array[segment.Offset]:X2} {segment.Array[segment.Offset + 1]:X2} {segment.Array[segment.Offset + 2]:X2}");
            }

            using var writer = File.Create(binFile);
            writer.Write(segment.Array, segment.Offset, segment.Count);

            player.ChatMessage($"Copy completed successfully, file: {binFile}");
        }
        catch (Exception ex)
        {
            player.ChatMessage($"Error during copy: {ex.Message}");
            Debug.LogError($"Copy error: {ex}");
        }
    }

    private bool TryGetTargetBuilding(BasePlayer player, out BuildingManager.Building building)
    {
        building = null;
        if (!TryGetTargetEntity(player, out var entity))
        {
            player.ChatMessage("Must be facing construction.");
            return false;
        }

        var priv = entity.GetBuildingPrivilege();
        if (!priv)
        {
            player.ChatMessage("Entity is not part of building privilege.");
            return false;
        }

        building = priv.GetBuilding();
        if (building == null)
        {
            player.ChatMessage("Building is null.");
            return false;
        }

        return true;
    }

    private bool TryGetTargetEntity(BasePlayer player, out BaseEntity entity)
    {
        var raycast = new Ray(player.eyes.position, player.eyes.HeadForward());

        if (Physics.Raycast(raycast, out var hit, 10f, LayerMask.GetMask("Construction")))
        {
            entity = hit.GetEntity();
            return entity != null;
        }

        entity = null;
        return false;
    }
}
