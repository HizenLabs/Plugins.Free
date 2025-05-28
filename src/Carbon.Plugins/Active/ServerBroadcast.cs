using System.Linq;

namespace Carbon.Plugins;

[Info("Server Broadcast", "hizenxyz", "25.5.38860")]
[Description("Allows broadcasting formatted server messages from the console.")]
public class ServerBroadcast : CarbonPlugin
{
    /// <summary>
    /// Sends a broadcast to all players on the server.
    /// </summary>
    [ConsoleCommand("broadcast"), Permission("broadcast.admin"), Permission("broadcast.use")]
    private void BroadcastCommand(ConsoleSystem.Arg arg)
    {
        if (arg.Args.Length < 1)
        {
            arg.ReplyWith("Usage: broadcast <message>");
            return;
        }

        var message = string.Join(" ", arg.Args);
        server.Broadcast(message);
    }

    /// <summary>
    /// Sends a broadcast to all players on the server with the specified user's icon.
    /// </summary>
    [ConsoleCommand("broadcast.icon"), Permission("broadcast.admin"), Permission("broadcast.icon")]
    private void BroadcastIconCommand(ConsoleSystem.Arg arg)
    {
        if (arg.Args.Length < 2)
        {
            arg.ReplyWith("Usage: broadcast.useicon <user> <message>");
            return;
        }

        var arg1 = arg.Args[0];
        var player = BasePlayer.Find(arg1);
        if (player == null)
        {
            arg.ReplyWith($"Player not found: '{arg1}'");
            return;
        }

        var message = string.Join(" ", arg.Args.Skip(1));
        server.Broadcast(message, player.displayName, player.userID);
    }

    /// <summary>
    /// Sends a broadcast to all players on the server with the specified user's name and icon.
    /// </summary>
    [ConsoleCommand("broadcast.user"), Permission("broadcast.admin"), Permission("broadcast.user")]
    private void BroadcastUserCommand(ConsoleSystem.Arg arg)
    {
        if (arg.Args.Length < 2)
        {
            arg.ReplyWith("Usage: broadcast.user <user> <message>");
            return;
        }

        var arg1 = arg.Args[0];
        var player = BasePlayer.Find(arg1);
        if (player == null)
        {
            arg.ReplyWith($"Player not found: '{arg1}'");
            return;
        }

        var message = string.Join(" ", arg.Args.Skip(1));
        server.Broadcast(message, player.displayName, player.userID);
    }
}
