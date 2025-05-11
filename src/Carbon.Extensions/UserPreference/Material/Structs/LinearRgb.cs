using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// A color in linear RGB color space, where each component is a double in the range [0.0, 100.0].
/// Linear RGB is used in color appearance computations and color space transformations.
/// </summary>
/// <remarks>
/// The components are not gamma-encoded. This format is often used as an intermediate in conversions
/// between sRGB and perceptual color spaces such as XYZ or L*a*b*.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct LinearRgb
{
    /// <summary>
    /// The red component of the color, in the range [0.0, 100.0].
    /// </summary>
    public readonly double R;

    /// <summary>
    /// The green component of the color, in the range [0.0, 100.0].
    /// </summary>
    public readonly double G;

    /// <summary>
    /// The blue component of the color, in the range [0.0, 100.0].
    /// </summary>
    public readonly double B;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearRgb"/> struct with the specified component values.
    /// </summary>
    /// <param name="r">The red component, in the range [0.0, 100.0].</param>
    /// <param name="g">The green component, in the range [0.0, 100.0].</param>
    /// <param name="b">The blue component, in the range [0.0, 100.0].</param>
    public LinearRgb(double r, double g, double b)
    {
        R = r;
        G = g;
        B = b;
    }

    /// <summary>
    /// Converts the LinearRgb instance to an XYZ color space using the linear RGB to XYZ transformation.
    /// </summary>
    /// <returns>A ColorArgb instance representing the color in ARGB format.</returns>
    public CieXyz ToColorXyz()
    {
        var xyz = ColorTransforms.LinearRgbToCieXyz * this;

        return new(xyz.R, xyz.G, xyz.B);
    }

    /// <summary>
    /// Converts the LinearRgb instance to a CAM16 pre-adapted RGB color.
    /// </summary>
    /// <returns>A ColorArgb instance representing the color in ARGB format.</returns>
    public Cam16PreAdaptRgb ToScaledDiscount()
    {
        var sd = ColorTransforms.LinearRgbToCam16ScaledDiscount * this;

        return new(sd.R, sd.G, sd.B);
    }

    /// <summary>
    /// Converts a linear RGB color to a Vector3i representation.
    /// </summary>
    /// <param name="linearRgb">Linear RGB components in range [0, 100].</param>
    /// <returns>A Vector3i representing the linear RGB color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StandardRgb ToStandardRgb()
    {
        return new
        (
            ColorUtils.DelinearizeComponent(R),
            ColorUtils.DelinearizeComponent(G),
            ColorUtils.DelinearizeComponent(B)
        );
    }
}
