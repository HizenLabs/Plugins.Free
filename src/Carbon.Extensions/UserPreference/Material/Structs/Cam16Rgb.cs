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

    public static readonly Cam16Rgb DefaultFactor = new(1d, 1d, 1d);

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
    public Cam16Rgb ToChromaticAdaptation(double fl = 1.0, bool signed = true)
    {
        return ToChromaticAdaptation(fl, DefaultFactor);
    }

    /// <inheritdoc cref="ToChromaticAdaptation(double)"/>
    /// <param name="factor">The chromatic adaptation factor.</param>
    public Cam16Rgb ToChromaticAdaptation(double fl, Cam16Rgb factor, bool signed = true)
    {
        double rA = ChromaticAdaptation(R, fl, factor.R, signed);
        double gA = ChromaticAdaptation(G, fl, factor.G, signed);
        double bA = ChromaticAdaptation(B, fl, factor.B, signed);

        return new(rA, gA, bA);
    }

    /// <summary>
    /// Applies chromatic adaptation to a single component of the CAM16 RGB color space.
    /// </summary>
    /// <param name="component">The color component to adapt.</param>
    /// <param name="fl">The luminance-level adaptation factor (FL).</param>
    /// <param name="factor">The chromatic adaptation factor.</param>
    /// <param name="signed">Indicates whether to return a signed value.</param>
    /// <returns>The adapted color component value.</returns>
    private static double ChromaticAdaptation(double component, double fl, double factor, bool signed = true)
    {
        double baseValue = fl * component * factor / 100.0;
        double adapted = Math.Pow(Math.Abs(baseValue), 0.42);
        double result = 400.0 * adapted / (adapted + 27.13);
        return signed ? Math.Sign(baseValue) * result : result;
    }

    /// <summary>
    /// Converts this CAM16 RGB color to its inverse using the inverse CAM16 RGB transformation.
    /// </summary>
    /// <returns>A new <see cref="Cam16Rgb"/> instance representing the inverse of this color.</returns>
    /// <remarks>
    /// This does not perfectly align with <see cref="ChromaticAdaptation(double, double, double, bool)"/>, it's just following the design spec.
    /// </remarks>
    public Cam16Rgb ToChromaticAdaptationInverse()
    {
        double r = ChromaticAdaptationInverse(R);
        double g = ChromaticAdaptationInverse(G);
        double b = ChromaticAdaptationInverse(B);

        return new(r, g, b);
    }

    /// <summary>
    /// Applies the inverse chromatic adaptation to a single component of the CAM16 RGB color space.
    /// </summary>
    /// <param name="adapted">The adapted color component to inverse.</param>
    /// <returns>The inverse adapted color component value.</returns>
    private static double ChromaticAdaptationInverse(double adapted)
    {
        double abs = Math.Abs(adapted);
        double baseVal = Math.Max(0, 27.13 * abs / (400.0 - abs));
        return Math.Sign(adapted) * Math.Pow(baseVal, 1.0 / 0.42);
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
