using HizenLabs.Extensions.UserPreference.Material.Structs;

namespace HizenLabs.Extensions.UserPreference.Material.Constants;

/// <summary>
/// Contains color transformation matrices for converting between linear sRGB and CIE XYZ color spaces.
/// </summary>
internal static class ColorTransforms
{
    /// <summary>
    /// 3×3 matrix to convert linear sRGB to CIE XYZ using a D65 white point.
    /// Each row represents X, Y, and Z contributions from RGB inputs.
    /// </summary>
    public static readonly ColorConversionMatrix SrgbToXyz = new
    (
        0.41233895d, 0.35762064d, 0.18051042d,
        0.2126d, 0.7152d, 0.0722d,
        0.01932141d, 0.11916382d, 0.95034478d
    );

    /// <summary>
    /// 3×3 matrix to convert CIE XYZ (D65) to linear sRGB.
    /// Each row maps X, Y, and Z to R, G, and B respectively.
    /// </summary>
    public static readonly ColorConversionMatrix XyzToSrgb = new
    (
        3.2413774792388685d, -1.5376652402851851d, -0.49885366846268053d,
        -0.9691452513005321d, 1.8758853451067872d, 0.04156585616912061d,
        0.05562093689691305d, -0.20395524564742123d, 1.0571799111220335d
    );
}
