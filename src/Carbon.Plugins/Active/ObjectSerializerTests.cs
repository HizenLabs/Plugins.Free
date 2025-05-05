// Reference: HizenLabs.Extensions.ObjectSerializer
using Carbon.Components;
using Facepunch;
using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

[Info("ObjectSerializerTests", "hizenxyz", "0.0.1")]
[Description("Plugin for testing the ObjectSerializer extension")]
public class ObjectSerializerTests : CarbonPlugin
{
    Dictionary<string, byte[]> _copyData;

    void Init()
    {
        _copyData = Pool.Get<Dictionary<string, byte[]>>();
    }

    void Unload()
    {
        Pool.FreeUnmanaged(ref _copyData);
    }

    [ChatCommand("copyobj")]
    private void TestCommand(BasePlayer player, string command, string[] args)
    {
        var target = GetPlayerTargetEntity(player);
        if (!target)
        {
            player.ChatMessage("No target entity found.");
            return;
        }

        using var context = Pool.Get<SerializationContext>();
        
        var priv = target.GetBuildingPrivilege();
        if (!priv)
        {
            player.ChatMessage("No building privilege found.");
            return;
        }

        context.AddObject(priv);

        var building = priv.GetBuilding();
        foreach (var entity in building.buildingBlocks)
        {
            context.AddObject(entity);
        }

        player.ChatMessage($"Copied {context.Objects.Count} objects.");

        using var stream = new MemoryStream();
        context.Save(stream);
        _copyData[player.UserIDString] = stream.ToArray();
    }

    [ChatCommand("pasteobj")]
    private void PasteCommand(BasePlayer player, string command, string[] args)
    {
        if (!_copyData.TryGetValue(player.UserIDString, out var copyData)
            || copyData.Length == 0)
        {
            player.ChatMessage("No save found.");
            return;
        }

        if (!TryGetPlayerTargetCoordinates(player, out var coords))
        {
            player.ChatMessage("No target coordinates found.");
            return;
        }

        using var cui = CreateCUI();

        var parent = cui.v2.CreateParent(
            CUI.ClientPanels.Overall,
            position: LuiPosition.MiddleCenter,
            name: "myTestContainer");

        parent.SetDestroy("myTestContainer"); // <-- this is important ! clean up the previous parent, must match 'name'

        cui.v2.SendUi(player);

        using var stream = new MemoryStream(copyData);
        using var context = Pool.Get<SerializationContext>();
        context.Load(stream);

        var obj = context.Objects.FirstOrDefault(o => o.GameObject is BuildingPrivlidge);
        if (obj == null 
            || obj.GameObject is not BuildingPrivlidge priv 
            || !priv)
        {
            player.ChatMessage("No building privilege found.");
            return;
        }

        player.ChatMessage($"Found building privilege at index {obj.Index}");

        var offset = coords - priv.transform.position;
        player.ChatMessage($"Offset: {offset}");

        foreach (var o in context.Objects)
        {
            if (o.GameObject is BaseEntity entity)
            {
                entity.ServerPosition += offset;
                entity.Spawn();

                entity.SendNetworkUpdateImmediate(true);
            }
        }
    }

    private bool TryGetPlayerTargetCoordinates(BasePlayer player, out Vector3 coords)
    {
        var raycast = new Ray(player.eyes.position, player.eyes.HeadForward());
        if (Physics.Raycast(raycast, out var hit, 10f))
        {
            coords = hit.point;
            return true;
        }

        coords = Vector3.zero;
        return false;
    }

    private BaseEntity GetPlayerTargetEntity(BasePlayer player)
    {
        var raycast = new Ray(player.eyes.position, player.eyes.HeadForward());
        if (Physics.Raycast(raycast, out var hit, 10f))
        {
            return hit.GetEntity();
        }

        return null;
    }
}
