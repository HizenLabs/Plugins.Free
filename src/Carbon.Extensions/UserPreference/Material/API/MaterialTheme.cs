using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.DynamicColors;
using HizenLabs.Extensions.UserPreference.Material.Scheme;
using System.Globalization;
using System;
using Newtonsoft.Json;

namespace HizenLabs.Extensions.UserPreference.Material.API;

/// <summary>
/// Represents a full Material Design color theme generated from a seed color,
/// with support for light/dark mode and multiple contrast levels.
/// </summary>
public readonly struct MaterialTheme
{
    private readonly uint _seedColor;
    private readonly bool _isDarkMode;
    private readonly MaterialContrast _contrast;

    public MaterialColor SeedColor => _seedColor;

    public bool IsDarkMode => _isDarkMode;

    public MaterialContrast Contrast => _contrast;

    public static MaterialTheme Default { get; } = CreateFromRgbHex("#769CDF");

    [JsonIgnore]
    public MaterialColor Transparent { get; } = 0x00000000;

    /// <summary>
    /// Gets the light version of this theme. If already light, returns this.
    /// </summary>
    [JsonIgnore]
    public readonly MaterialTheme Light => _isDarkMode ? Create(_seedColor, false, _contrast) : this;

    /// <summary>
    /// Gets the dark version of this theme. If already dark, returns this.
    /// </summary>
    [JsonIgnore]
    public readonly MaterialTheme Dark => _isDarkMode ? this : Create(_seedColor, true, _contrast);

    /// <summary>
    /// Gets the standard contrast version of this theme.
    /// </summary>
    [JsonIgnore]
    public readonly MaterialTheme StandardContrast => _contrast == MaterialContrast.Standard ? this : Create(_seedColor, _isDarkMode, MaterialContrast.Standard);

    /// <summary>
    /// Gets the medium contrast version of this theme.
    /// </summary>
    [JsonIgnore]
    public readonly MaterialTheme MediumContrast => _contrast == MaterialContrast.Medium ? this : Create(_seedColor, _isDarkMode, MaterialContrast.Medium);

    /// <summary>
    /// Gets the high contrast version of this theme.
    /// </summary>
    [JsonIgnore]
    public readonly MaterialTheme HighContrast => _contrast == MaterialContrast.High ? this : Create(_seedColor, _isDarkMode, MaterialContrast.High);

    public required MaterialColor Primary { get; init; }
    // public required MaterialColor PrimaryDim { get; init; }
    public required MaterialColor OnPrimary { get; init; }
    public required MaterialColor PrimaryContainer { get; init; }
    public required MaterialColor OnPrimaryContainer { get; init; }
    public required MaterialColor InversePrimary { get; init; }

    public required MaterialColor Secondary { get; init; }
    // public required MaterialColor SecondaryDim { get; init; }
    public required MaterialColor OnSecondary { get; init; }
    public required MaterialColor SecondaryContainer { get; init; }
    public required MaterialColor OnSecondaryContainer { get; init; }

    public required MaterialColor Tertiary { get; init; }
    // public required MaterialColor TertiaryDim { get; init; }
    public required MaterialColor OnTertiary { get; init; }
    public required MaterialColor TertiaryContainer { get; init; }
    public required MaterialColor OnTertiaryContainer { get; init; }

    public required MaterialColor Background { get; init; }
    public required MaterialColor OnBackground { get; init; }
    public required MaterialColor Surface { get; init; }
    // public required MaterialColor SurfaceDim { get; init; }
    public required MaterialColor SurfaceBright { get; init; }
    public required MaterialColor SurfaceContainerLowest { get; init; }
    public required MaterialColor SurfaceContainerLow { get; init; }
    public required MaterialColor SurfaceContainer { get; init; }
    public required MaterialColor SurfaceContainerHigh { get; init; }
    public required MaterialColor SurfaceContainerHighest { get; init; }
    public required MaterialColor OnSurface { get; init; }
    public required MaterialColor SurfaceVariant { get; init; }
    public required MaterialColor OnSurfaceVariant { get; init; }
    public required MaterialColor InverseSurface { get; init; }
    public required MaterialColor InverseOnSurface { get; init; }
    public required MaterialColor Outline { get; init; }
    public required MaterialColor OutlineVariant { get; init; }
    public required MaterialColor Shadow { get; init; }
    public required MaterialColor Scrim { get; init; }
    public required MaterialColor SurfaceTint { get; init; }

    public required MaterialColor Error { get; init; }
    // public required MaterialColor ErrorDim { get; init; }
    public required MaterialColor OnError { get; init; }
    public required MaterialColor ErrorContainer { get; init; }
    public required MaterialColor OnErrorContainer { get; init; }

    private MaterialTheme(uint seedColor, bool isDarkMode, MaterialContrast contrast)
    {
        _seedColor = seedColor;
        _isDarkMode = isDarkMode;
        _contrast = contrast;
    }

    /// <summary>
    /// Creates a new <see cref="MaterialTheme"/> from an RGB triplet.
    /// </summary>
    /// <param name="r">Red value (0–255).</param>
    /// <param name="g">Green value (0–255).</param>
    /// <param name="b">Blue value (0–255).</param>
    /// <param name="isDarkMode">Whether the theme should be generated in dark mode.</param>
    /// <param name="contrast">The desired contrast level.</param>
    /// <returns>A fully constructed <see cref="MaterialTheme"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a color component is out of range.</exception>
    public static MaterialTheme Create(byte r, byte g, byte b, bool isDarkMode = false, MaterialContrast contrast = MaterialContrast.Standard)
    {
        var seedColor = (uint)(255 << 24 | (r << 16) | (g << 8) | b);

        return Create(seedColor, isDarkMode, contrast);
    }

    public static MaterialTheme CreateFromRgbaHex(string seedColorHex, bool isDarkMode = false, MaterialContrast contrast = MaterialContrast.Standard)
    {
        return CreateFromRgbHex(seedColorHex, isDarkMode, contrast);
    }

    /// <summary>
    /// Creates a new <see cref="MaterialTheme"/> from a hexadecimal string (e.g., "#FF5733").
    /// </summary>
    /// <param name="seedColorHex">The seed color in hex format, with or without the leading '#'.</param>
    /// <param name="isDarkMode">Whether the theme should be generated in dark mode.</param>
    /// <param name="contrast">The desired contrast level.</param>
    /// <returns>A fully constructed <see cref="MaterialTheme"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the format is invalid or empty.</exception>
    public static MaterialTheme CreateFromRgbHex(string seedColorHex, bool isDarkMode = false, MaterialContrast contrast = MaterialContrast.Standard)
    {
        if (string.IsNullOrEmpty(seedColorHex))
        {
            throw new ArgumentException("Seed color cannot be null or empty.", nameof(seedColorHex));
        }

        var argbHex = seedColorHex.TrimStart('#');
        if (argbHex.Length < 6)
        {
            throw new ArgumentException("Seed color must be in format: RRGGBB, with or without #.", nameof(seedColorHex));
        }

        if (!uint.TryParse(argbHex[..6], NumberStyles.HexNumber, null, out uint seedColor))
        {
            throw new ArgumentException("Invalid seed color format. Use hexadecimal format (e.g., '#FF5733').", nameof(seedColorHex));
        }

        return Create(seedColor, isDarkMode, contrast);
    }

    /// <summary>
    /// Creates a new <see cref="MaterialTheme"/> from a packed ARGB seed color.
    /// </summary>
    /// <param name="seedColor">The 32-bit ARGB seed color.</param>
    /// <param name="isDarkMode">Whether the theme should be generated in dark mode.</param>
    /// <param name="contrast">The desired contrast level.</param>
    /// <returns>A fully constructed <see cref="MaterialTheme"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the contrast value is invalid.</exception>
    public static MaterialTheme Create(uint seedColor, bool isDarkMode = false, MaterialContrast contrast = MaterialContrast.Standard)
    {
        var contrastLevel = contrast switch
        {
            MaterialContrast.Standard => 0.0,
            MaterialContrast.Medium => 0.5,
            MaterialContrast.High => 1.0,
            _ => throw new ArgumentOutOfRangeException(nameof(contrast), "Invalid contrast level.")
        };

        var hct = Hct.Create(seedColor);
        using var scheme = SchemeTonalSpot.Create(hct, isDarkMode, contrastLevel);
        var spec = ColorSpecs.Get(scheme.SpecVersion);

        var primary = spec.Primary.GetColor(scheme).Value;
        // var primaryDim = spec.PrimaryDim.GetColor(scheme).Value;
        var onPrimary = spec.OnPrimary.GetColor(scheme).Value;
        var primaryContainer = spec.PrimaryContainer.GetColor(scheme).Value;
        var onPrimaryContainer = spec.OnPrimaryContainer.GetColor(scheme).Value;
        var inversePrimary = spec.InversePrimary.GetColor(scheme).Value;

        var secondary = spec.Secondary.GetColor(scheme).Value;
        // var secondaryDim = spec.SecondaryDim.GetColor(scheme).Value;
        var onSecondary = spec.OnSecondary.GetColor(scheme).Value;
        var secondaryContainer = spec.SecondaryContainer.GetColor(scheme).Value;
        var onSecondaryContainer = spec.OnSecondaryContainer.GetColor(scheme).Value;

        var tertiary = spec.Tertiary.GetColor(scheme).Value;
        // var tertiaryDim = spec.TertiaryDim.GetColor(scheme).Value;
        var onTertiary = spec.OnTertiary.GetColor(scheme).Value;
        var tertiaryContainer = spec.TertiaryContainer.GetColor(scheme).Value;
        var onTertiaryContainer = spec.OnTertiaryContainer.GetColor(scheme).Value;

        var background = spec.Background.GetColor(scheme).Value;
        var onBackground = spec.OnBackground.GetColor(scheme).Value;
        var surface = spec.Surface.GetColor(scheme).Value;
        // var surfaceDim = spec.SurfaceDim.GetColor(scheme).Value;
        var surfaceBright = spec.SurfaceBright.GetColor(scheme).Value;
        var surfaceContainerLowest = spec.SurfaceContainerLowest.GetColor(scheme).Value;
        var surfaceContainerLow = spec.SurfaceContainerLow.GetColor(scheme).Value;
        var surfaceContainer = spec.SurfaceContainer.GetColor(scheme).Value;
        var surfaceContainerHigh = spec.SurfaceContainerHigh.GetColor(scheme).Value;
        var surfaceContainerHighest = spec.SurfaceContainerHighest.GetColor(scheme).Value;
        var onSurface = spec.OnSurface.GetColor(scheme).Value;
        var surfaceVariant = spec.SurfaceVariant.GetColor(scheme).Value;
        var onSurfaceVariant = spec.OnSurfaceVariant.GetColor(scheme).Value;
        var inverseSurface = spec.InverseSurface.GetColor(scheme).Value;
        var inverseOnSurface = spec.InverseOnSurface.GetColor(scheme).Value;

        var outline = spec.Outline.GetColor(scheme).Value;
        var outlineVariant = spec.OutlineVariant.GetColor(scheme).Value;
        var shadow = spec.Shadow.GetColor(scheme).Value;
        var scrim = spec.Scrim.GetColor(scheme).Value;
        var surfaceTint = spec.SurfaceTint.GetColor(scheme).Value;

        var error = spec.Error.GetColor(scheme).Value;
        // var errorDim = spec.ErrorDim.GetColor(scheme).Value;
        var onError = spec.OnError.GetColor(scheme).Value;
        var errorContainer = spec.ErrorContainer.GetColor(scheme).Value;
        var onErrorContainer = spec.OnErrorContainer.GetColor(scheme).Value;

        return new MaterialTheme(seedColor, isDarkMode, contrast)
        {
            Primary = primary,
            // PrimaryDim = primaryDim,
            OnPrimary = onPrimary,
            PrimaryContainer = primaryContainer,
            OnPrimaryContainer = onPrimaryContainer,
            InversePrimary = inversePrimary,

            Secondary = secondary,
            // SecondaryDim = secondaryDim,
            OnSecondary = onSecondary,
            SecondaryContainer = secondaryContainer,
            OnSecondaryContainer = onSecondaryContainer,

            Tertiary = tertiary,
            // TertiaryDim = tertiaryDim,
            OnTertiary = onTertiary,
            TertiaryContainer = tertiaryContainer,
            OnTertiaryContainer = onTertiaryContainer,

            Background = background,
            OnBackground = onBackground,
            Surface = surface,
            // SurfaceDim = surfaceDim,
            SurfaceBright = surfaceBright,
            SurfaceContainerLowest = surfaceContainerLowest,
            SurfaceContainerLow = surfaceContainerLow,
            SurfaceContainer = surfaceContainer,
            SurfaceContainerHigh = surfaceContainerHigh,
            SurfaceContainerHighest = surfaceContainerHighest,
            OnSurface = onSurface,
            SurfaceVariant = surfaceVariant,
            OnSurfaceVariant = onSurfaceVariant,
            InverseSurface = inverseSurface,
            InverseOnSurface = inverseOnSurface,

            Outline = outline,
            OutlineVariant = outlineVariant,
            Shadow = shadow,
            Scrim = scrim,
            SurfaceTint = surfaceTint,

            Error = error,
            // ErrorDim = errorDim,
            OnError = onError,
            ErrorContainer = errorContainer,
            OnErrorContainer = onErrorContainer
        };

    }
}
