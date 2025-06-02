using Carbon.Plugins;
using HizenLabs.Extensions.UserPreference.Data;
using HizenLabs.Extensions.UserPreference.Material.API;
using HizenLabs.Extensions.UserPreference.UI;
using System;

namespace HizenLabs.Extensions.UserPreference;

[Info("User Preference API", "hizenxyz", "25.6.1880")]
[Description("Provides an API for the user preference extension, allowing users to customize their experience through settings and preferences.")]
public class UserPreferencePlugin : CarbonPlugin
{
    public void Show(
        CarbonPlugin plugin,
        BasePlayer player,
        Action onConfirm = null,
        Action onCancel = null,
        string title = UIDefaults.Title,
        bool showColor = UIDefaults.ShowColor,
        string labelThemeColor = UIDefaults.LabelThemeColor,
        bool showDisplay = UIDefaults.ShowDisplay,
        string labelDisplay = UIDefaults.LabelDisplay,
        string optionDisplayLight = UIDefaults.OptionDisplayLight,
        string optionDisplayDark = UIDefaults.OptionDisplayDark,
        bool showContrast = UIDefaults.ShowContrast,
        string labelContrast = UIDefaults.LabelContrast,
        string optionContrastStandard = UIDefaults.OptionContrastStandard,
        string optionContrastMedium = UIDefaults.OptionContrastMedium,
        string optionContrastHigh = UIDefaults.OptionContrastHigh
    )
    {
        UserPreferenceUI.Show(
            plugin: plugin,
            player: player,
            onConfirm: onConfirm,
            onCancel: onCancel,
            title: title,
            showColor: showColor,
            labelThemeColor: labelThemeColor,
            showDisplay: showDisplay,
            labelDisplay: labelDisplay,
            optionDisplayLight: optionDisplayLight,
            optionDisplayDark: optionDisplayDark,
            showContrast: showContrast,
            labelContrast: labelContrast,
            optionContrastStandard: optionContrastStandard,
            optionContrastMedium: optionContrastMedium,
            optionContrastHigh: optionContrastHigh
        );
    }

    private MaterialTheme GetTheme(CarbonPlugin plugin, BasePlayer player) => UserPreferenceData.Load(this, player).Theme;

    public string GetPrimary(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).Primary;
    public string GetPrimary(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).Primary.WithOpacity(opacity);

    public string GetOnPrimary(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OnPrimary;
    public string GetOnPrimary(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OnPrimary.WithOpacity(opacity);

    public string GetPrimaryContainer(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).PrimaryContainer;
    public string GetPrimaryContainer(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).PrimaryContainer.WithOpacity(opacity);

    public string GetOnPrimaryContainer(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OnPrimaryContainer;
    public string GetOnPrimaryContainer(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OnPrimaryContainer.WithOpacity(opacity);

    public string GetInversePrimary(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).InversePrimary;
    public string GetInversePrimary(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).InversePrimary.WithOpacity(opacity);

    public string GetSecondary(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).Secondary;
    public string GetSecondary(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).Secondary.WithOpacity(opacity);

    public string GetOnSecondary(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OnSecondary;
    public string GetOnSecondary(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OnSecondary.WithOpacity(opacity);

    public string GetSecondaryContainer(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).SecondaryContainer;
    public string GetSecondaryContainer(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).SecondaryContainer.WithOpacity(opacity);

    public string GetOnSecondaryContainer(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OnSecondaryContainer;
    public string GetOnSecondaryContainer(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OnSecondaryContainer.WithOpacity(opacity);

    public string GetTertiary(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).Tertiary;
    public string GetTertiary(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).Tertiary.WithOpacity(opacity);

    public string GetOnTertiary(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OnTertiary;
    public string GetOnTertiary(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OnTertiary.WithOpacity(opacity);

    public string GetTertiaryContainer(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).TertiaryContainer;
    public string GetTertiaryContainer(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).TertiaryContainer.WithOpacity(opacity);

    public string GetOnTertiaryContainer(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OnTertiaryContainer;
    public string GetOnTertiaryContainer(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OnTertiaryContainer.WithOpacity(opacity);

    public string GetBackground(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).Background;
    public string GetBackground(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).Background.WithOpacity(opacity);

    public string GetOnBackground(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OnBackground;
    public string GetOnBackground(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OnBackground.WithOpacity(opacity);

    public string GetSurface(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).Surface;
    public string GetSurface(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).Surface.WithOpacity(opacity);

    public string GetSurfaceBright(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).SurfaceBright;
    public string GetSurfaceBright(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).SurfaceBright.WithOpacity(opacity);

    public string GetSurfaceContainerLowest(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).SurfaceContainerLowest;
    public string GetSurfaceContainerLowest(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).SurfaceContainerLowest.WithOpacity(opacity);

    public string GetSurfaceContainerLow(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).SurfaceContainerLow;
    public string GetSurfaceContainerLow(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).SurfaceContainerLow.WithOpacity(opacity);

    public string GetSurfaceContainer(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).SurfaceContainer;
    public string GetSurfaceContainer(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).SurfaceContainer.WithOpacity(opacity);

    public string GetSurfaceContainerHigh(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).SurfaceContainerHigh;
    public string GetSurfaceContainerHigh(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).SurfaceContainerHigh.WithOpacity(opacity);

    public string GetSurfaceContainerHighest(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).SurfaceContainerHighest;
    public string GetSurfaceContainerHighest(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).SurfaceContainerHighest.WithOpacity(opacity);

    public string GetOnSurface(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OnSurface;
    public string GetOnSurface(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OnSurface.WithOpacity(opacity);

    public string GetSurfaceVariant(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).SurfaceVariant;
    public string GetSurfaceVariant(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).SurfaceVariant.WithOpacity(opacity);

    public string GetOnSurfaceVariant(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OnSurfaceVariant;
    public string GetOnSurfaceVariant(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OnSurfaceVariant.WithOpacity(opacity);

    public string GetInverseSurface(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).InverseSurface;
    public string GetInverseSurface(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).InverseSurface.WithOpacity(opacity);

    public string GetInverseOnSurface(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).InverseOnSurface;
    public string GetInverseOnSurface(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).InverseOnSurface.WithOpacity(opacity);

    public string GetOutline(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).Outline;
    public string GetOutline(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).Outline.WithOpacity(opacity);

    public string GetOutlineVariant(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OutlineVariant;
    public string GetOutlineVariant(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OutlineVariant.WithOpacity(opacity);

    public string GetShadow(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).Shadow;
    public string GetShadow(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).Shadow.WithOpacity(opacity);

    public string GetScrim(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).Scrim;
    public string GetScrim(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).Scrim.WithOpacity(opacity);

    public string GetSurfaceTint(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).SurfaceTint;
    public string GetSurfaceTint(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).SurfaceTint.WithOpacity(opacity);

    public string GetError(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).Error;
    public string GetError(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).Error.WithOpacity(opacity);

    public string GetOnError(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OnError;
    public string GetOnError(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OnError.WithOpacity(opacity);

    public string GetErrorContainer(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).ErrorContainer;
    public string GetErrorContainer(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).ErrorContainer.WithOpacity(opacity);

    public string GetOnErrorContainer(CarbonPlugin plugin, BasePlayer player) => GetTheme(plugin, player).OnErrorContainer;
    public string GetOnErrorContainer(CarbonPlugin plugin, BasePlayer player, float opacity) => GetTheme(plugin, player).OnErrorContainer.WithOpacity(opacity);
}
