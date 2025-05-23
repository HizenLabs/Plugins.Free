using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.DynamicColors;
using HizenLabs.Extensions.UserPreference.Material.Scheme;
using System.Globalization;
using System;
using Newtonsoft.Json;
using HizenLabs.Extensions.UserPreference.Pooling;

namespace HizenLabs.Extensions.UserPreference.Material.API;

/// <summary>
/// Represents a full Material Design color theme generated from a seed color,
/// with support for light/dark mode and multiple contrast levels.
/// </summary>
public class MaterialTheme : IDisposable, ITrackedPooled
{
#if DEBUG
    [JsonIgnore]
    public Guid TrackingId { get; set; }
#endif

    [JsonProperty]
    public MaterialColor SeedColor { get; private set; }

    [JsonProperty]
    public bool IsDarkMode { get; private set; }

    [JsonProperty]
    public MaterialContrast Contrast { get; private set; }

    public static MaterialTheme Default { get; } = CreateFromRgbHex("#769CDF");

    [JsonIgnore]
    public MaterialColor Transparent => _transparent;
    private static readonly MaterialColor _transparent = 0x00000000;

    /// <summary>
    /// Gets the light version of this theme. If already light, returns this.
    /// </summary>
    [JsonIgnore]
    public MaterialTheme Light => IsDarkMode ? Create(SeedColor, false, Contrast, this) : this;

    /// <summary>
    /// Gets the dark version of this theme. If already dark, returns this.
    /// </summary>
    [JsonIgnore]
    public MaterialTheme Dark => IsDarkMode ? this : Create(SeedColor, true, Contrast, this);

    /// <summary>
    /// Gets the standard contrast version of this theme.
    /// </summary>
    [JsonIgnore]
    public MaterialTheme StandardContrast => Contrast == MaterialContrast.Standard ? this : Create(SeedColor, IsDarkMode, MaterialContrast.Standard, this);

    /// <summary>
    /// Gets the medium contrast version of this theme.
    /// </summary>
    [JsonIgnore]
    public MaterialTheme MediumContrast => Contrast == MaterialContrast.Medium ? this : Create(SeedColor, IsDarkMode, MaterialContrast.Medium, this);

    /// <summary>
    /// Gets the high contrast version of this theme.
    /// </summary>
    [JsonIgnore]
    public MaterialTheme HighContrast => Contrast == MaterialContrast.High ? this : Create(SeedColor, IsDarkMode, MaterialContrast.High, this);

    [JsonProperty]
    public MaterialColor Primary { get; private set; }
    // [JsonProperty]
    // public MaterialColor PrimaryDim { get; private set; }
    [JsonProperty]
    public MaterialColor OnPrimary { get; private set; }
    [JsonProperty]
    public MaterialColor PrimaryContainer { get; private set; }
    [JsonProperty]
    public MaterialColor OnPrimaryContainer { get; private set; }
    [JsonProperty]
    public MaterialColor InversePrimary { get; private set; }

    [JsonProperty]
    public MaterialColor Secondary { get; private set; }
    // [JsonProperty]
    // public MaterialColor SecondaryDim { get; private set; }
    [JsonProperty]
    public MaterialColor OnSecondary { get; private set; }
    [JsonProperty]
    public MaterialColor SecondaryContainer { get; private set; }
    [JsonProperty]
    public MaterialColor OnSecondaryContainer { get; private set; }

    [JsonProperty]
    public MaterialColor Tertiary { get; private set; }
    // [JsonProperty]
    // public MaterialColor TertiaryDim { get; private set; }
    [JsonProperty]
    public MaterialColor OnTertiary { get; private set; }
    [JsonProperty]
    public MaterialColor TertiaryContainer { get; private set; }
    [JsonProperty]
    public MaterialColor OnTertiaryContainer { get; private set; }

    [JsonProperty]
    public MaterialColor Background { get; private set; }
    [JsonProperty]
    public MaterialColor OnBackground { get; private set; }
    [JsonProperty]
    public MaterialColor Surface { get; private set; }
    // [JsonProperty]
    // public MaterialColor SurfaceDim { get; private set; }
    [JsonProperty]
    public MaterialColor SurfaceBright { get; private set; }
    [JsonProperty]
    public MaterialColor SurfaceContainerLowest { get; private set; }
    [JsonProperty]
    public MaterialColor SurfaceContainerLow { get; private set; }
    [JsonProperty]
    public MaterialColor SurfaceContainer { get; private set; }
    [JsonProperty]
    public MaterialColor SurfaceContainerHigh { get; private set; }
    [JsonProperty]
    public MaterialColor SurfaceContainerHighest { get; private set; }
    [JsonProperty]
    public MaterialColor OnSurface { get; private set; }
    [JsonProperty]
    public MaterialColor SurfaceVariant { get; private set; }
    [JsonProperty]
    public MaterialColor OnSurfaceVariant { get; private set; }
    [JsonProperty]
    public MaterialColor InverseSurface { get; private set; }
    [JsonProperty]
    public MaterialColor InverseOnSurface { get; private set; }
    [JsonProperty]
    public MaterialColor Outline { get; private set; }
    [JsonProperty]
    public MaterialColor OutlineVariant { get; private set; }
    [JsonProperty]
    public MaterialColor Shadow { get; private set; }
    [JsonProperty]
    public MaterialColor Scrim { get; private set; }
    [JsonProperty]
    public MaterialColor SurfaceTint { get; private set; }

    [JsonProperty]
    public MaterialColor Error { get; private set; }
    // [JsonProperty]
    // public MaterialColor ErrorDim { get; private set; }
    [JsonProperty]
    public MaterialColor OnError { get; private set; }
    [JsonProperty]
    public MaterialColor ErrorContainer { get; private set; }
    [JsonProperty]
    public MaterialColor OnErrorContainer { get; private set; }

    [Obsolete("Use Create() instead.", true)]
    public MaterialTheme() { }

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
        var seedColor= GetSeedColorFromRgbaHex(seedColorHex);

        return Create(seedColor, isDarkMode, contrast);
    }

    public static uint GetSeedColorFromRgbaHex(string seedColorHex)
    {
        if (string.IsNullOrEmpty(seedColorHex))
        {
            throw new ArgumentException("Seed color cannot be null or empty.", nameof(seedColorHex));
        }

        var argbHex = seedColorHex.TrimStart('#');
        if (argbHex.Length < 6)
        {
            throw new ArgumentException("Seed color must be in format: RRGGBB or RRGGBBAA, with or without #.", nameof(seedColorHex));
        }

        if (!uint.TryParse("FF" + argbHex[..6], NumberStyles.HexNumber, null, out uint seedColor))
        {
            throw new ArgumentException("Invalid seed color format. Use hexadecimal format (e.g., '#FF5733').", nameof(seedColorHex));
        }

        return seedColor;
    }

    /// <summary>
    /// Creates a new <see cref="MaterialTheme"/> from a packed ARGB seed color.
    /// </summary>
    /// <param name="seedColor">The 32-bit ARGB seed color.</param>
    /// <param name="isDarkMode">Whether the theme should be generated in dark mode.</param>
    /// <param name="contrast">The desired contrast level.</param>
    /// <returns>A fully constructed <see cref="MaterialTheme"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the contrast value is invalid.</exception>
    public static MaterialTheme Create(uint seedColor, bool isDarkMode = false, MaterialContrast contrast = MaterialContrast.Standard, MaterialTheme existing = null)
    {
        // we used to want to dispose, but now that we are caching, we want the system to handle that.
        // existing?.Dispose();

        var theme = TrackedPool.Get<MaterialTheme>();

        theme.SeedColor = seedColor;
        theme.IsDarkMode = isDarkMode;
        theme.Contrast = contrast;

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

        theme.Primary = spec.Primary.GetColor(scheme).Value;
        // theme.PrimaryDim = spec.PrimaryDim.GetColor(scheme).Value;
        theme.OnPrimary = spec.OnPrimary.GetColor(scheme).Value;
        theme.PrimaryContainer = spec.PrimaryContainer.GetColor(scheme).Value;
        theme.OnPrimaryContainer = spec.OnPrimaryContainer.GetColor(scheme).Value;
        theme.InversePrimary = spec.InversePrimary.GetColor(scheme).Value;

        theme.Secondary = spec.Secondary.GetColor(scheme).Value;
        // theme.SecondaryDim = spec.SecondaryDim.GetColor(scheme).Value;
        theme.OnSecondary = spec.OnSecondary.GetColor(scheme).Value;
        theme.SecondaryContainer = spec.SecondaryContainer.GetColor(scheme).Value;
        theme.OnSecondaryContainer = spec.OnSecondaryContainer.GetColor(scheme).Value;

        theme.Tertiary = spec.Tertiary.GetColor(scheme).Value;
        // theme.TertiaryDim = spec.TertiaryDim.GetColor(scheme).Value;
        theme.OnTertiary = spec.OnTertiary.GetColor(scheme).Value;
        theme.TertiaryContainer = spec.TertiaryContainer.GetColor(scheme).Value;
        theme.OnTertiaryContainer = spec.OnTertiaryContainer.GetColor(scheme).Value;

        theme.Background = spec.Background.GetColor(scheme).Value;
        theme.OnBackground = spec.OnBackground.GetColor(scheme).Value;
        theme.Surface = spec.Surface.GetColor(scheme).Value;
        // theme.SurfaceDim = spec.SurfaceDim.GetColor(scheme).Value;
        theme.SurfaceBright = spec.SurfaceBright.GetColor(scheme).Value;
        theme.SurfaceContainerLowest = spec.SurfaceContainerLowest.GetColor(scheme).Value;
        theme.SurfaceContainerLow = spec.SurfaceContainerLow.GetColor(scheme).Value;
        theme.SurfaceContainer = spec.SurfaceContainer.GetColor(scheme).Value;
        theme.SurfaceContainerHigh = spec.SurfaceContainerHigh.GetColor(scheme).Value;
        theme.SurfaceContainerHighest = spec.SurfaceContainerHighest.GetColor(scheme).Value;
        theme.OnSurface = spec.OnSurface.GetColor(scheme).Value;
        theme.SurfaceVariant = spec.SurfaceVariant.GetColor(scheme).Value;
        theme.OnSurfaceVariant = spec.OnSurfaceVariant.GetColor(scheme).Value;
        theme.InverseSurface = spec.InverseSurface.GetColor(scheme).Value;
        theme.InverseOnSurface = spec.InverseOnSurface.GetColor(scheme).Value;

        theme.Outline = spec.Outline.GetColor(scheme).Value;
        theme.OutlineVariant = spec.OutlineVariant.GetColor(scheme).Value;
        theme.Shadow = spec.Shadow.GetColor(scheme).Value;
        theme.Scrim = spec.Scrim.GetColor(scheme).Value;
        theme.SurfaceTint = spec.SurfaceTint.GetColor(scheme).Value;

        theme.Error = spec.Error.GetColor(scheme).Value;
        // theme.ErrorDim = spec.ErrorDim.GetColor(scheme).Value;
        theme.OnError = spec.OnError.GetColor(scheme).Value;
        theme.ErrorContainer = spec.ErrorContainer.GetColor(scheme).Value;
        theme.OnErrorContainer = spec.OnErrorContainer.GetColor(scheme).Value;

        return theme;
    }

    public void Dispose()
    {
        if (ReferenceEquals(this, Default))
        {
            // Please do not dispose of the default theme
            return;
        }

        var obj = this;
        TrackedPool.Free(ref obj);
    }

    public void EnterPool()
    {
        SeedColor = default;
        IsDarkMode = default;
        Contrast = default;

        Primary = 0;
        // PrimaryDim = 0;
        OnPrimary = 0;
        PrimaryContainer = 0;
        OnPrimaryContainer = 0;
        InversePrimary = 0;
        Secondary = 0;
        // SecondaryDim = 0;
        OnSecondary = 0;
        SecondaryContainer = 0;
        OnSecondaryContainer = 0;
        Tertiary = 0;
        // TertiaryDim = 0;
        OnTertiary = 0;
        TertiaryContainer = 0;
        OnTertiaryContainer = 0;
        Background = 0;
        OnBackground = 0;
        Surface = 0;
        // SurfaceDim = 0;
        SurfaceBright = 0;
        SurfaceContainerLowest = 0;
        SurfaceContainerLow = 0;
        SurfaceContainer = 0;
        SurfaceContainerHigh = 0;
        SurfaceContainerHighest = 0;
        OnSurface = 0;
        SurfaceVariant = 0;
        OnSurfaceVariant = 0;
        InverseSurface = 0;
        InverseOnSurface = 0;
        Outline = 0;
        OutlineVariant = 0;
        Shadow = 0;
        Scrim = 0;
        SurfaceTint = 0;
        Error = 0;
        // ErrorDim = errorDim
    }

    public void LeavePool() { }
}
