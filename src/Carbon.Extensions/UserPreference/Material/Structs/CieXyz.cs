using HizenLabs.Extensions.UserPreference.Material.Constants;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a linear color in the XYZ color space, where X, Y, and Z are the red, green, and blue color components ranging from 0 to 1.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct CieXyz
{
    /// <summary>
    /// The X (red) component of the color (0-1).
    /// </summary>
    public readonly double X;

    /// <summary>
    /// The Y (green) component of the color (0-1).
    /// </summary>
    public readonly double Y;

    /// <summary>
    /// The Z (blue) component of the color (0-1).
    /// </summary>
    public readonly double Z;

    /// <summary>
    /// Gets the component of the color at the specified index.
    /// </summary>
    /// <param name="index">The index of the component (0 for X, 1 for Y, 2 for Z).</param>
    /// <returns>The value of the component at the specified index.</returns>
    /// <exception cref="System.IndexOutOfRangeException">Thrown if the index is not 0, 1, or 2.</exception>
    public double this[int index]
    {
        get
        {
            return index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new System.IndexOutOfRangeException("Invalid XYZ index!")
            };
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CieXyz"/> struct with the specified X, Y, and Z values.
    /// </summary>
    /// <param name="x">The X (red) component of the color.</param>
    /// <param name="y">The Y (green) component of the color.</param>
    /// <param name="z">The Z (blue) component of the color.</param>
    public CieXyz(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Converts the ColorXyz instance to a Cam16Rgb instance using the XYZ to CAM16 RGB transformation.
    /// </summary>
    /// <returns>A Cam16Rgb instance representing the color in CAM16 RGB space.</returns>
    public Cam16PreAdaptRgb ToCam16PreAdaptRgb()
    {
        var cam16 = ColorTransforms.CieXyzToCam16PreAdaptRgb * this;

        return new(cam16.X, cam16.Y, cam16.Z);
    }

    /// <summary>
    /// Converts the ColorXyz instance to a LinearRgb instance using the XYZ to linear RGB transformation.
    /// </summary>
    /// <returns>A LinearRgb instance representing the color in linear RGB space.</returns>
    public LinearRgb ToLinearRgb()
    {
        var rgb = ColorTransforms.CieXyzToLinearRgb * this;

        return new(rgb.X, rgb.Y, rgb.Z);
    }

    public StandardRgb ToStandardRgb()
    {
        return ToLinearRgb().ToStandardRgb();
    }

    /// <summary>
    /// Multiplies two ColorXyz instances component-wise.
    /// </summary>
    /// <param name="a">The first ColorXyz instance.</param>
    /// <param name="b">The second ColorXyz instance.</param>
    /// <returns>A new ColorXyz instance representing the product of the two colors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CieXyz operator *(CieXyz a, CieXyz b)
    {
        return new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }

    /// <summary>
    /// Divides two ColorXyz instances component-wise.
    /// </summary>
    /// <param name="a">The first ColorXyz instance.</param>
    /// <param name="b">The second ColorXyz instance.</param>
    /// <returns>A new ColorXyz instance representing the quotient of the two colors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CieXyz operator /(CieXyz a, CieXyz b)
    {
        return new(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    }
}
