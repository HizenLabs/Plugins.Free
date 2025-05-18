using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Dislike;
using HizenLabs.Extensions.UserPreference.Material.Enums;
using HizenLabs.Extensions.UserPreference.Material.Palettes;
using HizenLabs.Extensions.UserPreference.Material.Temperature;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using System;

namespace HizenLabs.Extensions.UserPreference.Material.DynamicColors;

public sealed class ColorSpec2021 : IColorSpec
{
    #region Main Palettes

    public DynamicColor PrimaryPaletteKeyColor => Build(b => b
        .SetName("primary_palette_key_color")
        .SetPalette(s => s.PrimaryPalette)
        .SetTone(s => s.PrimaryPalette.KeyColor.Tone)
    );

    public DynamicColor SecondaryPaletteKeyColor => Build(b => b
        .SetName("secondary_palette_key_color")
        .SetPalette(s => s.SecondaryPalette)
        .SetTone(s => s.SecondaryPalette.KeyColor.Tone)
    );

    public DynamicColor TertiaryPaletteKeyColor => Build(b => b
        .SetName("tertiary_palette_key_color")
        .SetPalette(s => s.TertiaryPalette)
        .SetTone(s => s.TertiaryPalette.KeyColor.Tone)
    );

    public DynamicColor NeutralPaletteKeyColor => Build(b => b
        .SetName("neutral_palette_key_color")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.NeutralPalette.KeyColor.Tone)
    );

    public DynamicColor NeutralVariantPaletteKeyColor => Build(b => b
        .SetName("neutral_variant_palette_key_color")
        .SetPalette(s => s.NeutralVariantPalette)
        .SetTone(s => s.NeutralVariantPalette.KeyColor.Tone)
    );

    public DynamicColor ErrorPaletteKeyColor => Build(b => b
        .SetName("error_palette_key_color")
        .SetPalette(s => s.ErrorPalette)
        .SetTone(s => s.ErrorPalette.KeyColor.Tone)
    );

    #endregion

    #region Surfaces [S]

    public DynamicColor Background => Build(b => b
        .SetName("background")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark ? 6.0 : 98.0)
        .SetIsBackground(true)
    );

    public DynamicColor OnBackground => Build(b => b
        .SetName("on_background")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark ? 90.0 : 10.0)
        .SetBackground(s => Background)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 3.0, 4.5, 7.0))
    );

    public DynamicColor Surface => Build(b => b
        .SetName("surface")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark ? 6.0 : 98.0)
        .SetIsBackground(true)
    );

    public DynamicColor SurfaceDim => Build(b => b
        .SetName("surface_dim")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark ? 6.0 : ContrastCurve.Create(87.0, 87.0, 80.0, 75.0).Get(s.ContrastLevel))
        .SetIsBackground(true)
    );

    public DynamicColor SurfaceBright => Build(b => b
        .SetName("surface_bright")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark ? ContrastCurve.Create(24.0, 24.0, 29.0, 34.0).Get(s.ContrastLevel) : 98.0)
        .SetIsBackground(true)
    );

    public DynamicColor SurfaceContainerLowest => Build(b => b
        .SetName("surface_container_lowest")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark ? ContrastCurve.Create(4.0, 4.0, 2.0, 0.0).Get(s.ContrastLevel) : 100.0)
        .SetIsBackground(true)
    );

    public DynamicColor SurfaceContainerLow => Build(b => b
        .SetName("surface_container_low")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark
            ? ContrastCurve.Create(10.0, 10.0, 11.0, 12.0).Get(s.ContrastLevel)
            : ContrastCurve.Create(96.0, 96.0, 96.0, 95.0).Get(s.ContrastLevel))
        .SetIsBackground(true)
    );

    public DynamicColor SurfaceContainer => Build(b => b
        .SetName("surface_container")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark
            ? ContrastCurve.Create(12.0, 12.0, 16.0, 20.0).Get(s.ContrastLevel)
            : ContrastCurve.Create(94.0, 94.0, 92.0, 90.0).Get(s.ContrastLevel))
        .SetIsBackground(true)
    );

    public DynamicColor SurfaceContainerHigh => Build(b => b
        .SetName("surface_container_high")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark
            ? ContrastCurve.Create(17.0, 17.0, 21.0, 25.0).Get(s.ContrastLevel)
            : ContrastCurve.Create(92.0, 92.0, 88.0, 85.0).Get(s.ContrastLevel))
        .SetIsBackground(true)
    );

    public DynamicColor SurfaceContainerHighest => Build(b => b
        .SetName("surface_container_highest")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark
            ? ContrastCurve.Create(22.0, 22.0, 26.0, 30.0).Get(s.ContrastLevel)
            : ContrastCurve.Create(90.0, 90.0, 84.0, 80.0).Get(s.ContrastLevel))
        .SetIsBackground(true)
    );

    public DynamicColor OnSurface => Build(b => b
        .SetName("on_surface")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark ? 90.0 : 10.0)
        .SetBackground(s => HighestSurface(s))
        .SetContrastCurve(s => ContrastCurve.Create(4.5, 7.0, 11.0, 21.0))
    );

    public DynamicColor SurfaceVariant => Build(b => b
        .SetName("surface_variant")
        .SetPalette(s => s.NeutralVariantPalette)
        .SetTone(s => s.IsDark ? 30.0 : 90.0)
        .SetIsBackground(true)
    );

    public DynamicColor OnSurfaceVariant => Build(b => b
        .SetName("on_surface_variant")
        .SetPalette(s => s.NeutralVariantPalette)
        .SetTone(s => s.IsDark ? 80.0 : 30.0)
        .SetBackground(s => HighestSurface(s))
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 11.0))
    );

    public DynamicColor InverseSurface => Build(b => b
        .SetName("inverse_surface")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark ? 90.0 : 20.0)
        .SetIsBackground(true)
    );

    public DynamicColor InverseOnSurface => Build(b => b
        .SetName("inverse_on_surface")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => s.IsDark ? 20.0 : 95.0)
        .SetBackground(s => InverseSurface)
        .SetContrastCurve(s => ContrastCurve.Create(4.5, 7.0, 11.0, 21.0))
    );

    public DynamicColor Outline => Build(b => b
        .SetName("outline")
        .SetPalette(s => s.NeutralVariantPalette)
        .SetTone(s => s.IsDark ? 60.0 : 50.0)
        .SetBackground(s => HighestSurface(s))
        .SetContrastCurve(s => ContrastCurve.Create(1.5, 3.0, 4.5, 7.0))
    );

    public DynamicColor OutlineVariant => Build(b => b
        .SetName("outline_variant")
        .SetPalette(s => s.NeutralVariantPalette)
        .SetTone(s => s.IsDark ? 30.0 : 80.0)
        .SetBackground(s => HighestSurface(s))
        .SetContrastCurve(s => ContrastCurve.Create(1.0, 1.0, 3.0, 4.5))
    );

    public DynamicColor Shadow => Build(b => b
        .SetName("shadow")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => 0.0)
    );

    public DynamicColor Scrim => Build(b => b
        .SetName("scrim")
        .SetPalette(s => s.NeutralPalette)
        .SetTone(s => 0.0)
    );

    public DynamicColor SurfaceTint => Build(b => b
        .SetName("surface_tint")
        .SetPalette(s => s.PrimaryPalette)
        .SetTone(s => s.IsDark ? 80.0 : 40.0)
        .SetIsBackground(true)
    );

    #endregion

    #region Primaries [P]

    public DynamicColor Primary => Build(b => b
        .SetName("primary")
        .SetPalette(s => s.PrimaryPalette)
        .SetTone(s =>
            IsMonochrome(s) ? (s.IsDark ? 100.0 : 0.0) : (s.IsDark ? 80.0 : 40.0)
        )
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 7.0))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(PrimaryContainer, Primary, 10.0, TonePolarity.Nearer, false))
    );

    public DynamicColor PrimaryDim => null;

    public DynamicColor OnPrimary => Build(b => b
        .SetName("on_primary")
        .SetPalette(s => s.PrimaryPalette)
        .SetTone(s =>
            IsMonochrome(s) ? (s.IsDark ? 10.0 : 90.0) : (s.IsDark ? 20.0 : 100.0)
        )
        .SetBackground(s => Primary)
        .SetContrastCurve(s => ContrastCurve.Create(4.5, 7.0, 11.0, 21.0))
    );

    public DynamicColor PrimaryContainer => Build(b => b
        .SetName("primary_container")
        .SetPalette(s => s.PrimaryPalette)
        .SetTone(s =>
        {
            if (IsFidelity(s)) return s.SourceColorHct.Tone;
            if (IsMonochrome(s)) return s.IsDark ? 85.0 : 25.0;
            return s.IsDark ? 30.0 : 90.0;
        })
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(1.0, 1.0, 3.0, 4.5))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(PrimaryContainer, Primary, 10.0, TonePolarity.Nearer, false))
    );

    public DynamicColor OnPrimaryContainer => Build(b => b
        .SetName("on_primary_container")
        .SetPalette(s => s.PrimaryPalette)
        .SetTone(s =>
        {
            if (IsFidelity(s)) return DynamicColor.ForegroundTone(PrimaryContainer.Tone(s), 4.5);
            if (IsMonochrome(s)) return s.IsDark ? 0.0 : 100.0;
            return s.IsDark ? 90.0 : 30.0;
        })
        .SetBackground(s => PrimaryContainer)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 11.0))
    );

    public DynamicColor InversePrimary => Build(b => b
        .SetName("inverse_primary")
        .SetPalette(s => s.PrimaryPalette)
        .SetTone(s => s.IsDark ? 40.0 : 80.0)
        .SetBackground(s => InverseSurface)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 7.0))
    );

    #endregion

    #region Secondaries [Q]

    public DynamicColor Secondary => Build(b => b
        .SetName("secondary")
        .SetPalette(s => s.SecondaryPalette)
        .SetTone(s => s.IsDark ? 80.0 : 40.0)
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 7.0))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(SecondaryContainer, Secondary, 10.0, TonePolarity.Nearer, false))
    );

    public DynamicColor SecondaryDim => null;

    public DynamicColor OnSecondary => Build(b => b
        .SetName("on_secondary")
        .SetPalette(s => s.SecondaryPalette)
        .SetTone(s =>
            IsMonochrome(s) ? (s.IsDark ? 10.0 : 100.0) : (s.IsDark ? 20.0 : 100.0)
        )
        .SetBackground(s => Secondary)
        .SetContrastCurve(s => ContrastCurve.Create(4.5, 7.0, 11.0, 21.0))
    );

    public DynamicColor SecondaryContainer => Build(b => b
        .SetName("secondary_container")
        .SetPalette(s => s.SecondaryPalette)
        .SetTone(s =>
        {
            var initialTone = s.IsDark ? 30.0 : 90.0;
            if (IsMonochrome(s)) return s.IsDark ? 30.0 : 85.0;
            if (!IsFidelity(s)) return initialTone;
            return FindDesiredChromaByTone(
                s.SecondaryPalette.Hue,
                s.SecondaryPalette.Chroma,
                initialTone,
                !s.IsDark
            );
        })
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(1.0, 1.0, 3.0, 4.5))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(SecondaryContainer, Secondary, 10.0, TonePolarity.Nearer, false))
    );

    public DynamicColor OnSecondaryContainer => Build(b => b
        .SetName("on_secondary_container")
        .SetPalette(s => s.SecondaryPalette)
        .SetTone(s =>
        {
            if (IsMonochrome(s)) return s.IsDark ? 90.0 : 10.0;
            if (!IsFidelity(s)) return s.IsDark ? 90.0 : 30.0;
            return DynamicColor.ForegroundTone(SecondaryContainer.Tone(s), 4.5);
        })
        .SetBackground(s => SecondaryContainer)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 11.0))
    );

    #endregion

    #region Tertiaries [T]

    public DynamicColor Tertiary => Build(b => b
        .SetName("tertiary")
        .SetPalette(s => s.TertiaryPalette)
        .SetTone(s =>
            IsMonochrome(s) ? (s.IsDark ? 90.0 : 25.0) : (s.IsDark ? 80.0 : 40.0)
        )
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 7.0))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(TertiaryContainer, Tertiary, 10.0, TonePolarity.Nearer, false))
    );

    public DynamicColor TertiaryDim => null;

    public DynamicColor OnTertiary => Build(b => b
        .SetName("on_tertiary")
        .SetPalette(s => s.TertiaryPalette)
        .SetTone(s =>
            IsMonochrome(s) ? (s.IsDark ? 10.0 : 90.0) : (s.IsDark ? 20.0 : 100.0)
        )
        .SetBackground(s => Tertiary)
        .SetContrastCurve(s => ContrastCurve.Create(4.5, 7.0, 11.0, 21.0))
    );

    public DynamicColor TertiaryContainer => Build(b => b
        .SetName("tertiary_container")
        .SetPalette(s => s.TertiaryPalette)
        .SetTone(s =>
        {
            if (IsMonochrome(s)) return s.IsDark ? 60.0 : 49.0;
            if (!IsFidelity(s)) return s.IsDark ? 30.0 : 90.0;
            var proposed = s.TertiaryPalette.GetHct(s.SourceColorHct.Tone);
            using var liked = DislikeAnalyzer.FixIfDisliked(proposed);
            return liked.Tone;
        })
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(1.0, 1.0, 3.0, 4.5))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(TertiaryContainer, Tertiary, 10.0, TonePolarity.Nearer, false))
    );

    public DynamicColor OnTertiaryContainer => Build(b => b
        .SetName("on_tertiary_container")
        .SetPalette(s => s.TertiaryPalette)
        .SetTone(s =>
        {
            if (IsMonochrome(s)) return s.IsDark ? 0.0 : 100.0;
            if (!IsFidelity(s)) return s.IsDark ? 90.0 : 30.0;
            return DynamicColor.ForegroundTone(TertiaryContainer.Tone(s), 4.5);
        })
        .SetBackground(s => TertiaryContainer)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 11.0))
    );

    #endregion

    #region Errors [E]

    public DynamicColor Error => Build(b => b
        .SetName("error")
        .SetPalette(s => s.ErrorPalette)
        .SetTone(s => s.IsDark ? 80.0 : 40.0)
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 7.0))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(ErrorContainer, Error, 10.0, TonePolarity.Nearer, false))
    );

    public DynamicColor ErrorDim => null;

    public DynamicColor OnError => Build(b => b
        .SetName("on_error")
        .SetPalette(s => s.ErrorPalette)
        .SetTone(s => s.IsDark ? 20.0 : 100.0)
        .SetBackground(s => Error)
        .SetContrastCurve(s => ContrastCurve.Create(4.5, 7.0, 11.0, 21.0))
    );

    public DynamicColor ErrorContainer => Build(b => b
        .SetName("error_container")
        .SetPalette(s => s.ErrorPalette)
        .SetTone(s => s.IsDark ? 30.0 : 90.0)
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(1.0, 1.0, 3.0, 4.5))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(ErrorContainer, Error, 10.0, TonePolarity.Nearer, false))
    );

    public DynamicColor OnErrorContainer => Build(b => b
        .SetName("on_error_container")
        .SetPalette(s => s.ErrorPalette)
        .SetTone(s =>
            IsMonochrome(s) ? (s.IsDark ? 90.0 : 10.0) : (s.IsDark ? 90.0 : 30.0)
        )
        .SetBackground(s => ErrorContainer)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 11.0))
    );

    #endregion

    #region Primary Fixed Colors [PF]

    public DynamicColor PrimaryFixed => Build(b => b
        .SetName("primary_fixed")
        .SetPalette(s => s.PrimaryPalette)
        .SetTone(s => IsMonochrome(s) ? 40.0 : 90.0)
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(1.0, 1.0, 3.0, 4.5))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(PrimaryFixed, PrimaryFixedDim, 10.0, TonePolarity.Lighter, true))
    );

    public DynamicColor PrimaryFixedDim => Build(b => b
        .SetName("primary_fixed_dim")
        .SetPalette(s => s.PrimaryPalette)
        .SetTone(s => IsMonochrome(s) ? 30.0 : 80.0)
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(1.0, 1.0, 3.0, 4.5))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(PrimaryFixed, PrimaryFixedDim, 10.0, TonePolarity.Lighter, true))
    );

    public DynamicColor OnPrimaryFixed => Build(b => b
        .SetName("on_primary_fixed")
        .SetPalette(s => s.PrimaryPalette)
        .SetTone(s => IsMonochrome(s) ? 100.0 : 10.0)
        .SetBackground(s => PrimaryFixedDim)
        .SetSecondBackground(s => PrimaryFixed)
        .SetContrastCurve(s => ContrastCurve.Create(4.5, 7.0, 11.0, 21.0))
    );

    public DynamicColor OnPrimaryFixedVariant => Build(b => b
        .SetName("on_primary_fixed_variant")
        .SetPalette(s => s.PrimaryPalette)
        .SetTone(s => IsMonochrome(s) ? 90.0 : 30.0)
        .SetBackground(s => PrimaryFixedDim)
        .SetSecondBackground(s => PrimaryFixed)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 11.0))
    );

    #endregion

    #region Secondary Fixed Colors [QF]

    public DynamicColor SecondaryFixed => Build(b => b
        .SetName("secondary_fixed")
        .SetPalette(s => s.SecondaryPalette)
        .SetTone(s => IsMonochrome(s) ? 80.0 : 90.0)
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(1.0, 1.0, 3.0, 4.5))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(SecondaryFixed, SecondaryFixedDim, 10.0, TonePolarity.Lighter, true))
    );

    public DynamicColor SecondaryFixedDim => Build(b => b
        .SetName("secondary_fixed_dim")
        .SetPalette(s => s.SecondaryPalette)
        .SetTone(s => IsMonochrome(s) ? 70.0 : 80.0)
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(1.0, 1.0, 3.0, 4.5))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(SecondaryFixed, SecondaryFixedDim, 10.0, TonePolarity.Lighter, true))
    );

    public DynamicColor OnSecondaryFixed => Build(b => b
        .SetName("on_secondary_fixed")
        .SetPalette(s => s.SecondaryPalette)
        .SetTone(s => 10.0)
        .SetBackground(s => SecondaryFixedDim)
        .SetSecondBackground(s => SecondaryFixed)
        .SetContrastCurve(s => ContrastCurve.Create(4.5, 7.0, 11.0, 21.0))
    );

    public DynamicColor OnSecondaryFixedVariant => Build(b => b
        .SetName("on_secondary_fixed_variant")
        .SetPalette(s => s.SecondaryPalette)
        .SetTone(s => IsMonochrome(s) ? 25.0 : 30.0)
        .SetBackground(s => SecondaryFixedDim)
        .SetSecondBackground(s => SecondaryFixed)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 11.0))
    );

    #endregion

    #region Tertiary Fixed Colors [TF]

    public DynamicColor TertiaryFixed => Build(b => b
        .SetName("tertiary_fixed")
        .SetPalette(s => s.TertiaryPalette)
        .SetTone(s => IsMonochrome(s) ? 40.0 : 90.0)
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(1.0, 1.0, 3.0, 4.5))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(TertiaryFixed, TertiaryFixedDim, 10.0, TonePolarity.Lighter, true))
    );

    public DynamicColor TertiaryFixedDim => Build(b => b
        .SetName("tertiary_fixed_dim")
        .SetPalette(s => s.TertiaryPalette)
        .SetTone(s => IsMonochrome(s) ? 30.0 : 80.0)
        .SetIsBackground(true)
        .SetBackground(HighestSurface)
        .SetContrastCurve(s => ContrastCurve.Create(1.0, 1.0, 3.0, 4.5))
        .SetToneDeltaPair(s => ToneDeltaPair.Create(TertiaryFixed, TertiaryFixedDim, 10.0, TonePolarity.Lighter, true))
    );

    public DynamicColor OnTertiaryFixed => Build(b => b
        .SetName("on_tertiary_fixed")
        .SetPalette(s => s.TertiaryPalette)
        .SetTone(s => IsMonochrome(s) ? 100.0 : 10.0)
        .SetBackground(s => TertiaryFixedDim)
        .SetSecondBackground(s => TertiaryFixed)
        .SetContrastCurve(s => ContrastCurve.Create(4.5, 7.0, 11.0, 21.0))
    );

    public DynamicColor OnTertiaryFixedVariant => Build(b => b
        .SetName("on_tertiary_fixed_variant")
        .SetPalette(s => s.TertiaryPalette)
        .SetTone(s => IsMonochrome(s) ? 90.0 : 30.0)
        .SetBackground(s => TertiaryFixedDim)
        .SetSecondBackground(s => TertiaryFixed)
        .SetContrastCurve(s => ContrastCurve.Create(3.0, 4.5, 7.0, 11.0))
    );

    #endregion

    #region Android (not-implemented)

    public DynamicColor ControlActivated => throw new NotImplementedException();
    public DynamicColor ControlNormal => throw new NotImplementedException();
    public DynamicColor ControlHighlight => throw new NotImplementedException();
    public DynamicColor TextPrimaryInverse => throw new NotImplementedException();
    public DynamicColor TextSecondaryAndTertiaryInverse => throw new NotImplementedException();
    public DynamicColor TextPrimaryInverseDisableOnly => throw new NotImplementedException();
    public DynamicColor TextSecondaryAndTertiaryInverseDisabled => throw new NotImplementedException();
    public DynamicColor TextHintInverse => throw new NotImplementedException();

    #endregion

    #region Color Value Calculations

    public Hct GetHct(DynamicScheme scheme, DynamicColor color)
    {
        var tone = GetTone(scheme, color);
        return color.Palette(scheme).GetHct(tone);
    }

    public double GetTone(DynamicScheme scheme, DynamicColor color)
    {
        bool decreasingContrast = scheme.ContrastLevel < 0;
        var toneDeltaPair = color.ToneDeltaPair?.Invoke(scheme);

        if (toneDeltaPair != null)
        {
            var roleA = toneDeltaPair.RoleA;
            var roleB = toneDeltaPair.RoleB;
            var delta = toneDeltaPair.Delta;
            var polarity = toneDeltaPair.Polarity;
            var stayTogether = toneDeltaPair.StayTogether;

            bool aIsNearer =
                polarity == TonePolarity.Nearer ||
                (polarity == TonePolarity.Lighter && !scheme.IsDark) ||
                (polarity == TonePolarity.Darker && scheme.IsDark);

            var nearer = aIsNearer ? roleA : roleB;
            var farther = aIsNearer ? roleB : roleA;
            bool amNearer = color.Name == nearer.Name;
            double expansionDir = scheme.IsDark ? 1 : -1;

            double nTone = nearer.Tone(scheme);
            double fTone = farther.Tone(scheme);

            if (color.Background != null &&
                nearer.ContrastCurve != null &&
                farther.ContrastCurve != null)
            {
                var bg = color.Background(scheme);
                var nCurve = nearer.ContrastCurve(scheme);
                var fCurve = farther.ContrastCurve(scheme);

                if (bg != null && nCurve != null && fCurve != null)
                {
                    double nContrast = nCurve.Get(scheme.ContrastLevel);
                    double fContrast = fCurve.Get(scheme.ContrastLevel);
                    double bgTone = bg.GetTone(scheme);

                    if (Contrast.RatioOfTones(bgTone, nTone) < nContrast)
                        nTone = DynamicColor.ForegroundTone(bgTone, nContrast);
                    if (Contrast.RatioOfTones(bgTone, fTone) < fContrast)
                        fTone = DynamicColor.ForegroundTone(bgTone, fContrast);

                    if (decreasingContrast)
                    {
                        nTone = DynamicColor.ForegroundTone(bgTone, nContrast);
                        fTone = DynamicColor.ForegroundTone(bgTone, fContrast);
                    }
                }
            }

            if ((fTone - nTone) * expansionDir < delta)
            {
                fTone = MathUtils.Clamp(0, 100, nTone + delta * expansionDir);
                if ((fTone - nTone) * expansionDir < delta)
                {
                    nTone = MathUtils.Clamp(0, 100, fTone - delta * expansionDir);
                }
            }

            if (nTone >= 50 && nTone < 60)
            {
                if (expansionDir > 0)
                {
                    nTone = 60;
                    fTone = Math.Max(fTone, nTone + delta * expansionDir);
                }
                else
                {
                    nTone = 49;
                    fTone = Math.Min(fTone, nTone + delta * expansionDir);
                }
            }
            else if (fTone >= 50 && fTone < 60)
            {
                if (stayTogether)
                {
                    if (expansionDir > 0)
                    {
                        nTone = 60;
                        fTone = Math.Max(fTone, nTone + delta * expansionDir);
                    }
                    else
                    {
                        nTone = 49;
                        fTone = Math.Min(fTone, nTone + delta * expansionDir);
                    }
                }
                else
                {
                    fTone = expansionDir > 0 ? 60 : 49;
                }
            }

            return amNearer ? nTone : fTone;
        }
        else
        {
            double answer = color.Tone(scheme);

            if (color.Background?.Invoke(scheme) == null ||
                color.ContrastCurve?.Invoke(scheme) == null)
                return answer;

            double bgTone = color.Background(scheme).GetTone(scheme);
            double desiredRatio = color.ContrastCurve(scheme).Get(scheme.ContrastLevel);

            if (Contrast.RatioOfTones(bgTone, answer) < desiredRatio)
                answer = DynamicColor.ForegroundTone(bgTone, desiredRatio);

            if (decreasingContrast)
                answer = DynamicColor.ForegroundTone(bgTone, desiredRatio);

            if (color.IsBackground && answer >= 50 && answer < 60)
            {
                answer = Contrast.RatioOfTones(49, bgTone) >= desiredRatio ? 49 : 60;
            }

            if (color.SecondBackground?.Invoke(scheme) == null)
                return answer;

            double bg1 = color.Background(scheme).GetTone(scheme);
            double bg2 = color.SecondBackground(scheme).GetTone(scheme);
            double upper = Math.Max(bg1, bg2);
            double lower = Math.Min(bg1, bg2);

            if (Contrast.RatioOfTones(upper, answer) >= desiredRatio &&
                Contrast.RatioOfTones(lower, answer) >= desiredRatio)
                return answer;

            double light = Contrast.Lighter(upper, desiredRatio);
            double dark = Contrast.Darker(lower, desiredRatio);

            bool prefersLight = DynamicColor.TonePrefersLightForeground(bg1) || DynamicColor.TonePrefersLightForeground(bg2);

            if (prefersLight) return light == -1 ? 100 : light;
            if (light == -1 && dark != -1) return dark;
            if (dark == -1 && light != -1) return light;

            return 0;
        }
    }

    #endregion

    #region Scheme Palettes

    public TonalPalette GetPrimaryPalette(Variant variant, Hct sourceColorHct, bool isDark, Platform platform, double contrastLevel)
    {
        return variant switch
        {
            Variant.Content or Variant.Fidelity => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, sourceColorHct.Chroma),
            Variant.FruitSalad => TonalPalette.FromHueAndChroma(MathUtils.SanitizeDegrees(sourceColorHct.Hue - 50.0), 48.0),
            Variant.Monochrome => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            Variant.Neutral => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 12.0),
            Variant.Rainbow => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 48.0),
            Variant.TonalSpot => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 36.0),
            Variant.Expressive => TonalPalette.FromHueAndChroma(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 240), 40.0),
            Variant.Vibrant => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 200.0),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };
    }

    public TonalPalette GetSecondaryPalette(Variant variant, Hct sourceColorHct, bool isDark, Platform platform, double contrastLevel)
    {
        return variant switch
        {
            Variant.Content or Variant.Fidelity =>
                TonalPalette.FromHueAndChroma(
                    sourceColorHct.Hue,
                    Math.Max(sourceColorHct.Chroma - 32.0, sourceColorHct.Chroma * 0.5)
                ),
            Variant.FruitSalad =>
                TonalPalette.FromHueAndChroma(MathUtils.SanitizeDegrees(sourceColorHct.Hue - 50.0), 36.0),
            Variant.Monochrome => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            Variant.Neutral => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 8.0),
            Variant.Rainbow or Variant.TonalSpot => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 16.0),
            Variant.Expressive =>
                TonalPalette.FromHueAndChroma(
                    DynamicScheme.GetRotatedHue(sourceColorHct,
                        new[] { 0.0, 21, 51, 121, 151, 191, 271, 321, 360 },
                        new[] { 45.0, 95, 45, 20, 45, 90, 45, 45, 45 }),
                    24.0),
            Variant.Vibrant =>
                TonalPalette.FromHueAndChroma(
                    DynamicScheme.GetRotatedHue(sourceColorHct,
                        new[] { 0.0, 41, 61, 101, 131, 181, 251, 301, 360 },
                        new[] { 18.0, 15, 10, 12, 15, 18, 15, 12, 12 }),
                    24.0),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };
    }

    public TonalPalette GetTertiaryPalette(Variant variant, Hct sourceColorHct, bool isDark, Platform platform, double contrastLevel)
    {
        return variant switch
        {
            Variant.Content =>
                TonalPalette.Create(DislikeAnalyzer.FixIfDisliked(
                    TemperatureCache.Create(sourceColorHct).GetAnalogousColors(3, 6)[2])),
            Variant.Fidelity =>
                TonalPalette.Create(DislikeAnalyzer.FixIfDisliked(
                    TemperatureCache.Create(sourceColorHct).GetComplement())),
            Variant.FruitSalad => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 36.0),
            Variant.Monochrome => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            Variant.Neutral => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 16.0),
            Variant.Rainbow or Variant.TonalSpot =>
                TonalPalette.FromHueAndChroma(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 60.0), 24.0),
            Variant.Expressive =>
                TonalPalette.FromHueAndChroma(
                    DynamicScheme.GetRotatedHue(sourceColorHct,
                        new[] { 0.0, 21, 51, 121, 151, 191, 271, 321, 360 },
                        new[] { 120.0, 120, 20, 45, 20, 15, 20, 120, 120 }),
                    32.0),
            Variant.Vibrant =>
                TonalPalette.FromHueAndChroma(
                    DynamicScheme.GetRotatedHue(sourceColorHct,
                        new[] { 0.0, 41, 61, 101, 131, 181, 251, 301, 360 },
                        new[] { 35.0, 30, 20, 25, 30, 35, 30, 25, 25 }),
                    32.0),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };
    }

    public TonalPalette GetNeutralPalette(Variant variant, Hct sourceColorHct, bool isDark, Platform platform, double contrastLevel)
    {
        return variant switch
        {
            Variant.Content or Variant.Fidelity => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, sourceColorHct.Chroma / 8.0),
            Variant.FruitSalad => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 10.0),
            Variant.Monochrome => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            Variant.Neutral => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 2.0),
            Variant.Rainbow => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            Variant.TonalSpot => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 6.0),
            Variant.Expressive => TonalPalette.FromHueAndChroma(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 15.0), 8.0),
            Variant.Vibrant => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 10.0),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };
    }

    public TonalPalette GetNeutralVariantPalette(Variant variant, Hct sourceColorHct, bool isDark, Platform platform, double contrastLevel)
    {
        return variant switch
        {
            Variant.Content or Variant.Fidelity =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, sourceColorHct.Chroma / 8.0 + 4.0),
            Variant.FruitSalad => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 16.0),
            Variant.Monochrome => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            Variant.Neutral => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 2.0),
            Variant.Rainbow => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            Variant.TonalSpot => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 8.0),
            Variant.Expressive => TonalPalette.FromHueAndChroma(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 15.0), 12.0),
            Variant.Vibrant => TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 12.0),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };
    }

    public TonalPalette GetErrorPalette(Variant variant, Hct sourceColorHct, bool isDark, Platform platform, double contrastLevel)
    {
        return null;
    }

    #endregion

    #region Helper Functions

    public DynamicColor HighestSurface(DynamicScheme scheme)
    {
        return scheme.IsDark
            ? SurfaceBright
            : SurfaceDim;
    }

    public static bool IsFidelity(DynamicScheme scheme)
    {
        return scheme.Variant == Variant.Fidelity
            || scheme.Variant == Variant.Content;
    }

    public static bool IsMonochrome(DynamicScheme scheme)
    {
        return scheme.Variant == Variant.Monochrome;
    }

    public static double FindDesiredChromaByTone(
        double hue,
        double chroma,
        double tone,
        bool byDecreasingTone
    )
    {
        double answer = tone;

        var closestToChroma = Hct.Create(hue, chroma, tone);
        try
        {
            if (closestToChroma.Chroma < chroma)
            {
                double chromaPeak = closestToChroma.Chroma;
                while (closestToChroma.Chroma < chroma)
                {
                    answer += byDecreasingTone ? -1.0 : 1.0;
                    var potentialSolution = Hct.Create(hue, chroma, answer);
                    if (chromaPeak > potentialSolution.Chroma)
                    {
                        break;
                    }
                    if (Math.Abs(potentialSolution.Chroma - chroma) < 0.4)
                    {
                        break;
                    }

                    double potentialDelta = Math.Abs(potentialSolution.Chroma - chroma);
                    double currentDelta = Math.Abs(closestToChroma.Chroma - chroma);
                    if (potentialDelta < currentDelta)
                    {
                        closestToChroma?.Dispose();
                        closestToChroma = potentialSolution;
                    }
                    chromaPeak = Math.Max(chromaPeak, potentialSolution.Chroma);
                }
            }
        }
        finally
        {
            closestToChroma?.Dispose();
        }

        return answer;
    }

    private DynamicColor Build(Func<DynamicColor.Builder, DynamicColor.Builder> builder)
    {
        var dynamic = new DynamicColor.Builder();
        builder(dynamic);
        return dynamic.Build();
    }

    #endregion
}
