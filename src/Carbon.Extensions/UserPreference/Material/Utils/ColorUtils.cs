using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using System;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.UserPreference.Material.Utils;

/// <summary>
/// Utility methods for color science constants and color space conversions that aren't HCT or CAM16.
/// </summary>
internal static class ColorUtils
{

    #region Linearization

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

    #endregion

    #region Lab

    /// <summary>
    /// Applies the LabF function to the values of a ColorXyz color.
    /// </summary>
    /// <param name="xyz">The ColorXyz color.</param>
    /// <returns>A new ColorXyz with the LabF transformation applied.</returns>
    public static CieXyz LabF(CieXyz xyz)
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
    public static double LabF(double t)
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
    public static double LabInvF(double ft)
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
        double y = argb.ToCieXyz().Y;

        return LabConstants.FScale * LabF(y / Gamma.LuminanceScale) - LabConstants.FOffset;
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
        return LabF(y / Gamma.LuminanceScale) * LabConstants.FScale - LabConstants.FOffset;
    }

    /// <summary>
    /// Applies the CIE L*a*b* transformation function to a value.
    /// </summary>
    /// <param name="value">The value to transform.</param>
    /// <returns>The transformed value according to the L*a*b* function.</returns>
    /// <remarks>
    /// For values > ε (0.008856...), applies a cube root function.
    /// For values ≤ ε, applies a linear function to avoid numerical issues near zero.
    /// </remarks>
    public static double ApplyLabFunction(double value)
    {
        if (value > LabConstants.Epsilon)
        {
            return Math.Pow(value, 1d / 3d);
        }
        else
        {
            return (LabConstants.Kappa * value + LabConstants.FOffset) / LabConstants.FScale;
        }
    }

    #endregion

    #region Chromatic Adaptation

    /// <summary>
    /// Applies chromatic adaptation to a single component of the CAM16 RGB color space.
    /// </summary>
    /// <param name="component">The color component to adapt.</param>
    /// <param name="fl">The luminance-level adaptation factor (FL).</param>
    /// <param name="factor">The chromatic adaptation factor.</param>
    /// <param name="signed">Indicates whether to return a signed value.</param>
    /// <returns>The adapted color component value.</returns>
    public static double ChromaticAdaptation(double component, double fl, double factor, bool signed = true)
    {
        double baseValue = fl * component * factor / 100.0;
        double adapted = Math.Pow(Math.Abs(baseValue), 0.42);
        double result = 400.0 * adapted / (adapted + 27.13);
        return signed ? Math.Sign(baseValue) * result : result;
    }

    /// <summary>
    /// Applies the inverse chromatic adaptation to a single component of the CAM16 RGB color space.
    /// </summary>
    /// <param name="adapted">The adapted color component to inverse.</param>
    /// <returns>The inverse adapted color component value.</returns>
    public static double ChromaticAdaptationInverse(double adapted)
    {
        double abs = Math.Abs(adapted);
        double baseVal = Math.Max(0, 27.13 * abs / (400.0 - abs));
        return Math.Sign(adapted) * Math.Pow(baseVal, 1.0 / 0.42);
    }

    #endregion
}
