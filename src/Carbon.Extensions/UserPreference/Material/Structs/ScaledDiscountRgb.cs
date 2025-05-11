using HizenLabs.Extensions.UserPreference.Material.Constants;
using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

[StructLayout(LayoutKind.Sequential)]
public readonly struct ScaledDiscountRgb
{
    public readonly double R;
    public readonly double G;
    public readonly double B;

    public ScaledDiscountRgb(double r, double g, double b)
    {
        R = r;
        G = g;
        B = b;
    }

    /// <summary>
    /// Converts this ScaledDiscount RGB color to a Linear RGB color using the <see cref="ColorTransforms.LinearRgbFromScaledDiscount"/> matrix.
    /// </summary>
    /// <returns>A new <see cref="LinearRgb"/> instance representing the converted color.</returns>
    public LinearRgb ToLinearRgb()
    {
        var linrgb = ColorTransforms.LinearRgbFromScaledDiscount * this;

        return new LinearRgb(linrgb.R, linrgb.G, linrgb.B);
    }
}
