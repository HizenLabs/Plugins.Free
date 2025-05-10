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
    /// Sanitize degrees to be in the range of [0, 360].
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
    /// Sanitize degrees to be in the range of [0, 360].
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The sanitized angle in degrees.</returns>
    public static float SanitizeDegrees(float degrees)
    {
        degrees %= 360f;

        if (degrees < 0f)
        {
            return degrees + 360f;
        }

        return degrees;
    }

    /// <summary>
    /// Calculates the direction of rotation between two angles.
    /// </summary>
    /// <param name="from">The starting angle in degrees.</param>
    /// <param name="to">The ending angle in degrees.</param>
    /// <returns>The direction of rotation: 1 for clockwise, -1 for counterclockwise.</returns>
    public static float RotationDirection(float from, float to)
    {
        return Mathf.DeltaAngle(from, to) >= 0f ? 1f : -1f;
    }

    /// <summary>
    /// Calculates the absolute difference between two angles in degrees.
    /// </summary>
    /// <param name="a">The first angle in degrees.</param>
    /// <param name="b">The second angle in degrees.</param>
    /// <returns>The absolute difference between the two angles in degrees.</returns>
    public static float DifferenceDegrees(float a, float b)
    {
        var delta = Mathf.DeltaAngle(a, b);

        return Math.Abs(delta);
    }

    [Obsolete("Array allocations, see if we can create custom matrices.")]
    public static float[] MatrixMultiply(float[] row, float[][] matrix)
    {
        var a = row[0] * matrix[0][0] + row[1] * matrix[0][1] + row[2] * matrix[0][2];
        var b = row[0] * matrix[1][0] + row[1] * matrix[1][1] + row[2] * matrix[1][2];
        var c = row[0] * matrix[2][0] + row[1] * matrix[2][1] + row[2] * matrix[2][2];
        return new float[] { a, b, c };
    }
}
