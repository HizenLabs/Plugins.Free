using API.Assembly;
using Carbon;
using HizenLabs.Extensions.UserPreference.Data;
using System;

namespace HizenLabs.Extensions.UserPreference;

[Info("User Preference", "hizenxyz", "25.6.713")]
[Description("User Preference is an extension that enables plugin authors to allow their users to configure their visual preferences, such as theme color, display mode (light/dark), and contrast level.")]
public class UserPreferenceExtension : ICarbonExtension
{
    public void Awake(EventArgs args)
    {
    }

    public void OnLoaded(EventArgs args)
    {
        ThemeCache.Init();
    }

    public void OnUnloaded(EventArgs args)
    {
        ThemeCache.Unload();

        foreach (var player in BasePlayer.activePlayerList)
        {
            if (player.UserPreferenceData is UserPreferenceData userPreferenceData
                && userPreferenceData.IsInitialized)
            {
                try
                {
                    userPreferenceData.Save();
                }
                catch
                {
                    Logger.Warn($"Failed to save user preference data for player {player.displayName}");
                }
                finally
                {
                    userPreferenceData?.Dispose();
                }
            }
        }
    }
}
