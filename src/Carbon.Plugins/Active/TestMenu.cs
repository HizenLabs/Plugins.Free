// Reference: HizenLabs.Carbon.Extensions.FluentUI

using HizenLabs.FluentUI;
using HizenLabs.FluentUI.Enums;
using HizenLabs.FluentUI.Primitives;

namespace Carbon.Plugins;

[Info("TestMenu", "hizenxyz", "0.0.13")]
public class TestMenu : CarbonPlugin
{
    /// <summary>
    /// Create an extremely basic panel with the specified color
    /// </summary>
    [ChatCommand("basic")]
    private void CommandBasic(BasePlayer player, string command, string[] args)
    {
        FluentColor color;
        float duration = 3;
        if (args.Length == 0)
        {
            Puts("No color specified, using default 'black'");
            color = FluentColor.Black;
        }
        else
        {
            if (args.Length > 1 && float.TryParse(args[1], out duration))
            {
                if (duration > 10)
                {
                    Puts("Duration too long, bringing down to 10 seconds");
                    duration = 10;
                }
            }
            color = FluentColor.ParseName(args[0]);
        }

        using var builder = FluentBuilder.Create(this, "basic")
            .Panel(p => p
                .BackgroundColor(color)
                .Anchor(FluentAnchor.Center)
                .AbsoluteSize(200, 200)
            )
            .Duration(duration)
            .Show(player);
    }

    [ChatCommand("sequence")]
    private void CommandSequence(BasePlayer player, string command, string[] args)
    {
        float currentDelay = 0.5f;
        float nextDelay() => currentDelay += 0.5f;

        using var builder = FluentBuilder.Create(this, "sequence")
            .Panel(inner => inner
                .BackgroundColor(FluentColor.Black)
                .AbsoluteSize(50, 50)
                .AbsolutePosition(10, 10)
                .Fade(0.5f)
                .Delay(nextDelay())
            )
            .Panel(inner => inner
                .BackgroundColor(FluentColor.Black)
                .AbsoluteSize(50, 50)
                .AbsolutePosition(70, 10)
                .Fade(0.5f)
                .Delay(nextDelay())
            )
            .Panel(inner => inner
                .BackgroundColor(FluentColor.Black)
                .AbsoluteSize(50, 50)
                .AbsolutePosition(130, 10)
                .Fade(0.5f)
                .Delay(nextDelay())
            )
            .Panel(inner => inner
                .BackgroundColor(FluentColor.Black)
                .AbsoluteSize(50, 50)
                .AbsolutePosition(190, 10)
                .Fade(0.5f)
                .Delay(nextDelay())
            )
            .Panel(inner => inner
                .BackgroundColor(FluentColor.Black)
                .AbsoluteSize(50, 50)
                .AbsolutePosition(250, 10)
                .Fade(0.5f)
                .Delay(nextDelay())
            )
            .Panel(inner => inner
                .BackgroundColor(FluentColor.Black)
                .AbsoluteSize(50, 50)
                .AbsolutePosition(10, 70)
                .Fade(0.5f)
                .Delay(nextDelay())
            )
            .Panel(inner => inner
                .BackgroundColor(FluentColor.Black)
                .AbsoluteSize(50, 50)
                .AbsolutePosition(70, 70)
                .Fade(0.5f)
                .Delay(nextDelay())
            )
            .Panel(inner => inner
                .BackgroundColor(FluentColor.Black)
                .AbsoluteSize(50, 50)
                .AbsolutePosition(130, 70)
                .Fade(0.5f)
                .Delay(nextDelay())
            )
            .Panel(inner => inner
                .BackgroundColor(FluentColor.Black)
                .AbsoluteSize(50, 50)
                .AbsolutePosition(190, 70)
                .Fade(0.5f)
                .Delay(nextDelay())
            )
            .Panel(inner => inner
                .BackgroundColor(FluentColor.Black)
                .AbsoluteSize(50, 50)
                .AbsolutePosition(250, 70)
                .Fade(0.5f)
                .Delay(nextDelay())
            )
            .Duration(8)
            .Show(player);
    }

    [ChatCommand("popup")]
    private void CommandPopup(BasePlayer player, string command, string[] args)
    {
        using var builder = FluentBuilder.Create(this, "popup")
            .Panel(main => main
                .Anchor(FluentAnchor.BottomCenter)
                .BackgroundColor(FluentColor.Black.SetAlpha(0.7f))
                .AbsoluteSize(200, 100)
                .RelativePosition(0, 0.1f)
                .Panel(green => green
                    .BackgroundColor(FluentColor.Green)
                    .Anchor(FluentAnchor.Center)
                    .AbsolutePosition(10, 10)
                    .RelativeSize(0.6f, 0.3f)
                )
            )
            .AsPopup(5, 1.5f, 1.5f)
            .Show(player);
    }

    [ChatCommand("relative")]
    private void CommandMenu(BasePlayer player, string command, string[] args)
    {
        using var builder = FluentBuilder.Create(this, "relative")
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
