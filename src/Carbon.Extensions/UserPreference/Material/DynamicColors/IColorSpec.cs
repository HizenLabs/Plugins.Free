﻿using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Enums;
using HizenLabs.Extensions.UserPreference.Material.Palettes;

namespace HizenLabs.Extensions.UserPreference.Material.DynamicColors;

internal interface IColorSpec
{
    // Palette key colors
    DynamicColor PrimaryPaletteKeyColor { get; }
    DynamicColor SecondaryPaletteKeyColor { get; }
    DynamicColor TertiaryPaletteKeyColor { get; }
    DynamicColor NeutralPaletteKeyColor { get; }
    DynamicColor NeutralVariantPaletteKeyColor { get; }
    DynamicColor ErrorPaletteKeyColor { get; }

    // Surfaces
    DynamicColor Background { get; }
    DynamicColor OnBackground { get; }
    DynamicColor Surface { get; }
    DynamicColor SurfaceDim { get; }
    DynamicColor SurfaceBright { get; }
    DynamicColor SurfaceContainerLowest { get; }
    DynamicColor SurfaceContainerLow { get; }
    DynamicColor SurfaceContainer { get; }
    DynamicColor SurfaceContainerHigh { get; }
    DynamicColor SurfaceContainerHighest { get; }
    DynamicColor OnSurface { get; }
    DynamicColor SurfaceVariant { get; }
    DynamicColor OnSurfaceVariant { get; }
    DynamicColor InverseSurface { get; }
    DynamicColor InverseOnSurface { get; }
    DynamicColor Outline { get; }
    DynamicColor OutlineVariant { get; }
    DynamicColor Shadow { get; }
    DynamicColor Scrim { get; }
    DynamicColor SurfaceTint { get; }

    // Primary
    DynamicColor Primary { get; }
    DynamicColor PrimaryDim { get; }
    DynamicColor OnPrimary { get; }
    DynamicColor PrimaryContainer { get; }
    DynamicColor OnPrimaryContainer { get; }
    DynamicColor InversePrimary { get; }

    // Secondary
    DynamicColor Secondary { get; }
    DynamicColor SecondaryDim { get; }
    DynamicColor OnSecondary { get; }
    DynamicColor SecondaryContainer { get; }
    DynamicColor OnSecondaryContainer { get; }

    // Tertiary
    DynamicColor Tertiary { get; }
    DynamicColor TertiaryDim { get; }
    DynamicColor OnTertiary { get; }
    DynamicColor TertiaryContainer { get; }
    DynamicColor OnTertiaryContainer { get; }

    // Error
    DynamicColor Error { get; }
    DynamicColor ErrorDim { get; }
    DynamicColor OnError { get; }
    DynamicColor ErrorContainer { get; }
    DynamicColor OnErrorContainer { get; }

    // Fixed Primary
    DynamicColor PrimaryFixed { get; }
    DynamicColor PrimaryFixedDim { get; }
    DynamicColor OnPrimaryFixed { get; }
    DynamicColor OnPrimaryFixedVariant { get; }

    // Fixed Secondary
    DynamicColor SecondaryFixed { get; }
    DynamicColor SecondaryFixedDim { get; }
    DynamicColor OnSecondaryFixed { get; }
    DynamicColor OnSecondaryFixedVariant { get; }

    // Fixed Tertiary
    DynamicColor TertiaryFixed { get; }
    DynamicColor TertiaryFixedDim { get; }
    DynamicColor OnTertiaryFixed { get; }
    DynamicColor OnTertiaryFixedVariant { get; }

    // Android-only
    DynamicColor ControlActivated { get; }
    DynamicColor ControlNormal { get; }
    DynamicColor ControlHighlight { get; }
    DynamicColor TextPrimaryInverse { get; }
    DynamicColor TextSecondaryAndTertiaryInverse { get; }
    DynamicColor TextPrimaryInverseDisableOnly { get; }
    DynamicColor TextSecondaryAndTertiaryInverseDisabled { get; }
    DynamicColor TextHintInverse { get; }

    // Derived
    DynamicColor HighestSurface(DynamicScheme scheme);

    // Calculations
    Hct GetHct(DynamicScheme scheme, DynamicColor color);
    double GetTone(DynamicScheme scheme, DynamicColor color);

    // Palettes
    TonalPalette GetPrimaryPalette(Variant variant, Hct sourceColor, bool isDark, Platform platform, double contrastLevel);
    TonalPalette GetSecondaryPalette(Variant variant, Hct sourceColor, bool isDark, Platform platform, double contrastLevel);
    TonalPalette GetTertiaryPalette(Variant variant, Hct sourceColor, bool isDark, Platform platform, double contrastLevel);
    TonalPalette GetNeutralPalette(Variant variant, Hct sourceColor, bool isDark, Platform platform, double contrastLevel);
    TonalPalette GetNeutralVariantPalette(Variant variant, Hct sourceColor, bool isDark, Platform platform, double contrastLevel);
    TonalPalette GetErrorPalette(Variant variant, Hct sourceColor, bool isDark, Platform platform, double contrastLevel);
}
