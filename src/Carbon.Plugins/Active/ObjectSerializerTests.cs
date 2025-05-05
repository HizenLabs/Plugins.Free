// Reference: HizenLabs.Extensions.ObjectSerializer
using Facepunch;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using UnityEngine;

namespace Carbon.Plugins;

[Info("ObjectSerializerTests", "hizenxyz", "0.0.1")]
[Description("Plugin for testing the ObjectSerializer extension")]
public class ObjectSerializerTests : CarbonPlugin
{
    [ChatCommand("test")]
    private void TestCommand(BasePlayer player, string command, string[] args)
    {
        player.ChatMessage("ObjectSerializerTests plugin is working!");

        var target = GetPlayerTargetEntity(player);
        if (!target)
        {
            player.ChatMessage("No target entity found.");
            return;
        }

        using var context = Pool.Get<SerializationContext>();
        
        var priv = target.GetBuildingPrivilege();
        context.AddObject(priv);

        var building = priv.GetBuilding();
        foreach (var entity in building.buildingBlocks)
        {
            context.AddObject(entity);
        }
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
