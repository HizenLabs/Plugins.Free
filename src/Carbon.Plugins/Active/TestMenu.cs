// Reference: HizenLabs.Carbon.Extensions.FluentUI

using HizenLabs.FluentUI;

namespace Carbon.Plugins;

[Info("TestMenu", "hizenxyz", "0.0.3")]
public class TestMenu : CarbonPlugin
{
    [ChatCommand("menu")]
    private void CommandMenu(BasePlayer player, string command, string[] args)
    {
        var builder = FluentBuilder.Create(this, "Test")
            .Panel(panel =>
            {

            })
            .Duration(3)
            .Show(player);
    }
}
