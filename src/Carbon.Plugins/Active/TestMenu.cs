// Reference: HizenLabs.Carbon.Extensions.FluentUI

using HizenLabs.FluentUI;
using System.Collections.Generic;

namespace Carbon.Plugins;

[Info("TestMenu", "hizenxyz", "0.0.3")]
public class TestMenu : CarbonPlugin
{
    Dictionary<string, FluentBuilder> menus = new();

    [ChatCommand("menu")]
    private void CommandMenu(BasePlayer player, string command, string[] args)
    {
    }
}