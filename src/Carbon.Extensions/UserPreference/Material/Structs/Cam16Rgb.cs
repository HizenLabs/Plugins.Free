using HizenLabs.Extensions.UserPreference.Material.Constants;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a color in the CAM16 RGB color space, used for color appearance modeling.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Cam16Rgb
{
    /// <summary>
    /// The R (red-like) component in the CAM16 RGB space.
    /// </summary>
    public readonly double R;

    /// <summary>
    /// The G (green-like) component in the CAM16 RGB space.
    /// </summary>
    public readonly double G;

    /// <summary>
    /// The B (blue-like) component in the CAM16 RGB space.
    /// </summary>
    public readonly double B;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cam16Rgb"/> struct.
    /// </summary>
    /// <param name="r">The R (red-like) component.</param>
    /// <param name="g">The G (green-like) component.</param>
    /// <param name="b">The B (blue-like) component.</param>
    public Cam16Rgb(double r, double g, double b)
    {
        R = r;
        G = g;
        B = b;
    }

    public static readonly Cam16Rgb DefaultFactor = new(1d, 1d, 1d);

    /// <summary>
    /// Inverts the chromatic adaptation of this CAM16 RGB color to obtain the pre-adapted CAM16 RGB color.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Cam16PreAdaptRgb ToCam16PreAdaptRgb()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts this CAM16 RGB color to CIE XYZ (D65) color space using the inverse CAM16 RGB transformation.
    /// </summary>
    /// <returns>A <see cref="CieXyz"/> representing the equivalent color in the XYZ color space.</returns>
    public CieXyz ToCieXyz()
    {
        var xyz = ColorTransforms.Cam16PreAdaptRgbToCieXyz * this;

        return new(xyz.X, xyz.Y, xyz.Z);
    }

    /// <summary>
    /// Multiplies two CAM16 RGB colors component-wise.
    /// </summary>
    /// <param name="a">The first CAM16 RGB color.</param>
    /// <param name="b">The second CAM16 RGB color.</param>
    /// <returns>A new <see cref="Cam16Rgb"/> instance representing the product of the two colors.</returns>
    public static Cam16Rgb operator *(Cam16Rgb a, Cam16Rgb b)
    {
        return new(a.R * b.R, a.G * b.G, a.B * b.B);
    }

    /// <summary>
    /// Converts a Cam16Rgb instance to a Vector3d instance.
    /// </summary>
    /// <param name="cam16">The Cam16Rgb instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3d(Cam16Rgb cam16)
    {
        return new(cam16.R, cam16.G, cam16.B);
    }

    /// <summary>
    /// Converts a Vector3d instance to a Cam16Rgb instance.
    /// </summary>
    /// <param name="xyz">The Cam16Rgb instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Cam16Rgb(Vector3d xyz)
    {
        return new(xyz.X, xyz.Y, xyz.Z);
    }

    public static Cam16Rgb operator *(Cam16Rgb cam16, Vector3d vector)
    {
        return new
        (
            cam16.R * vector.X,
            cam16.G * vector.Y,
            cam16.B * vector.Z
        );
    }
}
