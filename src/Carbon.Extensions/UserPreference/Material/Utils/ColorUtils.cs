using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using System;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.UserPreference.Material.Utils;

/// <summary>
/// Utility methods for color science constants and color space conversions that aren't HCT or CAM16.
/// </summary>
public static class ColorUtils
{
    /// <summary>
    /// Converts a color from linear RGB components to ARGB format.
    /// </summary>
    /// <param name="linearRgb">Linear RGB components in range [0, 100].</param>
    /// <returns>An ARGB integer with full alpha (255).</returns>
    [Obsolete("Use ToStandardRgb() instead.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StandardRgb ArgbFromLinearArgb(LinearRgb linearRgb)
    {
        return linearRgb.ToStandardRgb();
    }

    /// <summary>
    /// Converts a color from XYZ to ARGB.
    /// </summary>
    /// <param name="colorXyz">The XYZ vector.</param>
    /// <returns>An ARGB integer.</returns>
    [Obsolete("Use ToStandardRgb() instead.")]
    public static StandardRgb ArgbFromXyz(CieXyz colorXyz)
    {
        return colorXyz.ToStandardRgb();
    }

    /// <summary>
    /// Converts a color from ARGB to XYZ.
    /// </summary>
    /// <param name="color">The ARGB integer.</param>
    /// <returns>A ColorXyz representing the XYZ color.</returns>
    [Obsolete("Use ToColorXyz() instead.")]
    public static CieXyz XyzFromArgb(StandardRgb color)
    {
        var linearRgb = Linearized(color);

        return linearRgb.ToColorXyz();
    }

    /// <summary>
    /// Converts a color from CIE L*a*b* to ARGB.
    /// </summary>
    /// <param name="l">L* (lightness) component of the Lab color.</param>
    /// <param name="a">a* (green–red) component of the Lab color.</param>
    /// <param name="b">b* (blue–yellow) component of the Lab color.</param>
    /// <returns>An ARGB color converted from the Lab input.</returns>
    public static StandardRgb ArgbFromLab(CieXyz lab)
    {
        // Convert L*a*b* to intermediate f(x), f(y), f(z) values
        // These represent cube root–transformed XYZ values, normalized to the white point
        double fy = (lab.X + Constants.LabConstants.FOffset) / Constants.LabConstants.FScale;
        double fx = lab.Y / Constants.LabConstants.A_Scale + fy;
        double fz = fy - lab.Z / Constants.LabConstants.B_Scale;

        // Convert normalized f(x), f(y), f(z) values to relative XYZ
        // (i.e., where white point Y is 1.0)
        CieXyz relativeXyz = new
        (
            LabInvF(fx),
            LabInvF(fy),
            LabInvF(fz)
        );

        // Scale relative XYZ to absolute XYZ using the D65 white point
        // (i.e., where white point Y is 100.0 and XYZ reflects actual luminance)
        CieXyz absoluteXyz = relativeXyz * WhitePoints.D65;

        // Convert absolute XYZ to sRGB (ARGB representation)
        return ArgbFromXyz(absoluteXyz);
    }


    /// <summary>
    /// Converts an ARGB color to the CIE L\*a\*b\* color space.
    /// </summary>
    /// <param name="argb">The ARGB color as an unsigned integer.</param>
    /// <returns>An array containing L\*, a\*, and b\* values.</returns>
    public static CieXyz LabFromArgb(StandardRgb argb)
    {
        // Convert ARGB to absolute XYZ (under D65 white point)
        CieXyz absoluteXyz = XyzFromArgb(argb);

        // Normalize XYZ by dividing by the D65 white point
        CieXyz relativeXyz = absoluteXyz / WhitePoints.D65;

        // Convert normalized XYZ to intermediate f(x), f(y), f(z) values
        CieXyz labF = LabF(relativeXyz);

        // Convert to Lab using standard formulas
        double l = Constants.LabConstants.FScale * labF.Y - Constants.LabConstants.FOffset;
        double a = Constants.LabConstants.A_Scale * (labF.X - labF.Y);
        double b = Constants.LabConstants.B_Scale * (labF.Y - labF.Z);

        return new(l, a, b);
    }


    /// <summary>
    /// Converts an L* value to an ARGB representation (grayscale).
    /// </summary>
    /// <param name="lstar">L* value.</param>
    /// <returns>An ARGB integer.</returns>
    public static StandardRgb ArgbFromLstar(double lstar)
    {
        var y = YFromLstar(lstar);
        var component = Delinearized(y);

        return new(component, component, component);
    }

    /// <summary>
    /// Computes the L* value from an ARGB color.
    /// </summary>
    /// <param name="argb">The ARGB integer.</param>
    /// <returns>The L* value.</returns>
    public static double LstarFromArgb(StandardRgb argb)
    {
        double y = XyzFromArgb(argb)[1];

        return Constants.LabConstants.FScale * LabF(y / 100d) - Constants.LabConstants.FOffset;
    }

    /// <summary>
    /// Converts an L* value to a Y value.
    /// </summary>
    /// <param name="lstar">L* value.</param>
    /// <returns>Y value.</returns>
    public static double YFromLstar(double lstar)
    {
        return 100d * LabInvF((lstar + Constants.LabConstants.FOffset) / Constants.LabConstants.FScale);
    }

    /// <summary>
    /// Converts a Y value to an L* value.
    /// </summary>
    /// <param name="y">Y value.</param>
    /// <returns>L* value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LstarFromY(double y)
    {
        return LabF(y / 100d) * Constants.LabConstants.FScale - Constants.LabConstants.FOffset;
    }

    /// <summary>
    /// Linearizes a ColorArgb color.
    /// </summary>
    /// <param name="color">The RGB components in range [0, 255].</param>
    /// <returns>A ColorXyz representing the linearized RGB color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinearRgb Linearized(StandardRgb color)
    {
        return new
        (
            Linearized(color.R),
            Linearized(color.G),
            Linearized(color.B)
        );
    }

    /// <summary>
    /// Linearizes an RGB component.
    /// </summary>
    /// <param name="rgbComponent">RGB component (0-255).</param>
    /// <returns>Linearized value (0-100).</returns>
    public static double Linearized(byte rgbComponent)
    {
        double normalized = rgbComponent / 255d;

        if (normalized <= Gamma.LinearThreshold)
        {
            return normalized / Gamma.LinearScale * 100d;
        }
        else
        {
            return Math.Pow((normalized + Gamma.Offset) / Gamma.Scale, Gamma.DecodingExponent) * 100d;
        }
    }

    /// <summary>
    /// Applies sRGB gamma encoding to a linear RGB component and returns a byte value.
    /// </summary>
    /// <param name="rgbComponent">Linear RGB component in the range [0, 100].</param>
    /// <returns>Gamma-encoded sRGB value in the range [0, 255].</returns>
    public static byte Delinearized(double rgbComponent)
    {
        return (byte)MathUtils.Clamp(0, 255, (int)Math.Round(DelinearizeCore(rgbComponent) * 255));
    }

    /// <summary>
    /// Applies sRGB gamma encoding to a linear RGB component and returns the full-precision result.
    /// </summary>
    /// <param name="rgbComponent">Linear RGB component in the range [0, 100].</param>
    /// <returns>Gamma-encoded sRGB value in the range [0.0, 255.0].</returns>
    public static double TrueDelinearized(double rgbComponent)
    {
        return DelinearizeCore(rgbComponent) * 255.0;
    }

    /// <summary>
    /// Performs the core sRGB gamma encoding operation on a normalized linear RGB component.
    /// </summary>
    /// <param name="rgbComponent">Linear RGB component in the range [0, 100].</param>
    /// <returns>Normalized gamma-encoded sRGB value in the range [0.0, 1.0].</returns>
    private static double DelinearizeCore(double rgbComponent)
    {
        double normalized = rgbComponent / 100d;
        return normalized <= Gamma.Threshold
            ? normalized * Gamma.LinearScale
            : Gamma.Scale * Math.Pow(normalized, Gamma.EncodingExponent) - Gamma.Offset;
    }


    /// <summary>
    /// Applies the LabF function to the values of a ColorXyz color.
    /// </summary>
    /// <param name="xyz">The ColorXyz color.</param>
    /// <returns>A new ColorXyz with the LabF transformation applied.</returns>
    internal static CieXyz LabF(CieXyz xyz)
    {
        return new
        (
            LabF(xyz.X),
            LabF(xyz.Y),
            LabF(xyz.Z)
        );
    }

    /// <summary>
    /// Helper function for L*a*b* conversions.
    /// </summary>
    internal static double LabF(double t)
    {
        if (t > Constants.LabConstants.Epsilon)
        {
            return Math.Pow(t, 1d / 3d);
        }
        else
        {
            return (Constants.LabConstants.Kappa * t + Constants.LabConstants.FOffset) / Constants.LabConstants.FScale;
        }
    }

    /// <summary>
    /// Inverse of the LabF function.
    /// </summary>
    internal static double LabInvF(double ft)
    {
        double ft3 = ft * ft * ft;

        if (ft3 > Constants.LabConstants.Epsilon)
        {
            return ft3;
        }
        else
        {
            return (Constants.LabConstants.FScale * ft - Constants.LabConstants.FOffset) / Constants.LabConstants.Kappa;
        }
    }
}
