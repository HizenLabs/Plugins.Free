using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using System;

namespace HizenLabs.Extensions.UserPreference.Material.Dislike;

internal static class DislikeAnalyzer
{
    public static bool IsDisliked(Hct hct)
    {
        bool huePasses = Math.Round(hct.Hue) >= 90.0 && Math.Round(hct.Hue) <= 111.0;
        bool chromaPasses = Math.Round(hct.Chroma) > 16.0;
        bool tonePasses = Math.Round(hct.Tone) < 65.0;

        return huePasses && chromaPasses && tonePasses;
    }

    public static Hct FixIfDisliked(Hct hct)
    {
        if (IsDisliked(hct))
        {
            hct?.Dispose();
            return Hct.Create(hct.Hue, hct.Chroma, 70.0);
        }

        return hct;
    }
}
