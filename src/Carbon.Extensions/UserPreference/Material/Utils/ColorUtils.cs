﻿using HizenLabs.Extensions.UserPreference.Material.Constants;
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
    public static uint ArgbFromLinearArgb(ColorXyz linearArgb)
    {
        var red = Delinearized(linearArgb.X);
        var green = Delinearized(linearArgb.Y);
        var blue = Delinearized(linearArgb.Z);

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
    /// Converts a color from L*a*b* to ARGB.
    /// </summary>
    /// <param name="l">L* component.</param>
    /// <param name="a">a* component.</param>
    /// <param name="b">b* component.</param>
    /// <returns>An ARGB integer.</returns>
    public static ColorArgb ArgbFromLab(double l, double a, double b)
    {
        double fy = (l + Lab.FOffset) / Lab.FScale;
        double fx = a / Lab.A_Scale + fy;
        double fz = fy - b / Lab.B_Scale;

        double xNormalized = LabInvF(fx);
        double yNormalized = LabInvF(fy);
        double zNormalized = LabInvF(fz);

        double x = xNormalized * WhitePoints.D65[0];
        double y = yNormalized * WhitePoints.D65[1];
        double z = zNormalized * WhitePoints.D65[2];

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

        double xNormalized = colorXyz[0] / WhitePoints.D65[0];
        double yNormalized = colorXyz[1] / WhitePoints.D65[1];
        double zNormalized = colorXyz[2] / WhitePoints.D65[2];

        double fx = LabF(xNormalized);
        double fy = LabF(yNormalized);
        double fz = LabF(zNormalized);

        double l = Lab.FScale * fy - Lab.FOffset;
        double a = Lab.A_Scale * (fx - fy);
        double b = Lab.B_Scale * (fy - fz);

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
    /// Helper function for L*a*b* conversions.
    /// </summary>
    private static double LabF(double t)
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
    private static double LabInvF(double ft)
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
