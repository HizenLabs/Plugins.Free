using HizenLabs.Extensions.UserPreference.Material.Constants;
using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a CAM16 pre-adaptation RGB color. 
/// This form is produced by applying a linear transformation from XYZ to CAM16 RGB, 
/// and then applying the discounting illuminant factors (D).
/// Chromatic adaptation has not yet been applied to these values.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Cam16PreAdaptRgb
{
    /// <summary>
    /// The red-like component of the pre-adapted CAM16 RGB color.
    /// </summary>
    public readonly double R;

    /// <summary>
    /// The green-like component of the pre-adapted CAM16 RGB color.
    /// </summary>
    public readonly double G;

    /// <summary>
    /// The blue-like component of the pre-adapted CAM16 RGB color.
    /// </summary>
    public readonly double B;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cam16PreAdaptRgb"/> struct with the specified components.
    /// </summary>
    /// <param name="r">The red-like component.</param>
    /// <param name="g">The green-like component.</param>
    /// <param name="b">The blue-like component.</param>
    public Cam16PreAdaptRgb(double r, double g, double b)
    {
        R = r;
        G = g;
        B = b;
    }

    /// <summary>
    /// Converts this pre-adapted CAM16 RGB color to linear RGB using the 
    /// <see cref="ColorTransforms.Cam16ScaledDiscountToLinearRgb"/> matrix.
    /// </summary>
    /// <returns>
    /// A <see cref="LinearRgb"/> instance representing the color in linear RGB space.
    /// </returns>
    public LinearRgb ToLinearRgb()
    {
        var linrgb = ColorTransforms.Cam16ScaledDiscountToLinearRgb * this;
        return new LinearRgb(linrgb.R, linrgb.G, linrgb.B);
    }
}
