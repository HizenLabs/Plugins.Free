// Reference: HizenLabs.Carbon.Extensions.FluentUI

using HizenLabs.FluentUI;
using HizenLabs.FluentUI.Primitives;

namespace Carbon.Plugins;

[Info("TestMenu", "hizenxyz", "0.0.5")]
public class TestMenu : CarbonPlugin
{
    [ChatCommand("menu")]
    private void CommandMenu(BasePlayer player, string command, string[] args)
    {
        var builder = FluentBuilder.Create(this, "Test")
            .Panel(main =>
            {
                main.BackgroundColor(FluentColor.Black)
                    .RelativePosition(0.1f, 0.1f)
                    .RelativeSize(0.2f, 0.2f);
            })
            .Duration(3)
            .Show(player);
    }
}
