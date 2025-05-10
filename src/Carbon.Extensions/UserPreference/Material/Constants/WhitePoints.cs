using HizenLabs.Extensions.UserPreference.Material.Structs;

namespace HizenLabs.Extensions.UserPreference.Material.Constants;

/// <summary>
/// Contains constants for color science, specifically the D65 reference white point.
/// </summary>
internal static class WhitePoints
{
    /// <summary>
    /// The D65 reference white point used for many color spaces including sRGB.
    /// </summary>
    public static readonly ColorXyz D65 = new(95.047d, 100.0d, 108.883d);
}
