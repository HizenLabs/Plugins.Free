using Carbon.Plugins;
using HizenLabs.FluentUI.Internals;

namespace HizenLabs.FluentUI.Extensions;

internal static class CarbonPluginExtensions
{
    public static PluginHandle GetHandle(this CarbonPlugin plugin) =>
        PluginHandleManager.GetHandle(plugin);
}
