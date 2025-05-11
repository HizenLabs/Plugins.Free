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
    /// Converts an L* value to an ARGB representation (grayscale).
    /// </summary>
    /// <param name="lstar">L* value.</param>
    /// <returns>An ARGB integer.</returns>
    public static StandardRgb ArgbFromLstar(double lstar)
    {
        var y = YFromLstar(lstar);
        var component = DelinearizeComponent(y);

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

        return LabConstants.FScale * LabF(y / 100d) - LabConstants.FOffset;
    }

    /// <summary>
    /// Converts an L* value to a Y value.
    /// </summary>
    /// <param name="lstar">L* value.</param>
    /// <returns>Y value.</returns>
    public static double YFromLstar(double lstar)
    {
        return 100d * LabInvF((lstar + LabConstants.FOffset) / LabConstants.FScale);
    }

    /// <summary>
    /// Converts a Y value to an L* value.
    /// </summary>
    /// <param name="y">Y value.</param>
    /// <returns>L* value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LstarFromY(double y)
    {
        return LabF(y / 100d) * LabConstants.FScale - LabConstants.FOffset;
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
            LinearizeComponent(color.R),
            LinearizeComponent(color.G),
            LinearizeComponent(color.B)
        );
    }

    /// <summary>
    /// Converts an sRGB component (0-255) to a linear RGB value (0-1) and then scales it according to the scaling factor (default 100).
    /// </summary>
    /// <param name="rgbComponent">RGB component (0-255).</param>
    /// <returns>Linearized value (0-100).</returns>
    /// <remarks>
    /// A lot of this is defined in the 
    /// <a href="https://cdn.standards.iteh.ai/samples/10795/ae461684569b40bbbb2d9a22b1047f05/IEC-61966-2-1-1999-AMD1-2003.pdf">
    /// IEC 61966-2-1
    /// </a> international standard.
    /// </remarks>
    public static double LinearizeComponent(byte rgbComponent)
    {
        double normalized = rgbComponent / 255d;

        if (normalized <= Gamma.SrgbToLinearThreshold)
        {
            return normalized / Gamma.LinearScale * Gamma.LuminanceScale;
        }

        return Math.Pow((normalized + Gamma.Offset) / Gamma.Scale, Gamma.DecodingExponent) * Gamma.LuminanceScale;
    }

    /// <summary>
    /// Applies sRGB gamma encoding to a linear RGB component and returns a byte value.
    /// </summary>
    /// <param name="rgbComponent">Linear RGB component in the range [0, 100].</param>
    /// <returns>Gamma-encoded sRGB value in the range [0, 255].</returns>
    public static byte DelinearizeComponent(double rgbComponent)
    {
        var delinearized = DelinearizeCore(rgbComponent) * 255;

        return (byte)MathUtils.Clamp(0, 255, (int)Math.Round(delinearized, MidpointRounding.AwayFromZero));
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

        if (normalized <= Gamma.LinearToSrgbThreshold)
        {
            return normalized * Gamma.LinearScale;
        }

        return Gamma.Scale * Math.Pow(normalized, Gamma.EncodingExponent) - Gamma.Offset;
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
        if (t > LabConstants.Epsilon)
        {
            return Math.Pow(t, 1d / 3d);
        }
        else
        {
            return (LabConstants.Kappa * t + LabConstants.FOffset) / LabConstants.FScale;
        }
    }

    /// <summary>
    /// Inverse of the LabF function.
    /// </summary>
    internal static double LabInvF(double ft)
    {
        double ft3 = ft * ft * ft;

        if (ft3 > LabConstants.Epsilon)
        {
            return ft3;
        }
        else
        {
            return (LabConstants.FScale * ft - LabConstants.FOffset) / LabConstants.Kappa;
        }
    }
}
