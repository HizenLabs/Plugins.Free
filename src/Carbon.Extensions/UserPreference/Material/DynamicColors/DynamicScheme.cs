using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Enums;
using HizenLabs.Extensions.UserPreference.Material.Palettes;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using System;

namespace HizenLabs.Extensions.UserPreference.Material.DynamicColors;

public class DynamicScheme
{
    public const Platform DefaultPlatform = Platform.Phone;
    public const SpecVersion DefaultSpecVersion = SpecVersion.SPEC_2021;

    public StandardRgb SourceColor { get; private set; }

    public Hct SourceColorHct { get; private set; }

    public Variant Variant { get; private set; }

    public bool IsDark { get; private set; }

    public double ContrastLevel { get; private set; }

    public Platform Platform { get; private set; } = DefaultPlatform;

    public SpecVersion SpecVersion { get; private set; } = DefaultSpecVersion;

    public TonalPalette PrimaryPalette { get; private set; }

    public TonalPalette SecondaryPalette { get; private set; }

    public TonalPalette TertiaryPalette { get; private set; }

    public TonalPalette NeutralPalette { get; private set; }

    public TonalPalette NeutralVariantPalette { get; private set; }

    public TonalPalette ErrorPalette { get; private set; }

    public static DynamicScheme Create(
        Hct sourceColorHct,
        Variant variant,
        bool isDark,
        double contrastLevel,
        Platform platform,
        SpecVersion specVersion,
        TonalPalette primaryPalette,
        TonalPalette secondaryPalette,
        TonalPalette tertiaryPalette,
        TonalPalette neutralPalette,
        TonalPalette neutralVariantPalette,
        TonalPalette errorPalette = null)
    {
        return new()
        {
            SourceColorHct = sourceColorHct,
            Variant = variant,
            IsDark = isDark,
            ContrastLevel = contrastLevel,
            Platform = platform,
            SpecVersion = specVersion,
            PrimaryPalette = primaryPalette,
            SecondaryPalette = secondaryPalette,
            TertiaryPalette = tertiaryPalette,
            NeutralPalette = neutralPalette,
            NeutralVariantPalette = neutralVariantPalette,
            ErrorPalette = errorPalette
        };
    }

    public static double GetRotatedHue(
        Hct sourceColorHct,
        double[] hueBreakpoints,
        double[] rotations)
    {
        double rotation = GetPiecewiseValue(sourceColorHct, hueBreakpoints, rotations);
        if (Math.Min(hueBreakpoints.Length - 1, rotations.Length) <= 0)
        {
            // No condition matched, return the source hue.
            rotation = 0;
        }
        return MathUtils.SanitizeDegrees(sourceColorHct.Hue + rotation);
    }

    public static double GetPiecewiseValue(
        Hct sourceColorHct,
        double[] hueBreakpoints,
        double[] hues)
    {
        int size = Math.Min(hueBreakpoints.Length - 1, hues.Length);
        double sourceHue = sourceColorHct.Hue;
        for (int i = 0; i < size; i++)
        {
            if (sourceHue >= hueBreakpoints[i] && sourceHue < hueBreakpoints[i + 1])
            {
                return MathUtils.SanitizeDegrees(hues[i]);
            }
        }
        // No condition matched, return the source value.
        return sourceHue;
    }
}
