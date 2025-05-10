using System;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.UserPreference.Material.Utils;

public static class MathUtils
{
    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    /// <param name="start">The starting value.</param>
    /// <param name="stop">The ending value.</param>
    /// <param name="amount">The interpolation factor, typically in the range [0, 1].</param>
    /// <returns>The interpolated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Lerp(double start, double stop, double amount)
    {
        return (1 - amount) * start + amount * stop;
    }

    /// <summary>
    /// Clamps a value between a minimum and maximum range.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="input">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public static int Clamp(int min, int max, int input)
    {
        if (input < min)
        {
            return min;
        }
        else if (input > max)
        {
            return max;
        }

        return input;
    }

    /// <summary>
    /// Clamps a value between a minimum and maximum range.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="input">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public static double Clamp(double min, double max, double input)
    {
        if (input < min)
        {
            return min;
        }
        else if (input > max)
        {
            return max;
        }

        return input;
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
    public static double SanitizeDegrees(double degrees)
    {
        degrees %= 360d;

        if (degrees < 0d)
        {
            return degrees + 360d;
        }

        return degrees;
    }

    /// <summary>
    /// Calculates the direction of rotation between two angles.
    /// </summary>
    /// <param name="from">The starting angle in degrees.</param>
    /// <param name="to">The ending angle in degrees.</param>
    /// <returns>The direction of rotation: 1 for clockwise, -1 for counterclockwise.</returns>
    public static double RotationDirection(double from, double to)
    {
        double increasingDifference = SanitizeDegrees(to - from);

        return increasingDifference <= 180.0 ? 1.0 : -1.0;
    }

    /// <summary>
    /// Calculates the absolute difference between two angles in degrees.
    /// </summary>
    /// <param name="a">The first angle in degrees.</param>
    /// <param name="b">The second angle in degrees.</param>
    /// <returns>The absolute difference between the two angles in degrees.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DifferenceDegrees(double a, double b)
    {
        return 180.0 - Math.Abs(Math.Abs(a - b) - 180.0);
    }

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    /// <param name="radians">The angle in radians.</param>
    /// <returns>The angle in degrees.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToDegrees(double radians)
    {
        return radians * (180.0 / Math.PI);
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The angle in radians.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180.0);
    }

    /// <summary>
    /// Calculates the hypotenuse of a right triangle given its two sides.
    /// </summary>
    /// <param name="x">The length of one side.</param>
    /// <param name="y">The length of the other side.</param>
    /// <returns>The length of the hypotenuse.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Hypotenuse(double x, double y)
    {
        return Math.Sqrt(x * x + y * y);
    }
}
