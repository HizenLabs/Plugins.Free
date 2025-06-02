using API.Assembly;
using API.Events;
using Carbon;
using Carbon.Core;
using HizenLabs.Extensions.UserPreference.Data;
using System;

namespace HizenLabs.Extensions.UserPreference;

[Info("User Preference", "hizenxyz", "25.6.1962")]
[Description("User Preference is an extension that enables plugin authors to allow their users to configure their visual preferences, such as theme color, display mode (light/dark), and contrast level.")]
public class UserPreferenceExtension : ICarbonExtension
{
    #region Fields

    public const string CompilerSymbol = "EXTENSION_USER_PREFERENCE";

    public static bool Installed;

    #endregion

    #region Interface

    public void OnLoaded(EventArgs args)
    {
        ThemeCache.Init();

        Community.Runtime.Config.Compiler.ConditionalCompilationSymbols.Add(CompilerSymbol);

        Community.Runtime.Events.Subscribe(CarbonEvent.HooksInstalled, args =>
        {
            if (Installed)
            {
                return;
            }

            Installed = true;

            try
            {
                var package = ModLoader.Package.Get("User Preference", false);
                ModLoader.RegisterPackage(package);

                ModLoader.InitializePlugin(typeof(UserPreferencePlugin), out _, package, precompiled: true);
            }
            catch (Exception e)
            {
                Logger.Error("Failed loading User Preference extension.", e);
            }
        });
    }

    public void Awake(EventArgs args)
    {
    }

    public void OnUnloaded(EventArgs args)
    {
        Community.Runtime.Config.Compiler.ConditionalCompilationSymbols.Remove(CompilerSymbol);

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

    #endregion
}
