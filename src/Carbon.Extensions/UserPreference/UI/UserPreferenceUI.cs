using Carbon.Modules;
using HizenLabs.Extensions.UserPreference.Data;
using System.Collections.Generic;
using HizenLabs.Extensions.UserPreference.Material.API;
using Carbon.Base;
using System;
using Carbon.Plugins;
using Cysharp.Threading.Tasks;

namespace HizenLabs.Extensions.UserPreference.UI;

/// <summary>
/// Provides helper methods to display modals for configuring user preferences, 
/// such as theme color, display mode (light/dark), and contrast level.
/// </summary>
public static class UserPreferenceUI
{
    private static readonly string[] _displayOptions = new string[2];
    private static readonly string[] _contrastOptions = new string[3];

    /// <summary>
    /// Displays a modal window allowing users to configure their visual preferences.
    /// </summary>
    /// <param name="plugin">Reference to the plugin instance (used for saving user data).</param>
    /// <param name="player">The player for whom the modal will be shown.</param>
    /// <param name="onConfirm">Callback executed if the user confirms the changes.</param>
    /// <param name="onCancel">Callback executed if the user cancels the modal.</param>
    /// <param name="title">Title of the modal window.</param>
    /// <param name="labelThemeColor">Label for the theme color field.</param>
    /// <param name="labelDisplay">Label for the display mode field (Light/Dark).</param>
    /// <param name="optionDisplayLight">Text for the 'Light' display option.</param>
    /// <param name="optionDisplayDark">Text for the 'Dark' display option.</param>
    /// <param name="labelContrast">Label for the contrast selection field.</param>
    /// <param name="optionContrastStandard">Text for the 'Standard' contrast option.</param>
    /// <param name="optionContrastMedium">Text for the 'Medium' contrast option.</param>
    /// <param name="optionContrastHigh">Text for the 'High' contrast option.</param>
    public static void Show(
        CarbonPlugin plugin,
        BasePlayer player,
        Action onConfirm = null,
        Action onCancel = null,
        string title = "User Preference",
        bool showColor = true,
        string labelThemeColor = "Theme Color",
        bool showDisplay = true,
        string labelDisplay = "Display",
        string optionDisplayLight = "Light",
        string optionDisplayDark = "Dark",
        bool showContrast = true,
        string labelContrast = "Contrast",
        string optionContrastStandard = "Standard",
        string optionContrastMedium = "Medium",
        string optionContrastHigh = "High"
    )
    {
        var userPreferenceData = UserPreferenceData.Load(plugin, player);
        var fields = new Dictionary<string, ModalModule.Modal.Field>();

        if (showColor)
        {
            fields["color"] = ModalModule.Modal.Field.Make(
                displayName: labelThemeColor,
                type: ModalModule.Modal.Field.FieldTypes.HexColor,
                required: true,
                @default: userPreferenceData.Theme.SeedColor.ToRgbaHex());
        }

        if (showDisplay)
        {
            _displayOptions[0] = optionDisplayLight;
            _displayOptions[1] = optionDisplayDark;

            var displaySelected = userPreferenceData.Theme.IsDarkMode ? 1 : 0;
            fields["display"] = ModalModule.Modal.EnumField.MakeEnum(
                displayName: labelDisplay,
                options: _displayOptions,
                required: true,
                @default: displaySelected,
                isReadOnly: false);
        }

        if (showContrast)
        {
            _contrastOptions[0] = optionContrastStandard;
            _contrastOptions[1] = optionContrastMedium;
            _contrastOptions[2] = optionContrastHigh;

            var contrastSelected = userPreferenceData.Theme.Contrast switch
            {
                MaterialContrast.Medium => 1,
                MaterialContrast.High => 2,
                _ => 0
            };
            fields["contrast"] = ModalModule.Modal.EnumField.MakeEnum(
                displayName: labelContrast,
                options: _contrastOptions,
                required: true,
                @default: contrastSelected,
                isReadOnly: false);
        }

        var modalModule = BaseModule.GetModule<ModalModule>();
        modalModule.Open(player, title, fields, (player, modal) => HandleShowConfirm(plugin, player, modal, onConfirm).Forget(), onCancel);
    }

    /// <summary>
    /// Handles the logic when the user confirms the modal, updating their preferences if needed.
    /// </summary>
    private static async UniTaskVoid HandleShowConfirm(CarbonPlugin plugin, BasePlayer player, ModalModule.Modal modal, Action onConfirm)
    {
        var themeColor = modal.Get<string>("color");
        var display = modal.Get<int>("display");
        var contrast = modal.Get<int>("contrast");

        var isDarkMode = display == 1;
        var materialContrast = contrast switch
        {
            1 => MaterialContrast.Medium,
            2 => MaterialContrast.High,
            _ => MaterialContrast.Standard
        };

        var userPreferenceData = await UserPreferenceData.LoadAsync(plugin, player);
        if (!userPreferenceData.Theme.SeedColor.ToRgbaHex().Equals(themeColor, StringComparison.OrdinalIgnoreCase)
            || userPreferenceData.Theme.IsDarkMode != isDarkMode
            || userPreferenceData.Theme.Contrast != materialContrast)
        {
            var theme = await UniTask.RunOnThreadPool(() => ThemeCache.GetFromRgbaHex(themeColor, isDarkMode, materialContrast));
            userPreferenceData.Theme = theme;
        }

        await userPreferenceData.SaveAsync();
        onConfirm?.Invoke();
    }
}
