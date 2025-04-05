// Reference: HizenLabs.Carbon.Extensions.FluentUI

using HizenLabs.FluentUI;
using HizenLabs.FluentUI.Enums;
using HizenLabs.FluentUI.Primitives;

namespace Carbon.Plugins;

[Info("TestMenu", "hizenxyz", "0.0.5")]
public class TestMenu : CarbonPlugin
{
    [ChatCommand("menu")]
    private void CommandMenu(BasePlayer player, string command, string[] args)
    {
        var builder = FluentBuilder.Create(this, "Test")
            .Panel(main => main
                .BackgroundColor(FluentColor.Black)
                .RelativePosition(0.1f, 0.1f)
                .AbsoluteSize(200, 200)
                .Panel(green => green
                    .BackgroundColor(FluentColor.Green)
                    .AbsolutePosition(10, 10)
                    .RelativeSize(0.1f, 0.1f)
                )
                .Panel(red => red
                    .BackgroundColor(FluentColor.Red)
                    .AbsolutePosition(10, 10)
                    .RelativeSize(0.1f, 0.1f)
                    .Anchor(FluentAnchor.TopRight)
                )
                .Panel(blue => blue
                    .BackgroundColor(FluentColor.Blue)
                    .AbsolutePosition(10, 10)
                    .RelativeSize(0.1f, 0.1f)
                    .Anchor(FluentAnchor.BottomRight)
                )
                .Panel(darkBlue => darkBlue
                    .BackgroundColor("0 0 80 1")
                    .AbsoluteSize(25, 25)
                    .Anchor(FluentAnchor.BottomLeft)
                )
                .Panel(white => white
                    .BackgroundColor(FluentColor.White)
                    .AbsolutePosition(10, 10)
                    .RelativeSize(0.2f, 0.1f)
                    .Anchor(FluentAnchor.Center)
                )
            )
            .Duration(3)
            .Show(player);
    }
}
