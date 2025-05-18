using HizenLabs.Extensions.UserPreference.Material.Constants;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a color in the CAM16 scaled discount RGB color space.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly struct Cam16ScaledDiscountRgb
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

    public Cam16ScaledDiscountRgb(Vector3d vector)
    {
        R = vector.X;
        G = vector.Y;
        B = vector.Z;
    }

    /// <summary>
    /// Converts to LinearRgb by applying the inverse of the gamma correction matrix.
    /// </summary>
    public LinearRgb ToLinearRgb()
    {
        var linearRgb = ColorTransforms.Cam16ScaledDiscountToLinearRgb * this;

        return new(linearRgb);
    }

    /// <summary>
    /// Converts a Cam16ScaledDiscountRgb instance to a Vector3d instance.
    /// </summary>
    /// <param name="sd">The Cam16ScaledDiscountRgb instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3d(Cam16ScaledDiscountRgb sd)
    {
        return new(sd.R, sd.G, sd.B);
    }

    /// <summary>
    /// Converts a Vector3d instance to a Cam16ScaledDiscountRgb instance.
    /// </summary>
    /// <param name="xyz">The Vector3d instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Cam16ScaledDiscountRgb(Vector3d xyz) => new(xyz);
}
