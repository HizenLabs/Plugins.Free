namespace Carbon.Plugins;

/// <summary>
/// Creates snapshots of user bases which can then be rolled back to at a later date.
/// </summary>
[Info("AutoBuildSnapshot", "hizenxyz", "1.0.0")]
[Description("Creates snapshots of user bases which can then be rolled back to at a later date.")]
public partial class AutoBuildSnapshot : CarbonPlugin
{
    private static AutoBuildSnapshot _instance;

    /// <summary>
    /// Called when a plugin initializes (setting up on plugin load).
    /// </summary>
    void Init()
    {
        _instance = this;

        Helpers.Init();
        ChangeManagement.Init();
    }

    /// <summary>
    /// Called when a plugin is being unloaded (for cleanup).
    /// </summary>
    void Unload()
    {
        ChangeManagement.Unload();
        Helpers.Unload();

        _instance = null;
    }

    /// <summary>
    /// Called when an entity is spawned in the world.
    /// </summary>
    /// <param name="networkable">The entity that was spawned.</param>
    void OnEntitySpawned(BaseNetworkable networkable)
    {
        ChangeManagement.HandleOnEntitySpawned(networkable);
    }

    /// <summary>
    /// Called when an entity is killed/destroyed (cleanup is initiated). Useful for handling removal logic.
    /// </summary>
    /// <param name="networkable">The entity that was killed.</param>
    void OnEntityKill(BaseNetworkable networkable)
    {
        ChangeManagement.HandleOnEntityKill(networkable);
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
}
