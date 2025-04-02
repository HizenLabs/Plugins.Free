using Carbon.Plugins;
using Oxide.Game.Rust.Cui;
using System;

namespace HizenLabs.FluentUI;

/// <summary>
/// Builder for constructing and sending UI containers to players.
/// </summary>
public class FluentBuilder
{
    private readonly CarbonPlugin _plugin;

    public FluentBuilder(CarbonPlugin plugin)
    {
        _plugin = plugin;
    }

    public CuiElementContainer Build()
    {
        throw new NotImplementedException();
    }
}
