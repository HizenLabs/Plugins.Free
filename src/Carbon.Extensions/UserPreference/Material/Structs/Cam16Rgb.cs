using HizenLabs.Extensions.UserPreference.Material.Constants;
using System;
using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a color in the CAM16 RGB color space, used for color appearance modeling.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Cam16Rgb
{
    /// <summary>
    /// The R (red-like) component in the CAM16 RGB space.
    /// </summary>
    public readonly double R;

    /// <summary>
    /// The G (green-like) component in the CAM16 RGB space.
    /// </summary>
    public readonly double G;

    /// <summary>
    /// The B (blue-like) component in the CAM16 RGB space.
    /// </summary>
    public readonly double B;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cam16Rgb"/> struct.
    /// </summary>
    /// <param name="r">The R (red-like) component.</param>
    /// <param name="g">The G (green-like) component.</param>
    /// <param name="b">The B (blue-like) component.</param>
    public Cam16Rgb(double r, double g, double b)
    {
        R = r;
        G = g;
        B = b;
    }

    /// <summary>
    /// Applies chromatic adaptation to this CAM16 RGB color using the specified luminance-level adaptation factor (FL).
    /// This simulates the human visual system’s nonlinear response to light based on the viewing conditions.
    /// </summary>
    /// <param name="fl">
    /// The luminance-level adaptation factor (<c>FL</c>), typically derived from viewing conditions.
    /// This value affects the degree of chromatic adaptation applied to the input color.
    /// </param>
    /// <returns>
    /// A new <see cref="Cam16Rgb"/> instance representing the chromatically adapted color,
    /// transformed using the CAM16 non-linear response function.
    /// </returns>

    public Cam16Rgb ToChromaticAdaptation(double fl = 1.0)
    {
        double rA = ChromaticAdaptation(R, fl);
        double gA = ChromaticAdaptation(G, fl);
        double bA = ChromaticAdaptation(B, fl);

        return new(rA, gA, bA);
    }

    /// <summary>
    /// Applies chromatic adaptation to a single component of the CAM16 RGB color space.
    /// </summary>
    /// <param name="component">The color component to adapt.</param>
    /// <param name="fl">The luminance-level adaptation factor (FL).</param>
    /// <returns>The adapted color component value.</returns>
    private static double ChromaticAdaptation(double component, double fl)
    {
        double rAF = Math.Pow(fl * Math.Abs(component) / 100.0, 0.42);

        return Math.Sign(component) * 400.0 * rAF / (rAF + 27.13);
    }

    /// <summary>
    /// Converts this CAM16 RGB color to CIE XYZ (D65) color space using the inverse CAM16 RGB transformation.
    /// </summary>
    /// <returns>A <see cref="ColorXyz"/> representing the equivalent color in the XYZ color space.</returns>
    public ColorXyz ToColorXyz()
    {
        var xyz = ColorTransforms.Cam16rgbToXyz * this;
        return new(xyz.R, xyz.G, xyz.B);
    }

    /// <summary>
    /// Multiplies two CAM16 RGB colors component-wise.
    /// </summary>
    /// <param name="a">The first CAM16 RGB color.</param>
    /// <param name="b">The second CAM16 RGB color.</param>
    /// <returns>A new <see cref="Cam16Rgb"/> instance representing the product of the two colors.</returns>
    public static Cam16Rgb operator *(Cam16Rgb a, Cam16Rgb b)
    {
        return new Cam16Rgb(a.R * b.R, a.G * b.G, a.B * b.B);
    }
}
