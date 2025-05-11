using System;
using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a white point in the CIE 1931 color space using tristimulus values (X, Y, Z).
/// Typically derived from chromaticity coordinates (x, y) using a Y of 1.0.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct WhitePoint
{
    /// <summary>
    /// The X component of the white point.
    /// </summary>
    public readonly double X;

    /// <summary>
    /// The Y component of the white point. Typically normalized to 1.0.
    /// </summary>
    public readonly double Y;

    /// <summary>
    /// The Z component of the white point.
    /// </summary>
    public readonly double Z;

    [Obsolete($"WhitePoint is formulaic. Use {nameof(FromChromaticity)} instead.", true)]
    public WhitePoint() { }

    private WhitePoint(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Creates a <see cref="WhitePoint"/> from 2D chromaticity coordinates (x, y).
    /// Assumes a Y luminance value of 1.0, producing a unit-normalized white point.
    /// </summary>
    /// <param name="x">The x chromaticity coordinate.</param>
    /// <param name="y">The y chromaticity coordinate.</param>
    /// <returns>A new <see cref="WhitePoint"/> with computed XYZ values.</returns>
    public static WhitePoint FromChromaticity(double x, double y)
    {
        return new
        (
            x / y * 100,
            100.0,
            (1.0 - x - y) / y * 100
        );
    }
}
