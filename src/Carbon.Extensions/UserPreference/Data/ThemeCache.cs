using Facepunch;
using HizenLabs.Extensions.UserPreference.Material.API;
using System.Collections.Generic;

namespace HizenLabs.Extensions.UserPreference.Data;

/// <summary>
/// As long as there are only 400 colors with 19 shade variants + 20 grascale colors (white -> black),
/// then our cache footprint <i>should</i> only be around ~3.2MB.
/// This is calcualted from
/// <code>
/// (colors x shades + grayscaleColors) x estBytes x displayOptions = total bytes
/// (400 * 19 + 20) * 208 * 2 = 3,169,920 = ~3.2MB
/// </code>
/// Assuming an estimated ~208 bytes per <see cref="MaterialTheme"/>
/// <code>
/// 
/// </code>
/// </summary>
internal static class ThemeCache
{
    private static Dictionary<uint, MaterialTheme> _lightThemeCache;
    private static Dictionary<uint, MaterialTheme> _darkThemeCache;

    public static MaterialTheme GetFromRgbaHex(string rgbaHex, bool isDark, MaterialContrast contrast)
    {
        var seedColor = MaterialTheme.GetSeedColorFromRgbaHex(rgbaHex);

        return Get(seedColor, isDark, contrast);
    }

    public static MaterialTheme Get(uint value, bool isDark, MaterialContrast contrast)
    {
        var cache = isDark ? _darkThemeCache : _lightThemeCache;

        if (!cache.TryGetValue(value, out var baseTheme))
        {
            baseTheme = MaterialTheme.Create(value, isDark, MaterialContrast.Standard);

            cache[value] = baseTheme;
        }

        return contrast switch
        {
            MaterialContrast.Standard => baseTheme,
            MaterialContrast.Medium => baseTheme.MediumContrast,
            MaterialContrast.High => baseTheme.HighContrast,
            _ => baseTheme
        };
    }

    public static void Init()
    {
        _lightThemeCache = Pool.Get<Dictionary<uint, MaterialTheme>>();
        _darkThemeCache = Pool.Get<Dictionary<uint, MaterialTheme>>();
    }

    public static void Unload()
    {
        foreach (var theme in _lightThemeCache.Values)
        {
            theme?.Dispose();
        }

        foreach (var theme in _darkThemeCache.Values)
        {
            theme?.Dispose();
        }

        Pool.FreeUnmanaged(ref _lightThemeCache);
        Pool.FreeUnmanaged(ref _darkThemeCache);
    }
}
