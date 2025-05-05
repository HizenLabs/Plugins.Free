// Reference: HizenLabs.Extensions.ObjectSerializer
using Facepunch;
using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Carbon.Plugins;

[Info("ObjectSerializerTests", "hizenxyz", "0.0.1")]
[Description("Plugin for testing the ObjectSerializer extension")]
public class ObjectSerializerTests : CarbonPlugin
{
    Dictionary<string, MemoryStream> _streams;

    void Init()
    {
        _streams = Pool.Get<Dictionary<string, MemoryStream>>();
    }

    void Unload()
    {
        foreach (var key in _streams.Keys)
        {
            var stream = _streams[key];
            Pool.FreeUnmanaged(ref stream);
        }

        Pool.FreeUnmanaged(ref _streams);
    }

    [ChatCommand("copy")]
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

        if (!_streams.TryGetValue(player.UserIDString, out var stream))
        {
            stream = Pool.Get<MemoryStream>();
            _streams[player.UserIDString] = stream;
        }

        stream.SetLength(0L);
        context.Save(stream);
    }

    [ChatCommand("paste")]
    private void PasteCommand(BasePlayer player, string command, string[] args)
    {
        if (!_streams.TryGetValue(player.UserIDString, out var stream)
            || stream.Length == 0)
        {
            player.ChatMessage("No save found.");
            return;
        }

        if (!TryGetPlayerTargetCoordinates(player, out var coords))
        {
            player.ChatMessage("No target coordinates found.");
            return;
        }

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
