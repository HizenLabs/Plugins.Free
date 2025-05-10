using HizenLabs.Extensions.UserPreference.Material.Structs;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HizenLabs.Extensions.UserPreference.Material.Utils;

/// <summary>
/// Utility methods for color science constants and color space conversions that aren't HCT or CAM16.
/// </summary>
public static class ColorUtils
{
    /// <summary>
    /// Threshold used to determine if gamma compression is linear or nonlinear.
    /// </summary>
    public const double GammaThreshold = 0.0031308d;

    /// <summary>
    /// Linear scale multiplier for low-range gamma.
    /// </summary>
    public const double LinearGammaScale = 12.92d;

    /// <summary>
    /// Offset applied after gamma encoding for sRGB.
    /// </summary>
    public const double GammaOffset = 0.055d;

    /// <summary>
    /// Scale factor used in gamma encoding for sRGB.
    /// </summary>
    public const double GammaScale = 1.055d;

    /// <summary>
    /// The exponent used to convert linear RGB to gamma-encoded sRGB (forward gamma).
    /// </summary>
    public const double SrgbGammaEncodingExponent = 1d / 2.4d; // ≈ 0.41666...

    /// <summary>
    /// The exponent used to convert gamma-encoded sRGB to linear RGB (inverse gamma).
    /// </summary>
    public const double SrgbGammaDecodingExponent = 2.4d;

    /// <summary>
    /// Threshold constant (ε) used in CIE L*a*b* conversion.  
    /// Below this value, the function uses the linear portion of the LabF curve.
    /// </summary>
    public const double LabEpsilon = 216d / 24389d; // ≈ 0.008856

    /// <summary>
    /// Scaling constant (κ) used in CIE L*a*b* conversion.  
    /// Applied to linearize values below <see cref="LabEpsilon"/>.
    /// </summary>
    public const double LabKappa = 24389d / 27d; // ≈ 903.296

    /// <summary>
    /// Offset used in Lab to XYZ conversion and grayscale L* calculations.  
    /// Part of the L* scaling formula: (116 * f) - 16.
    /// </summary>
    public const double LabFOffset = 16d;

    /// <summary>
    /// Scale factor for converting f(t) to L*.  
    /// Used in the formula: L = 116 * f - 16.
    /// </summary>
    public const double LabFScale = 116d;

    /// <summary>
    /// Scale factor for computing the a* component: a = 500 × (fx - fy).
    /// </summary>
    public const double LabA_Scale = 500d;

    /// <summary>
    /// Scale factor for computing the b* component: b = 200 × (fy - fz).
    /// </summary>
    public const double LabB_Scale = 200d;

    /// <summary>
    /// Threshold used in sRGB linearization for determining whether to apply the linear or exponential segment.
    /// </summary>
    public const double SrgbLinearThreshold = 0.040449936d;

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

    /// <summary>
    /// The D65 reference white point used for many color spaces including sRGB.
    /// </summary>
    public static readonly ColorXyz WhitePointD65 = new(95.047d, 100.0d, 108.883d);

    /// <summary>
    /// Packs RGB components into a 32-bit ARGB integer.
    /// </summary>
    /// <param name="red">Red component (0–255).</param>
    /// <param name="green">Green component (0–255).</param>
    /// <param name="blue">Blue component (0–255).</param>
    /// <returns>An ARGB integer with full alpha (255).</returns>
    /// <remarks>
    /// ARGB Format: 0xAARRGGBB
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ArgbFromRgb(byte red, byte green, byte blue)
    {
        return (uint)((255 << 24) | (red << 16) | (green << 8) | blue);
    }

    /// <summary>
    /// Converts a color from linear RGB components to ARGB format.
    /// </summary>
    /// <param name="linearRgb">Linear RGB components in range [0, 100].</param>
    /// <returns>An ARGB integer with full alpha (255).</returns>
    public static uint ArgbFromLinearArgb(Vector3 linearArgb)
    {
        var red = Delinearized(linearArgb.x);
        var green = Delinearized(linearArgb.y);
        var blue = Delinearized(linearArgb.z);

        return ArgbFromRgb(red, green, blue);
    }

    /// <summary>
    /// Checks if the color represented by the ARGB integer is opaque.
    /// </summary>
    /// <param name="argb">The ARGB integer.</param>
    /// <returns>True if the color is opaque (alpha = 255), otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOpaque(uint argb)
    {
        return ((argb >> 24) & 255) == 255;
    }

    /// <summary>
    /// Converts a color from XYZ to ARGB.
    /// </summary>
    /// <param name="x">X component.</param>
    /// <param name="y">Y component.</param>
    /// <param name="z">Z component.</param>
    /// <returns>An ARGB integer.</returns>
    public static ColorArgb ArgbFromXyz(double x, double y, double z)
    {
        var colorXyz = new ColorXyz(x, y, z);

        return ArgbFromXyzVector(colorXyz);
    }

    /// <summary>
    /// Converts a color from XYZ to ARGB.
    /// </summary>
    /// <param name="colorXyz">The XYZ vector.</param>
    /// <returns>An ARGB integer.</returns>
    public static ColorArgb ArgbFromXyzVector(ColorXyz colorXyz)
    {
        var linearRgb = XyzToSrgb * colorXyz;
        
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

        return SrgbToXyz * linearRgb;
    }

    /// <summary>
    /// Converts a color from L*a*b* to ARGB.
    /// </summary>
    /// <param name="l">L* component.</param>
    /// <param name="a">a* component.</param>
    /// <param name="b">b* component.</param>
    /// <returns>An ARGB integer.</returns>
    public static ColorArgb ArgbFromLab(double l, double a, double b)
    {
        double fy = (l + LabFOffset) / LabFScale;
        double fx = a / LabA_Scale + fy;
        double fz = fy - b / LabB_Scale;

        double xNormalized = LabInvF(fx);
        double yNormalized = LabInvF(fy);
        double zNormalized = LabInvF(fz);

        double x = xNormalized * WhitePointD65[0];
        double y = yNormalized * WhitePointD65[1];
        double z = zNormalized * WhitePointD65[2];

        return ArgbFromXyz(x, y, z);
    }

    /// <summary>
    /// Converts a color from ARGB to L*a*b* color space.
    /// </summary>
    /// <param name="argb">The ARGB integer.</param>
    /// <returns>An array containing L*, a*, and b* values.</returns>
    public static double[] LabFromArgb(uint argb)
    {
        var colorXyz = XyzFromArgb(argb);

        double xNormalized = colorXyz[0] / WhitePointD65[0];
        double yNormalized = colorXyz[1] / WhitePointD65[1];
        double zNormalized = colorXyz[2] / WhitePointD65[2];

        double fx = LabF(xNormalized);
        double fy = LabF(yNormalized);
        double fz = LabF(zNormalized);

        double l = LabFScale * fy - LabFOffset;
        double a = LabA_Scale * (fx - fy);
        double b = LabB_Scale * (fy - fz);

        return new double[] { l, a, b };
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

        return LabFScale * LabF(y / 100d) - LabFOffset;
    }

    /// <summary>
    /// Converts an L* value to a Y value.
    /// </summary>
    /// <param name="lstar">L* value.</param>
    /// <returns>Y value.</returns>
    public static double YFromLstar(double lstar)
    {
        return 100d * LabInvF((lstar + LabFOffset) / LabFScale);
    }

    /// <summary>
    /// Converts a Y value to an L* value.
    /// </summary>
    /// <param name="y">Y value.</param>
    /// <returns>L* value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LstarFromY(double y)
    {
        return LabF(y / 100d) * LabFScale - LabFOffset;
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

        if (normalized <= SrgbLinearThreshold)
        {
            return normalized / LinearGammaScale * 100d;
        }
        else
        {
            return Math.Pow((normalized + GammaOffset) / GammaScale, SrgbGammaDecodingExponent) * 100d;
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

        double delinearized = normalized <= GammaThreshold
            ? normalized * LinearGammaScale
            : GammaScale * Math.Pow(normalized, SrgbGammaEncodingExponent) - GammaOffset;

        return (byte)Mathf.Clamp((int)Math.Round(delinearized * 255), 0, 255);
    }

    /// <summary>
    /// Helper function for L*a*b* conversions.
    /// </summary>
    private static double LabF(double t)
    {
        if (t > LabEpsilon)
        {
            return Math.Pow(t, 1d / 3d);
        }
        else
        {
            return (LabKappa * t + LabFOffset) / LabFScale;
        }
    }

    /// <summary>
    /// Inverse of the LabF function.
    /// </summary>
    private static double LabInvF(double ft)
    {
        double ft3 = ft * ft * ft;

        if (ft3 > LabEpsilon)
        {
            return ft3;
        }
        else
        {
            return (LabFScale * ft - LabFOffset) / LabKappa;
        }
    }
}
