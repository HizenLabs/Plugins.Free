using Facepunch;
using System.Collections.Generic;
using System.Security.Policy;

namespace Carbon.Plugins;

[Info("AutoBaseBackup", "hizenxyz", "0.0.1")]
[Description("Automatically backs up a player's base when they build to it, allowing it to be restored later.")]
public class AutoBaseBackup : CarbonPlugin
{
    #region Fields


    #endregion

    #region Hooks

    void Init() => Setup();

    void Unload() => Shutdown();

    #endregion

    #region Commands

    [ChatCommand("test")]
    private void CommandTest(BasePlayer player, string command, string[] args)
    {
    }

    #endregion

    #region Setup

    private void Setup()
    {
    }

    #endregion

    #region Shutdown

    private void Shutdown()
    {
    }

    #endregion
}
