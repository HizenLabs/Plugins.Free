namespace Carbon.Plugins.Active.AutoBuildSnapshot;

/// <summary>
/// Creates snapshots of user bases which can then be rolled back to at a later date.
/// </summary>
[Info("AutoBuildSnapshot", "hizenxyz", "1.0.0")]
[Description("Creates snapshots of user bases which can then be rolled back to at a later date.")]
public partial class AutoBuildSnapshot : CarbonPlugin
{
    private static AutoBuildSnapshot _instance;

    /// <summary>
    /// Initializes the plugin and obtains resources from the pool.
    /// </summary>
    void Init()
    {
        _instance = this;

        Helpers.Init();
        ChangeManagement.Init();
    }

    /// <summary>
    /// Unloads the plugin and frees resources.
    /// </summary>
    void Unload()
    {
        ChangeManagement.Unload();
        Helpers.Unload();

        _instance = null;
    }

    void OnEntitySpawned(BaseNetworkable networkable)
    {
        ChangeManagement.HandleOnEntitySpawned(networkable);
    }

    void OnEntityKill(BaseNetworkable networkable)
    {
        ChangeManagement.HandleOnEntityKill(networkable);
    }

    void OnLootEntityEnd(BasePlayer player, BaseEntity entity)
    {
        ChangeManagement.HandleChange(entity, ChangeAction.Update, player);
    }

    void OnConstructionPlace(BaseEntity entity, Construction component, Construction.Target constructionTarget, BasePlayer player)
    {
        ChangeManagement.HandleChange(entity, ChangeAction.Update, player);
    }

    void OnDebrisSpawned(BaseEntity entity)
    {
        ChangeManagement.HandleChange(entity, ChangeAction.Decay);
    }
}
