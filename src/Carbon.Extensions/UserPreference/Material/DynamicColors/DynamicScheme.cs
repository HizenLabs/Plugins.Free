using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Enums;
using HizenLabs.Extensions.UserPreference.Material.Palettes;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using HizenLabs.Extensions.UserPreference.Pooling;
using System;

namespace HizenLabs.Extensions.UserPreference.Material.DynamicColors;

internal class DynamicScheme : IDisposable, ITrackedPooled
{
    public Guid TrackingId { get; set; }

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
        var scheme = TrackedPool.Get<DynamicScheme>();
        scheme.SourceColorHct = sourceColorHct;
        scheme.Variant = variant;
        scheme.IsDark = isDark;
        scheme.ContrastLevel = contrastLevel;
        scheme.Platform = platform;
        scheme.SpecVersion = specVersion;
        scheme.PrimaryPalette = primaryPalette;
        scheme.SecondaryPalette = secondaryPalette;
        scheme.TertiaryPalette = tertiaryPalette;
        scheme.NeutralPalette = neutralPalette;
        scheme.NeutralVariantPalette = neutralVariantPalette;
        scheme.ErrorPalette = errorPalette;
        return scheme;
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

    public void Dispose()
    {
        var obj = this;
        TrackedPool.Free(ref obj);
    }

    public void EnterPool()
    {
        SourceColorHct?.Dispose();

        PrimaryPalette?.Dispose();
        SecondaryPalette?.Dispose();
        TertiaryPalette?.Dispose();
        NeutralPalette?.Dispose();
        NeutralVariantPalette?.Dispose();
        ErrorPalette?.Dispose();

        SourceColor = default;
        SourceColorHct = default;
        Variant = default;
        IsDark = false;
        ContrastLevel = 0;
        Platform = DefaultPlatform;
        SpecVersion = DefaultSpecVersion;
        PrimaryPalette = null;
        SecondaryPalette = null;
        TertiaryPalette = null;
        NeutralPalette = null;
        NeutralVariantPalette = null;
        ErrorPalette = null;
    }

    public void LeavePool()
    {
    }
}
