using System;

namespace HizenLabs.Extensions.UserPreference.Material.Utils;

/// <summary>
/// Color science for contrast utilities.
/// </summary>
internal static class Contrast
{
    public const double RatioMin = 1.0;
    public const double RatioMax = 21.0;
    public const double Ratio30 = 3.0;
    public const double Ratio45 = 4.5;
    public const double Ratio70 = 7.0;

    public const double ContrastRatioEpsilon = 0.04;
    public const double LuminanceGamutMapTolerance = 0.4;

    /// <summary>
    /// Contrast ratio using relative luminance Y.
    /// </summary>
    public static double RatioOfYs(double y1, double y2)
    {
        var lighter = Math.Max(y1, y2);
        var darker = (lighter == y2) ? y1 : y2;
        return (lighter + 5.0) / (darker + 5.0);
    }

    /// <summary>
    /// Contrast ratio of two tones (perceptual luminance).
    /// </summary>
    public static double RatioOfTones(double t1, double t2)
    {
        return RatioOfYs(ColorUtils.YFromLstar(t1), ColorUtils.YFromLstar(t2));
    }

    /// <summary>
    /// Tone ≥ input that meets ratio. Returns -1 if not possible.
    /// </summary>
    public static double Lighter(double tone, double ratio)
    {
        if (tone < 0.0 || tone > 100.0)
            return -1.0;

        double darkY = ColorUtils.YFromLstar(tone);
        double lightY = ratio * (darkY + 5.0) - 5.0;
        if (lightY < 0.0 || lightY > 100.0)
            return -1.0;

        double realContrast = RatioOfYs(lightY, darkY);
        if (realContrast < ratio && Math.Abs(realContrast - ratio) > ContrastRatioEpsilon)
            return -1.0;

        double result = ColorUtils.LstarFromY(lightY) + LuminanceGamutMapTolerance;
        return (result < 0 || result > 100) ? -1.0 : result;
    }

    /// <summary>
    /// Unsafe lighter: clamps to 100.
    /// </summary>
    public static double LighterUnsafe(double tone, double ratio)
    {
        var result = Lighter(tone, ratio);
        return result < 0.0 ? 100.0 : result;
    }

    /// <summary>
    /// Tone ≤ input that meets ratio. Returns -1 if not possible.
    /// </summary>
    public static double Darker(double tone, double ratio)
    {
        if (tone < 0.0 || tone > 100.0)
            return -1.0;

        double lightY = ColorUtils.YFromLstar(tone);
        double darkY = (lightY + 5.0) / ratio - 5.0;
        if (darkY < 0.0 || darkY > 100.0)
            return -1.0;

        double realContrast = RatioOfYs(lightY, darkY);
        if (realContrast < ratio && Math.Abs(realContrast - ratio) > ContrastRatioEpsilon)
            return -1.0;

        double result = ColorUtils.LstarFromY(darkY) - LuminanceGamutMapTolerance;
        return (result < 0 || result > 100) ? -1.0 : result;
    }

    /// <summary>
    /// Unsafe darker: clamps to 0.
    /// </summary>
    public static double DarkerUnsafe(double tone, double ratio)
    {
        var result = Darker(tone, ratio);
        return Math.Max(0.0, result);
    }
}
