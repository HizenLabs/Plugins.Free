using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HizenLabs.Extensions.UserPreference.Material.Utils;

public static class MathUtils
{
    [Obsolete("Use Math.Sign instead.")]
    public static int Signum(float value)
    {
        return Math.Sign(value);
    }

    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    /// <param name="start">The starting value.</param>
    /// <param name="stop">The ending value.</param>
    /// <param name="amount">The interpolation factor, typically in the range [0, 1].</param>
    /// <returns>The interpolated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float start, float stop, float amount)
    {
        return (1 - amount) * start + amount * stop;
    }

    [Obsolete("Caution: Parameters are swapped, but convert to Mathf.Clamp.")]
    public static int Clamp(int min, int max, int value)
    {
        return Mathf.Clamp(value, min, max);
    }

    [Obsolete("Caution: Parameters are swapped, but convert to Mathf.Clamp.")]
    public static float Clamp(float min, float max, float value)
    {
        return Mathf.Clamp(value, min, max);
    }

    /// <summary>
    /// Sanitize degrees to be in the range of [0, 360).
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The sanitized angle in degrees.</returns>
    public static int SanitizeDegrees(int degrees)
    {
        degrees %= 360;

        if (degrees < 0)
        {
            return degrees + 360;
        }

        return degrees;
    }

    /// <summary>
    /// Sanitize degrees to be in the range of [0, 360).
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The sanitized angle in degrees.</returns>
    public static float SanitizeDegrees(float degrees)
    {
        degrees %= 360;

        if (degrees < 0)
        {
            return degrees + 360;
        }

        return degrees;
    }
}
