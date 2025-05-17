using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.DynamicColors;
using HizenLabs.Extensions.UserPreference.Material.Enums;

namespace HizenLabs.Extensions.UserPreference.Material.Scheme;

public class SchemeTonalSpot : DynamicScheme
{
    public static DynamicScheme Create(
        Hct sourceColorHct,
        bool isDark,
        double contrastLevel,
        SpecVersion specVersion = DefaultSpecVersion,
        Platform platform = DefaultPlatform)
    {
        var spec = ColorSpecs.Get(specVersion);
        var variant = Variant.TonalSpot;

        var primaryPalette = spec.GetPrimaryPalette(variant, sourceColorHct, isDark, platform, contrastLevel);
        var secondaryPalette = spec.GetSecondaryPalette(variant, sourceColorHct, isDark, platform, contrastLevel);
        var tertiaryPalette = spec.GetTertiaryPalette(variant, sourceColorHct, isDark, platform, contrastLevel);
        var neutralPalette = spec.GetNeutralPalette(variant, sourceColorHct, isDark, platform, contrastLevel);
        var neutralVariantPalette = spec.GetNeutralVariantPalette(variant, sourceColorHct, isDark, platform, contrastLevel);
        var errorPalette = spec.GetErrorPalette(variant, sourceColorHct, isDark, platform, contrastLevel);

        return Create(
            sourceColorHct: sourceColorHct,
            variant: Variant.TonalSpot,
            isDark: isDark,
            contrastLevel: contrastLevel,
            platform: platform,
            specVersion: specVersion,
            primaryPalette: primaryPalette,
            secondaryPalette: secondaryPalette,
            tertiaryPalette: tertiaryPalette,
            neutralPalette: neutralPalette,
            neutralVariantPalette: neutralVariantPalette,
            errorPalette: errorPalette);
    }
}
