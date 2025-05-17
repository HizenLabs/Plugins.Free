using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Enums;
using HizenLabs.Extensions.UserPreference.Material.Palettes;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using System;

namespace HizenLabs.Extensions.UserPreference.Material.DynamicColor;

public class DynamicScheme
{
    public static readonly Platform DefaultPlatform = Platform.Phone;
    public static readonly SpecVersion DefaultSpecVersion = SpecVersion.SPEC_2021;

    public static DynamicScheme CreateBasic(StandardRgb seed, bool isDark, double contrastLevel)
    {
        var hct = Hct.Create(seed);

        return Create(hct, Variant.TONAL_SPOT, isDark, contrastLevel);
    }

    public static DynamicScheme Create(
        Hct sourceColorHct,
        Variant variant,
        bool isDark,
        double contrastLevel,
        TonalPalette primaryPalette = null,
        TonalPalette secondaryPalette = null,
        TonalPalette tertiaryPalette = null,
        TonalPalette neutralPalette = null,
        TonalPalette neutralVariantPalette = null,
        TonalPalette errorPalette = null)
    {
        throw new NotImplementedException();
    }
}
