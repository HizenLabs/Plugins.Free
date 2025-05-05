using Facepunch;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

[Info("CopyPasteTests", "hizenxyz", "0.0.1")]
[Description("CopyPasteTests")]
internal class CopyPasteTests : CarbonPlugin
{
    byte[] _copyPasteData;

    [ChatCommand("copy")]
    private void CopyCommand(BasePlayer player, string command, string[] args)
    {
        if (!TryGetTargetBuilding(player, out var building))
        {
            return;
        }

        var entities = Pool.Get<List<BaseEntity>>();
        foreach (var entity in building.buildingBlocks
            .Cast<BaseEntity>()
            .Concat(building.decayEntities)
            .Concat(building.buildingPrivileges)
            .Distinct())
        {
            entities.Add(entity);
        }

        _copyPasteData = CopyEntities(player, entities, player.ServerPosition, player.ServerRotation);
    }

    [ChatCommand("paste")]
    private void PasteCommand(BasePlayer player, string command, string[] args)
    {
        if (_copyPasteData == null || _copyPasteData.Length == 0)
        {
            player.ChatMessage("No data to paste.");
            return;
        }

        using var copyPasteEntityInfo = CopyPasteEntityInfo.Deserialize(_copyPasteData);

        using var req = Pool.Get<PasteRequest>();
        req.origin = player.ServerPosition;

        var entities = ConVar.CopyPaste.PasteEntities(copyPasteEntityInfo, new(req), player.userID);
        if (entities == null || entities.Count == 0)
        {
            player.ChatMessage("No entities to paste.");
            return;
        }

        player.ChatMessage($"Pasted {entities.Count} entities.");
    }

    private byte[] CopyEntities(BasePlayer player, List<BaseEntity> entities, Vector3 originPos, Quaternion originRot)
    {
        ConVar.CopyPaste.OrderEntitiesForSave(entities);
        using CopyPasteEntityInfo copyPasteEntityInfo = Pool.Get<CopyPasteEntityInfo>();
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

        Object.Destroy(transform.gameObject);

        return copyPasteEntityInfo.ToProtoBytes();
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
