using HizenLabs.Extensions.UserPreference.Material.Structs;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
    public const float GammaThreshold = 0.0031308f;

    /// <summary>
    /// Linear scale multiplier for low-range gamma.
    /// </summary>
    public const float LinearGammaScale = 12.92f;

    /// <summary>
    /// Offset applied after gamma encoding for sRGB.
    /// </summary>
    public const float GammaOffset = 0.055f;

    /// <summary>
    /// Scale factor used in gamma encoding for sRGB.
    /// </summary>
    public const float GammaScale = 1.055f;

    /// <summary>
    /// The exponent used to convert linear RGB to gamma-encoded sRGB (forward gamma).
    /// </summary>
    public const float SrgbGammaEncodingExponent = 1f / 2.4f; // ≈ 0.41666...

    /// <summary>
    /// The exponent used to convert gamma-encoded sRGB to linear RGB (inverse gamma).
    /// </summary>
    public const float SrgbGammaDecodingExponent = 2.4f;

    /// <summary>
    /// Threshold constant (ε) used in CIE L*a*b* conversion.  
    /// Below this value, the function uses the linear portion of the LabF curve.
    /// </summary>
    public const float LabEpsilon = 216f / 24389f; // ≈ 0.008856

    /// <summary>
    /// Scaling constant (κ) used in CIE L*a*b* conversion.  
    /// Applied to linearize values below <see cref="LabEpsilon"/>.
    /// </summary>
    public const float LabKappa = 24389f / 27f; // ≈ 903.296

    /// <summary>
    /// Offset used in Lab to XYZ conversion and grayscale L* calculations.  
    /// Part of the L* scaling formula: (116 * f) - 16.
    /// </summary>
    public const float LabFOffset = 16f;

    /// <summary>
    /// Scale factor for converting f(t) to L*.  
    /// Used in the formula: L = 116 * f - 16.
    /// </summary>
    public const float LabFScale = 116f;

    /// <summary>
    /// Scale factor for computing the a* component: a = 500 × (fx - fy).
    /// </summary>
    public const float LabA_Scale = 500f;

    /// <summary>
    /// Scale factor for computing the b* component: b = 200 × (fy - fz).
    /// </summary>
    public const float LabB_Scale = 200f;

    /// <summary>
    /// Threshold used in sRGB linearization for determining whether to apply the linear or exponential segment.
    /// </summary>
    public const float SrgbLinearThreshold = 0.040449936f;

    /// <summary>
    /// 3×3 matrix to convert linear sRGB to CIE XYZ using a D65 white point.
    /// Each row represents X, Y, and Z contributions from RGB inputs.
    /// </summary>
    public static readonly Matrix3x3 SrgbToXyz = new
    (
        0.41233895f, 0.35762064f, 0.18051042f,
        0.2126f, 0.7152f, 0.0722f,
        0.01932141f, 0.11916382f, 0.95034478f
    );

    /// <summary>
    /// 3×3 matrix to convert CIE XYZ (D65) to linear sRGB.
    /// Each row maps X, Y, and Z to R, G, and B respectively.
    /// </summary>
    public static readonly Matrix3x3 XyzToSrgb = new
    (
        3.2413774792388685f, -1.5376652402851851f, -0.49885366846268053f,
        -0.9691452513005321f, 1.8758853451067872f, 0.04156585616912061f,
        0.05562093689691305f, -0.20395524564742123f, 1.0571799111220335f
    );

    /// <summary>
    /// The D65 reference white point used for many color spaces including sRGB.
    /// </summary>
    public static readonly float[] WhitePointD65 = new[] { 95.047f, 100.0f, 108.883f };

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

    [Obsolete("Avoid float[] alloc, use ArgbFromLinearArgb(Vector3) instead.")]
    public static uint ArgbFromLinearRgb(float[] linearRgb)
    {
        return ArgbFromLinearArgb(new(linearRgb[0], linearRgb[1], linearRgb[2]));
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
    /// Extracts the alpha component from an ARGB integer.
    /// </summary>
    /// <param name="argb">The ARGB integer.</param>
    /// <returns>The alpha component (0–255).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AlphaFromArgb(uint argb)
    {
        return (argb >> 24) & 255;
    }

    /// <summary>
    /// Extracts the red component from an ARGB integer.
    /// </summary>
    /// <param name="argb">The ARGB integer.</param>
    /// <returns>The red component (0–255).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint RedFromArgb(uint argb)
    {
        return (argb >> 16) & 255;
    }

    /// <summary>
    /// Extracts the green component from an ARGB integer.
    /// </summary>
    /// <param name="argb">The ARGB integer.</param>
    /// <returns>The green component (0–255).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GreenFromArgb(uint argb)
    {
        return (argb >> 8) & 255;
    }

    /// <summary>
    /// Extracts the blue component from an ARGB integer.
    /// </summary>
    /// <param name="argb">The ARGB integer.</param>
    /// <returns>The blue component (0–255).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint BlueFromArgb(uint argb)
    {
        return argb & 255;
    }

    /// <summary>
    /// Checks if the color represented by the ARGB integer is opaque.
    /// </summary>
    /// <param name="argb">The ARGB integer.</param>
    /// <returns>True if the color is opaque (alpha = 255), otherwise false.</returns>
    public static bool IsOpaque(uint argb)
    {
        var alpha = AlphaFromArgb(argb);

        return alpha >= 255;
    }

    /// <summary>
    /// Converts a color from XYZ to ARGB.
    /// </summary>
    /// <param name="x">X component.</param>
    /// <param name="y">Y component.</param>
    /// <param name="z">Z component.</param>
    /// <returns>An ARGB integer.</returns>
    public static ColorArgb ArgbFromXyz(float x, float y, float z)
    {
        var xyzVector = new Vector3(x, y, z);

        return ArgbFromXyzVector(xyzVector);
    }

    /// <summary>
    /// Converts a color from XYZ to ARGB.
    /// </summary>
    /// <param name="vector">The XYZ vector.</param>
    /// <returns>An ARGB integer.</returns>
    public static ColorArgb ArgbFromXyzVector(Vector3 vector)
    {
        var linearRgb = XyzToSrgb * vector;
        
        return Delinearized(linearRgb);
    }

    /// <summary>
    /// Converts a color from ARGB to XYZ.
    /// </summary>
    /// <param name="argb">The ARGB integer.</param>
    /// <returns>An array containing X, Y, and Z values.</returns>
    [Obsolete("Avoid float[] alloc, use XyzVectorFromArgb(int) instead.")]
    public static float[] XyzFromArgb(uint argb)
    {
        var vector = XyzVectorFromArgb(argb);

        return new[] { vector.x, vector.y, vector.z };
    }

    /// <summary>
    /// Converts a color from ARGB to XYZ.
    /// </summary>
    /// <param name="argb">The ARGB integer.</param>
    /// <returns>A Vector3 representing the XYZ color.</returns>
    public static Vector3 XyzVectorFromArgb(ColorArgb color)
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
    public static ColorArgb ArgbFromLab(float l, float a, float b)
    {
        float fy = (l + LabFOffset) / LabFScale;
        float fx = a / LabA_Scale + fy;
        float fz = fy - b / LabB_Scale;

        float xNormalized = LabInvF(fx);
        float yNormalized = LabInvF(fy);
        float zNormalized = LabInvF(fz);

        float x = xNormalized * WhitePointD65[0];
        float y = yNormalized * WhitePointD65[1];
        float z = zNormalized * WhitePointD65[2];

        return ArgbFromXyz(x, y, z);
    }

    /// <summary>
    /// Converts a color from ARGB to L*a*b* color space.
    /// </summary>
    /// <param name="argb">The ARGB integer.</param>
    /// <returns>An array containing L*, a*, and b* values.</returns>
    public static float[] LabFromArgb(uint argb)
    {
        float[] xyz = XyzFromArgb(argb);

        float xNormalized = xyz[0] / WhitePointD65[0];
        float yNormalized = xyz[1] / WhitePointD65[1];
        float zNormalized = xyz[2] / WhitePointD65[2];

        float fx = LabF(xNormalized);
        float fy = LabF(yNormalized);
        float fz = LabF(zNormalized);

        float l = LabFScale * fy - LabFOffset;
        float a = LabA_Scale * (fx - fy);
        float b = LabB_Scale * (fy - fz);

        return new float[] { l, a, b };
    }

    /// <summary>
    /// Converts an L* value to an ARGB representation (grayscale).
    /// </summary>
    /// <param name="lstar">L* value.</param>
    /// <returns>An ARGB integer.</returns>
    public static ColorArgb ArgbFromLstar(float lstar)
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
    public static float LstarFromArgb(uint argb)
    {
        float y = XyzFromArgb(argb)[1];
        return LabFScale * LabF(y / 100f) - LabFOffset;
    }

    /// <summary>
    /// Converts an L* value to a Y value.
    /// </summary>
    /// <param name="lstar">L* value.</param>
    /// <returns>Y value.</returns>
    public static float YFromLstar(float lstar)
    {
        return 100f * LabInvF((lstar + LabFOffset) / LabFScale);
    }

    /// <summary>
    /// Converts a Y value to an L* value.
    /// </summary>
    /// <param name="y">Y value.</param>
    /// <returns>L* value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LstarFromY(float y)
    {
        return LabF(y / 100f) * LabFScale - LabFOffset;
    }

    /// <summary>
    /// Linearizes a Vector3i color.
    /// </summary>
    /// <param name="color">The RGB components in range [0, 255].</param>
    /// <returns>A Vector3 representing the linearized RGB color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Linearized(ColorArgb color)
    {
        return new Vector3
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
    public static float Linearized(byte rgbComponent)
    {
        float normalized = rgbComponent / 255f;

        if (normalized <= SrgbLinearThreshold)
        {
            return normalized / LinearGammaScale * 100f;
        }
        else
        {
            return Mathf.Pow((normalized + GammaOffset) / GammaScale, SrgbGammaDecodingExponent) * 100f;
        }
    }

    /// <summary>
    /// Converts a linear RGB color to a Vector3i representation.
    /// </summary>
    /// <param name="linearRgb">Linear RGB components in range [0, 100].</param>
    /// <returns>A Vector3i representing the linear RGB color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ColorArgb Delinearized(Vector3 linearRgb)
    {
        return new
        (
            255,
            Delinearized(linearRgb.x),
            Delinearized(linearRgb.y),
            Delinearized(linearRgb.z)
        );
    }

    /// <summary>
    /// Applies sRGB gamma encoding to a linear RGB component.
    /// </summary>
    /// <param name="rgbComponent">Linear RGB component in range [0, 100].</param>
    /// <returns>Delinearized byte value (0–255).</returns>
    public static byte Delinearized(float rgbComponent)
    {
        float normalized = rgbComponent / 100f;

        float delinearized = normalized <= GammaThreshold
            ? normalized * LinearGammaScale
            : GammaScale * Mathf.Pow(normalized, SrgbGammaEncodingExponent) - GammaOffset;

        return (byte)Mathf.Clamp((int)Math.Round(delinearized * 255), 0, 255);
    }

    /// <summary>
    /// Helper function for L*a*b* conversions.
    /// </summary>
    private static float LabF(float t)
    {
        if (t > LabEpsilon)
        {
            return Mathf.Pow(t, 1f / 3f);
        }
        else
        {
            return (LabKappa * t + LabFOffset) / LabFScale;
        }
    }

    /// <summary>
    /// Inverse of the LabF function.
    /// </summary>
    private static float LabInvF(float ft)
    {
        float ft3 = ft * ft * ft;

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
