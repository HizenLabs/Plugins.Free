using Carbon.Plugins;
using Cysharp.Threading.Tasks;
using HizenLabs.Extensions.UserPreference.Material.API;
using HizenLabs.Extensions.UserPreference.Pooling;
using Newtonsoft.Json;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace HizenLabs.Extensions.UserPreference.Data;

/// <summary>
/// Represents user preference data, including theme and other settings.
/// </summary>
public class UserPreferenceData : IDisposable, ITrackedPooled
{
    private static readonly Dictionary<string, UserPreferenceData> _defaults = new();

    [JsonIgnore]
    public Guid TrackingId { get; set; }

    private string _pluginName;
    private string _userId;
    private bool _isDirty;

    /// <summary>
    /// Gets or sets the theme for the user preference data.
    /// </summary>
    public MaterialTheme Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            _isDirty = true;
        }
    }
    private MaterialTheme _theme;

    /// <summary>
    /// Returns whether the user preference data has been initialized.
    /// </summary>
    [JsonIgnore]
    private bool IsInitialized => _pluginName != null && _userId != null;

    #region API

    /// <summary>
    /// Set the default theme for users of your plugin.
    /// </summary>
    /// <param name="plugin">The plugin to set the default theme for.</param>
    /// <param name="theme">The theme to set as default.</param>
    public static void SetDefaultTheme(CarbonPlugin plugin, MaterialTheme theme)
    {
        var defaultPreferences = GetDefaultPreferences(plugin.Name);

        defaultPreferences.Theme = theme;
    }

    public static UserPreferenceData Load(CarbonPlugin plugin, BasePlayer player)
    {
        if (player.UserPreferenceData is not UserPreferenceData userPreferenceData)
        {
            userPreferenceData = Load(plugin.Name, player.UserIDString);
            player.UserPreferenceData = userPreferenceData;
        }

        return userPreferenceData;
    }

    /// <summary>
    /// Loads the user preference data for a specific player, or the default data if no data exists.
    /// </summary>
    /// <param name="plugin">The plugin to load the user preference data for.</param>
    /// <param name="player">The player to load the user preference data for.</param>
    /// <returns>The user preference data for the specified player.</returns>
    public static async UniTask<UserPreferenceData> LoadAsync(CarbonPlugin plugin, BasePlayer player)
    {
        if (player.UserPreferenceData is not UserPreferenceData userPreferenceData)
        {
            var pluginName = plugin.Name;
            var userId = player.UserIDString;

            userPreferenceData = await UniTask.RunOnThreadPool(() => Load(pluginName, userId));
            player.UserPreferenceData = userPreferenceData;
        }

        return userPreferenceData;
    }

    private static UserPreferenceData Load(string pluginName, string userId)
    {
        var data = TrackedPool.Get<UserPreferenceData>();

        var userPreferenceFile = GetUserPreferenceFile(pluginName, userId);
        if (File.Exists(userPreferenceFile))
        {
            try
            {
                var json = File.ReadAllText(userPreferenceFile);
                JsonConvert.PopulateObject(json, data);

                data.Initialize(pluginName, userId);
            }
            catch { }
        }

        if (!data.IsInitialized)
        {
            data.Initialize(pluginName, userId);
            var defaultPreferences = GetDefaultPreferences(pluginName);
            data.Theme = defaultPreferences.Theme;
        }

        return data;
    }

    /// <summary>
    /// Saves the user preference data for the player.
    /// </summary>
    /// <returns></returns>
    public UniTask SaveAsync()
    {
        return UniTask.RunOnThreadPool(Save);
    }

    public void Save()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("User Preference data must first be initialized.");
        }

        if (!_isDirty)
        {
            return;
        }

        var userPreferenceFile = GetUserPreferenceFile(_pluginName, _userId);

        var json = JsonConvert.SerializeObject(this, Formatting.Indented);

        File.WriteAllText(userPreferenceFile, json);
    }

    #endregion

    #region Initialize

    private void Initialize(string pluginName, string userId)
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("User Preference data is already initialized.");
        }

        _pluginName = pluginName;
        _userId = userId;
        _isDirty = false;
    }

    #endregion

    #region Helpers

    private static string GetUserPreferenceFile(string plugin, string playerId)
    {
        var userPreferenceFolder = GetUserPreferenceFolder(plugin);

        return Path.Combine(userPreferenceFolder, $"{playerId}.json");
    }

    private static string GetUserPreferenceFolder(string plugin)
    {
        var userPreferenceFolder = Path.Combine(Interface.Oxide.DataDirectory, "userpref", plugin);

        CreateDirectoryIfNotExist(userPreferenceFolder);

        return Path.Combine(userPreferenceFolder);
    }

    private static void CreateDirectoryIfNotExist(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static UserPreferenceData GetDefaultPreferences(string pluginName)
    {
        if (!_defaults.TryGetValue(pluginName, out var defaultPreferences))
        {
            defaultPreferences = TrackedPool.Get<UserPreferenceData>();
            defaultPreferences.Theme = MaterialTheme.Default;
            _defaults[pluginName] = defaultPreferences;
        }

        return defaultPreferences;
    }

    #endregion

    #region Interface

    public void Dispose()
    {
        var obj = this;
        TrackedPool.Free(ref obj);
    }

    public void EnterPool()
    {
        _pluginName = null;
        _userId = null;
        _isDirty = default;

        Theme = default;
    }

    public void LeavePool() { }

    #endregion
}
