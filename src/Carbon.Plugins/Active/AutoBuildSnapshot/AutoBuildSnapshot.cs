// Reference: HizenLabs.Extensions.UserPreference

namespace Carbon.Plugins;

/// <summary>
/// Creates snapshots of user bases which can then be rolled back to at a later date.
/// </summary>
[Info("AutoBuildSnapshot", "hizenxyz", "25.6.593")]
[Description("Creates snapshots of user bases which can then be rolled back to at a later date.")]
public partial class AutoBuildSnapshot : CarbonPlugin
{
    private static AutoBuildSnapshot _instance;

    #region Hooks

    /// <summary>
    /// Called when a plugin initializes (setting up on plugin load).
    /// </summary>
    void Init()
    {
        Puts("Begin initializing...");

        _instance = this;

        Helpers.Init();
        SaveManager.Init();
        ChangeManagement.Init();
        UserInterface.Init();

        UserInterface.ToggleMenu(BasePlayer.activePlayerList[0]);
    }

    /// <summary>
    /// Called when a plugin is being unloaded (for cleanup).
    /// </summary>
    void Unload()
    {
        UserInterface.Unload();
        ChangeManagement.Unload();
        SaveManager.Unload();
        Helpers.Unload();

        _instance = null;
    }

    /// <summary>
    /// Called when the server has finished initializing (startup complete).
    /// </summary>
    void OnServerInitialized()
    {
        if (_instance == null) return;

        ChangeManagement.ScanBasesToRecord();
    }

    /// <summary>
    /// Called when an entity’s data has been loaded from storage (e.g., during server startup or entity spawning from a save).
    /// </summary>
    /// param name="networkable">The entity that was loaded.</param>
    /// <param name="info">Information about how the entity was loaded.</param>
    void OnEntityLoaded(BuildingPrivlidge priv, BaseNetworkable.LoadInfo info)
    {
        ChangeManagement.StartRecording(priv);
    }

    /// <summary>
    /// Called when an entity is spawned in the world.
    /// </summary>
    /// <param name="networkable">The entity that was spawned.</param>
    void OnEntitySpawned(BuildingPrivlidge priv)
    {
        ChangeManagement.StartRecording(priv);
    }

    /// <summary>
    /// Called when an entity is killed/destroyed (cleanup is initiated). Useful for handling removal logic.
    /// </summary>
    /// <param name="networkable">The entity that was killed.</param>
    void OnEntityKill(BaseEntity entity)
    {
        ChangeManagement.HandleChange(entity, ChangeAction.Kill);
    }

    /// <summary>
    /// Triggered when a player finishes looting an entity or container (loot UI closed).
    /// </summary>
    /// <param name="player">The player who looted the entity.</param>
    /// <param name="entity">The entity that was looted.</param>
    void OnLootEntityEnd(BasePlayer player, BaseEntity entity)
    {
        ChangeManagement.HandleChange(entity, ChangeAction.Update, player);
    }

    /// <summary>
    /// Returning a non-null value cancels default behavior.
    /// </summary>
    /// <param name="entity">The entity that was placed.</param>
    /// <param name="component">The construction component.</param>
    /// <param name="constructionTarget">The target of the construction.</param>
    /// <param name="player">The player who placed the entity.</param>
    void OnConstructionPlace(BaseEntity entity, Construction component, Construction.Target constructionTarget, BasePlayer player)
    {
        ChangeManagement.HandleChange(entity, ChangeAction.Update, player);
    }

    /// <summary>
    /// Called when a building part decays and spawns debris (collapsed structure debris entity created).
    /// </summary>
    /// <param name="entity">The entity that was decayed.</param>
    void OnDebrisSpawned(BaseEntity entity)
    {
        ChangeManagement.HandleChange(entity, ChangeAction.Decay);
    }

    /// <summary>
    /// Called when a player disconnects from the server.
    /// </summary>
    /// <param name="player">The player that disconnected.</param>
    /// <param name="reason">The reason for the disconnection.</param>
    void OnPlayerDisconnected(BasePlayer player, string reason)
    {
        UserInterface.HandleDisconnect(player);
    }

    #endregion
}
