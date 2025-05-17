using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Utils;
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

    public CieXyz(Vector3d vector)
    {
        X = vector.X;
        Y = vector.Y;
        Z = vector.Z;
    }

    /// <summary>
    /// Creates a <see cref="CieXyz"/> from 2D chromaticity coordinates (x, y).
    /// Assumes a Y luminance value of 1.0, producing a unit-normalized white point.
    /// </summary>
    /// <param name="x">The x chromaticity coordinate.</param>
    /// <param name="y">The y chromaticity coordinate.</param>
    /// <param name="luminance">The luminance value (default is 1.0).</param>
    /// <returns>A new <see cref="CieXyz"/> with computed XYZ values.</returns>
    public static CieXyz FromChromaticity(double x, double y, double luminance = 1.0)
    {
        return new
        (
            x / y * luminance,
            luminance,
            (1.0 - x - y) / y * luminance
        );
    }

    /// <summary>
    /// Converts the ColorXyz instance to a Cam16Rgb instance using the XYZ to CAM16 RGB transformation.
    /// </summary>
    /// <returns>A Cam16Rgb instance representing the color in CAM16 RGB space.</returns>
    public Cam16PreAdaptRgb ToCam16PreAdaptRgb()
    {
        var cam16 = ColorTransforms.CieXyzToCam16PreAdaptRgb * this;

        return new(cam16);
    }

    public Cam16Rgb ToCam16Rgb(ViewingConditions viewingConditions)
    {
        return ToCam16PreAdaptRgb().ToCam16Rgb(viewingConditions);
    }

    /// <summary>
    /// Converts the ColorXyz instance to a LabFxyz instance using the CIE L*a*b* transformation function
    /// </summary>
    /// <returns>A LabFxyz instance representing the color in L*a*b* space.</returns>
    /// <remarks>
    /// Values created with <see cref="ColorUtils.ApplyLabFunction(double)"/>
    /// </remarks>
    public LabFxyz ToLabFxyz()
    {
        return new
        (
            ColorUtils.ApplyLabFunction(X),
            ColorUtils.ApplyLabFunction(Y),
            ColorUtils.ApplyLabFunction(Z)
        );
    }

    /// <summary>
    /// Converts the ColorXyz instance to a LinearRgb instance using the XYZ to linear RGB transformation.
    /// </summary>
    /// <returns>A LinearRgb instance representing the color in linear RGB space.</returns>
    public LinearRgb ToLinearRgb()
    {
        var rgb = ColorTransforms.CieXyzToLinearRgb * this;

        return new(rgb);
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

    /// <summary>
    /// Multiplies a ColorXyz instance by a scalar value.
    /// </summary>
    /// <param name="a">The ColorXyz instance.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>A new ColorXyz instance representing the product of the color and the scalar.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CieXyz operator *(CieXyz a, double b)
    {
        return new(a.X * b, a.Y * b, a.Z * b);
    }

    /// <summary>
    /// Divides a ColorXyz instance by a scalar value.
    /// </summary>
    /// <param name="a">The ColorXyz instance.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>A new ColorXyz instance representing the quotient of the color and the scalar.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CieXyz operator /(CieXyz a, double b)
    {
        return new(a.X / b, a.Y / b, a.Z / b);
    }

    /// <summary>
    /// Converts a CieXyz instance to a Vector3d instance.
    /// </summary>
    /// <param name="xyz">The CieXyz instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3d(CieXyz xyz)
    {
        return new(xyz.X, xyz.Y, xyz.Z);
    }

    /// <summary>
    /// Converts a Vector3d instance to a CieXyz instance.
    /// </summary>
    /// <param name="xyz">The Vector3d instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator CieXyz(Vector3d xyz) => new(xyz);
}
