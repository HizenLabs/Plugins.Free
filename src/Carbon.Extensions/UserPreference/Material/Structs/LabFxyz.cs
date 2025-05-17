using HizenLabs.Extensions.UserPreference.Material.Constants;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a color in the CIE L*a*b* color space using the intermediate nonlinear components f(x), f(y), and f(z),
/// as defined by the CIE 1976 standard. These values are typically derived from the ratio of XYZ values to the reference white point
/// and passed through the non-linear Lab function.
/// </summary>
/// <remarks>
/// Some additional resources:<br />
/// - <a href="https://en.wikipedia.org/wiki/CIELAB">Wikipedia: CIELAB</a><br />
/// - <a href="http://www.brucelindbloom.com/index.html?Eqn_XYZ_to_Lab.html">Bruce Lindbloom's CIE XYZ to Lab conversion</a>
/// </remarks>
public readonly struct LabFxyz
{
    /// <summary>
    /// The nonlinear transformed X component (f(x)).
    /// </summary>
    public readonly double Fx;

    /// <summary>
    /// The nonlinear transformed Y component (f(y)).
    /// </summary>
    public readonly double Fy;

    /// <summary>
    /// The nonlinear transformed Z component (f(z)).
    /// </summary>
    public readonly double Fz;

    /// <summary>
    /// Initializes a new instance of the <see cref="LabFxyz"/> struct with the specified nonlinear components.
    /// </summary>
    /// <param name="fx">The f(x) component.</param>
    /// <param name="fy">The f(y) component.</param>
    /// <param name="fz">The f(z) component.</param>
    public LabFxyz(double fx, double fy, double fz)
    {
        Fx = fx;
        Fy = fy;
        Fz = fz;
    }

    /// <summary>
    /// Converts this LabFxyz instance to a standard CIE L*a*b* color using the conventional scaling formula.
    /// </summary>
    /// <returns>A <see cref="Lab"/> instance representing the corresponding color in L*a*b* space.</returns>
    /// <remarks>
    /// This conversion follows the definitions:
    /// <list type="bullet">
    /// <item><description>L* = 116 × f(y) − 16</description></item>
    /// <item><description>a* = 500 × (f(x) − f(y))</description></item>
    /// <item><description>b* = 200 × (f(y) − f(z))</description></item>
    /// </list>
    /// These formulas are part of the standard CIE L*a*b* conversion pipeline.
    /// </remarks>
    public Lab ToLab()
    {
        double l = LabConstants.FScale * Fy - LabConstants.FOffset;
        double a = LabConstants.A_Scale * (Fx - Fy);
        double b = LabConstants.B_Scale * (Fy - Fz);

        return new(l, a, b);
    }
}
