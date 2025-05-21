using API.Assembly;
using System;

namespace HizenLabs.Extensions.UserPreference;

[Info("UserPreference", "hizenxyz", "1.0.0")]
[Description("User Preference is an extension that enables plugin authors to allow their users to configure their visual preferences, such as theme color, display mode (light/dark), and contrast level.")]
public class UserPreferenceExtension : ICarbonExtension
{
    public void Awake(EventArgs args)
    {
    }

    public void OnLoaded(EventArgs args)
    {
    }

    public void OnUnloaded(EventArgs args)
    {
    }
}
