using HizenLabs.Extensions.UserPreference.Material.Structs;

namespace HizenLabs.Extensions.UserPreference.Material.Constants;

/// <summary>
/// Defines standard white points used in color science, specifically the D65 illuminant in the CIE 1931 2° standard observer.
/// </summary>
internal static class WhitePoints
{
    /// <summary>
    /// The x chromaticity coordinate of the D65 white point.
    /// </summary>
    public const double D65X = 0.31272;

    /// <summary>
    /// The y chromaticity coordinate of the D65 white point.
    /// </summary>
    public const double D65Y = 0.32903;

    /// <summary>
    /// The D65 white point represented in CIE 1931 XYZ tristimulus values, normalized such that Y = 1.0.
    /// </summary>
    public static readonly CieXyz D65 = CieXyz.FromChromaticity(D65X, D65Y, Gamma.LuminanceScale);
}
