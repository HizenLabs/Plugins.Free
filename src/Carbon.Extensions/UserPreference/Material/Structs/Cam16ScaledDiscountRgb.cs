using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a color in the CAM16 scaled discount RGB color space.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Cam16ScaledDiscountRgb
{
    /// <summary>
    /// The red component of the color (0-1).
    /// </summary>
    public readonly double R;

    /// <summary>
    /// The green component of the color (0-1).
    /// </summary>
    public readonly double G;

    /// <summary>
    /// The blue component of the color (0-1).
    /// </summary>
    public readonly double B;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cam16ScaledDiscountRgb"/> struct with the specified component values.
    /// </summary>
    /// <param name="r">The red component (0-1).</param>
    /// <param name="g">The green component (0-1).</param>
    /// <param name="b">The blue component (0-1).</param>
    public Cam16ScaledDiscountRgb(double r, double g, double b)
    {
        R = r;
        G = g;
        B = b;
    }

    /// <summary>
    /// Converts to LinearRgb by applying the inverse of the gamma correction matrix.
    /// </summary>
    public void ToLinearRgb()
    {

    }
}
