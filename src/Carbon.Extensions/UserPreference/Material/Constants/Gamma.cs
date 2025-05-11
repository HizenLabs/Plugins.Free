using HizenLabs.Extensions.UserPreference.Material.Structs;

namespace HizenLabs.Extensions.UserPreference.Material.Constants;

/// <summary>
/// Provides constants for gamma correction and encoding used in color space conversions.
/// </summary>
internal static class Gamma
{
    /// <summary>
    /// Threshold used to determine if gamma compression is linear or nonlinear.
    /// </summary>
    public const double LinearToSrgbThreshold = 0.0031308d;

    /// <summary>
    /// Threshold used in sRGB linearization for determining whether to apply the linear or exponential segment.
    /// </summary>
    public const double SrgbToLinearThreshold = 0.040449936d;

    /// <summary>
    /// Linear scale multiplier for low-range gamma.
    /// </summary>
    public const double LinearScale = 12.92d;

    /// <summary>
    /// Offset applied after gamma encoding for sRGB.
    /// </summary>
    public const double Offset = 0.055d;

    /// <summary>
    /// Scale factor used in gamma encoding for sRGB.
    /// </summary>
    public const double Scale = 1d + Offset;

    /// <summary>
    /// The exponent used to convert linear RGB to gamma-encoded sRGB (forward gamma).
    /// </summary>
    public const double EncodingExponent = 1d / DecodingExponent; // ≈ 0.41666...

    /// <summary>
    /// The exponent used to convert gamma-encoded sRGB to linear RGB (inverse gamma).
    /// </summary>
    public const double DecodingExponent = 2.4d;

    /// <summary>
    /// The threshold for the linear segment in sRGB gamma encoding.
    /// </summary>
    public static readonly Vector3d YFromLinearRgb = new(0.2126, 0.7152, 0.0722);

    /// <summary>
    /// Scaling factor used across the entire linear RGB color space.
    /// </summary>
    public const double LuminanceScale = 100d;
}
