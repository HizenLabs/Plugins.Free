using Carbon.Components;
using Oxide.Game.Rust.Cui;

namespace HizenLabs.FluentUI.Extensions;

internal static class CUIExtensions
{
    /// <summary>
    /// Sends the container ui to all the specified players.
    /// </summary>
    /// <param name="cui">The CUI instance to use.</param>
    /// <param name="container">The container to send.</param>
    /// <param name="players">The players to send the container to.</param>
    public static void SendAll(this CUI cui, CuiElementContainer container, params BasePlayer[] players)
    {
        foreach(var player in players)
        {
            cui.Send(container, player);
        }
    }

    /// <summary>
    /// Destroys the specified UI element for all players.
    /// </summary>
    /// <param name="cui">The CUI instance to use.</param>
    /// <param name="name">The name of the UI element to destroy.</param>
    /// <param name="players">The players for whom to destroy the UI element.</param>
    public static void DestroyAll(this CUI cui, string name, params BasePlayer[] players)
    {
        foreach(var player in players)
        {
            cui.Destroy(name, player);
        }
    }
}
