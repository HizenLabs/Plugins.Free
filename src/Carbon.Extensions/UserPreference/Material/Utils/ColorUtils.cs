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
    /// <param name="colorXyz">Linear RGB components in range [0, 100].</param>
    /// <returns>An ARGB integer with full alpha (255).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ColorArgb ArgbFromLinearArgb(ColorXyz colorXyz)
    {
        return Delinearized(colorXyz);
    }

    /// <summary>
    /// Converts a color from XYZ to ARGB.
    /// </summary>
    /// <param name="colorXyz">The XYZ vector.</param>
    /// <returns>An ARGB integer.</returns>
    public static ColorArgb ArgbFromXyz(ColorXyz colorXyz)
    {
        var linearRgb = ColorTransforms.XyzToSrgb * colorXyz;
        
        return Delinearized(linearRgb);
    }

    /// <summary>
    /// Converts a color from ARGB to XYZ.
    /// </summary>
    /// <param name="color">The ARGB integer.</param>
    /// <returns>A ColorXyz representing the XYZ color.</returns>
    public static ColorXyz XyzFromArgb(ColorArgb color)
    {
        var linearRgb = Linearized(color);

        return ColorTransforms.SrgbToXyz * linearRgb;
    }

    /// <summary>
    /// Converts a color from CIE L*a*b* to ARGB.
    /// </summary>
    /// <param name="l">L* (lightness) component of the Lab color.</param>
    /// <param name="a">a* (green–red) component of the Lab color.</param>
    /// <param name="b">b* (blue–yellow) component of the Lab color.</param>
    /// <returns>An ARGB color converted from the Lab input.</returns>
    public static ColorArgb ArgbFromLab(ColorXyz lab)
    {
        // Convert L*a*b* to intermediate f(x), f(y), f(z) values
        // These represent cube root–transformed XYZ values, normalized to the white point
        double fy = (lab.X + Lab.FOffset) / Lab.FScale;
        double fx = lab.Y / Lab.A_Scale + fy;
        double fz = fy - lab.Z / Lab.B_Scale;

        // Convert normalized f(x), f(y), f(z) values to relative XYZ
        // (i.e., where white point Y is 1.0)
        ColorXyz relativeXyz = new
        (
            LabInvF(fx),
            LabInvF(fy),
            LabInvF(fz)
        );

        // Scale relative XYZ to absolute XYZ using the D65 white point
        // (i.e., where white point Y is 100.0 and XYZ reflects actual luminance)
        ColorXyz absoluteXyz = relativeXyz * WhitePoints.D65;

        // Convert absolute XYZ to sRGB (ARGB representation)
        return ArgbFromXyz(absoluteXyz);
    }


    /// <summary>
    /// Converts an ARGB color to the CIE L\*a\*b\* color space.
    /// </summary>
    /// <param name="argb">The ARGB color as an unsigned integer.</param>
    /// <returns>An array containing L\*, a\*, and b\* values.</returns>
    public static ColorXyz LabFromArgb(uint argb)
    {
        // Convert ARGB to absolute XYZ (under D65 white point)
        ColorXyz absoluteXyz = XyzFromArgb(argb);

        // Normalize XYZ by dividing by the D65 white point
        ColorXyz relativeXyz = absoluteXyz / WhitePoints.D65;

        // Convert normalized XYZ to intermediate f(x), f(y), f(z) values
        ColorXyz labF = LabF(relativeXyz);

        // Convert to Lab using standard formulas
        double l = Lab.FScale * labF.Y - Lab.FOffset;
        double a = Lab.A_Scale * (labF.X - labF.Y);
        double b = Lab.B_Scale * (labF.Y - labF.Z);

        return new(l, a, b);
    }


    /// <summary>
    /// Converts an L* value to an ARGB representation (grayscale).
    /// </summary>
    /// <param name="lstar">L* value.</param>
    /// <returns>An ARGB integer.</returns>
    public static ColorArgb ArgbFromLstar(double lstar)
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
    public static double LstarFromArgb(uint argb)
    {
        double y = XyzFromArgb(argb)[1];

        return Lab.FScale * LabF(y / 100d) - Lab.FOffset;
    }

    /// <summary>
    /// Converts an L* value to a Y value.
    /// </summary>
    /// <param name="lstar">L* value.</param>
    /// <returns>Y value.</returns>
    public static double YFromLstar(double lstar)
    {
        return 100d * LabInvF((lstar + Lab.FOffset) / Lab.FScale);
    }

    /// <summary>
    /// Converts a Y value to an L* value.
    /// </summary>
    /// <param name="y">Y value.</param>
    /// <returns>L* value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LstarFromY(double y)
    {
        return LabF(y / 100d) * Lab.FScale - Lab.FOffset;
    }

    /// <summary>
    /// Linearizes a ColorArgb color.
    /// </summary>
    /// <param name="color">The RGB components in range [0, 255].</param>
    /// <returns>A ColorXyz representing the linearized RGB color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ColorXyz Linearized(ColorArgb color)
    {
        return new ColorXyz
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
    /// Converts a linear RGB color to a Vector3i representation.
    /// </summary>
    /// <param name="linearRgb">Linear RGB components in range [0, 100].</param>
    /// <returns>A Vector3i representing the linear RGB color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ColorArgb Delinearized(ColorXyz linearRgb)
    {
        return new
        (
            Delinearized(linearRgb.X),
            Delinearized(linearRgb.Y),
            Delinearized(linearRgb.Z)
        );
    }

    /// <summary>
    /// Applies sRGB gamma encoding to a linear RGB component.
    /// </summary>
    /// <param name="rgbComponent">Linear RGB component in range [0, 100].</param>
    /// <returns>Delinearized byte value (0–255).</returns>
    public static byte Delinearized(double rgbComponent)
    {
        double normalized = rgbComponent / 100d;

        double delinearized = normalized <= Gamma.Threshold
            ? normalized * Gamma.LinearScale
            : Gamma.Scale * Math.Pow(normalized, Gamma.EncodingExponent) - Gamma.Offset;

        return (byte)MathUtils.Clamp(0, 255, (int)Math.Round(delinearized * 255));
    }

    /// <summary>
    /// Applies the LabF function to the values of a ColorXyz color.
    /// </summary>
    /// <param name="xyz">The ColorXyz color.</param>
    /// <returns>A new ColorXyz with the LabF transformation applied.</returns>
    internal static ColorXyz LabF(ColorXyz xyz)
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
        if (t > Lab.Epsilon)
        {
            return Math.Pow(t, 1d / 3d);
        }
        else
        {
            return (Lab.Kappa * t + Lab.FOffset) / Lab.FScale;
        }
    }

    /// <summary>
    /// Inverse of the LabF function.
    /// </summary>
    internal static double LabInvF(double ft)
    {
        double ft3 = ft * ft * ft;

        if (ft3 > Lab.Epsilon)
        {
            return ft3;
        }
        else
        {
            return (Lab.FScale * ft - Lab.FOffset) / Lab.Kappa;
        }
    }
}
