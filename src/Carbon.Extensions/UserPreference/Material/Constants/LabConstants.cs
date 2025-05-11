namespace HizenLabs.Extensions.UserPreference.Material.Constants;

/// <summary>
/// Contains constants for color science, specifically for CIE L*a*b* color space.
/// </summary>
internal static class LabConstants
{
    /// <summary>
    /// Threshold constant (ε) used in CIE L*a*b* conversion.  
    /// Below this value, the function uses the linear portion of the LabF curve.
    /// </summary>
    public const double Epsilon = 216d / 24389d; // ≈ 0.008856

    /// <summary>
    /// Scaling constant (κ) used in CIE L*a*b* conversion.  
    /// Applied to linearize values below <see cref="Epsilon"/>.
    /// </summary>
    public const double Kappa = 24389d / 27d; // ≈ 903.296

    /// <summary>
    /// Offset used in Lab to XYZ conversion and grayscale L* calculations.  
    /// Part of the L* scaling formula: (116 * f) - 16.
    /// </summary>
    public const double FOffset = 16d;

    /// <summary>
    /// Scale factor for converting f(t) to L*.  
    /// Used in the formula: L = 116 * f - 16.
    /// </summary>
    public const double FScale = 116d;

    /// <summary>
    /// Scale factor for computing the a* component: a = 500 × (fx - fy).
    /// </summary>
    public const double A_Scale = 500d;

    /// <summary>
    /// Scale factor for computing the b* component: b = 200 × (fy - fz).
    /// </summary>
    public const double B_Scale = 200d;
}
