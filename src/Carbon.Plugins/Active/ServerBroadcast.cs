namespace Carbon.Plugins;

[Info("Server Broadcast", "hizenxyz", "25.6.713")]
[Description("Allows broadcasting formatted server messages from the console.")]
public class ServerBroadcast : CarbonPlugin
{
    private const string command_broadcast = "broadcast";
    private const string command_broadcast_icon = "broadcast.icon";
    private const string command_broadcast_user = "broadcast.user";

    private const string permission_broadcast = "broadcast.use";
    private const string permission_broadcast_icon = "broadcast.icon";
    private const string permission_broadcast_user = "broadcast.user";

    private void Init()
    {
        permission.RegisterPermission(permission_broadcast, this);
        permission.RegisterPermission(permission_broadcast_icon, this);
        permission.RegisterPermission(permission_broadcast_user, this);
    }

    /// <summary>
    /// Sends a broadcast to all players on the server.
    /// </summary>
    [Command(command_broadcast), Permission(permission_broadcast)]
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
    [Command(command_broadcast_icon), Permission(permission_broadcast_icon)]
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

        var message = string.Join(" ", arg.Args, 1, arg.Args.Length - 1);
        server.Broadcast(message, player.userID);
    }

    /// <summary>
    /// Sends a broadcast to all players on the server with the specified user's name and icon.
    /// </summary>
    [Command(command_broadcast_user), Permission(permission_broadcast_user)]
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

        var message = string.Join(" ", arg.Args, 1, arg.Args.Length - 1);
        server.Broadcast(message, player.displayName, player.userID);
    }
}
