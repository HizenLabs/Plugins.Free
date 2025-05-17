using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Hct;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using System;
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

    public Cam16PreAdaptRgb(Vector3d vector)
    {
        R = vector.X;
        G = vector.Y;
        B = vector.Z;
    }

    /// <summary>
    /// Converts this pre-adapted CAM16 RGB color to a fully adapted <see cref="Cam16Rgb"/> using the provided viewing conditions.
    /// </summary>
    /// <param name="viewingConditions">
    /// The viewing conditions under which the color is perceived, including degree of adaptation and luminance scaling.
    /// </param>
    /// <returns>
    /// A perceptually adapted <see cref="Cam16Rgb"/> color.
    /// </returns>
    public Cam16Rgb ToCam16Rgb(ViewingConditions viewingConditions)
    {
        return ToCam16Rgb(viewingConditions.RgbD, viewingConditions.Fl);
    }

    /// <summary>
    /// Converts this pre-adapted CAM16 RGB color to a fully adapted <see cref="Cam16Rgb"/> by applying chromatic adaptation,
    /// nonlinear response compression, and sigmoid perceptual scaling.
    /// </summary>
    /// <param name="degreeOfAdaptation">
    /// A scalar representing the degree of adaptation (D), typically in the range [0, 1].
    /// </param>
    /// <param name="fl">
    /// The luminance-level adaptation factor (<c>Fl</c>) from the viewing conditions.
    /// </param>
    /// <param name="adaptedRgb">
    /// Outputs the intermediate chromatically adapted linear RGB color (prior to compression and scaling).
    /// </param>
    /// <returns>
    /// The final perceptually adapted <see cref="Cam16Rgb"/> color.
    /// </returns>
    public Cam16Rgb ToCam16Rgb(double degreeOfAdaptation, double fl, out Cam16Rgb adaptedRgb)
    {
        // Step 1: Chromatic adaptation
        adaptedRgb = ColorUtils.ChromaticAdaptation(this, degreeOfAdaptation);

        return ToCam16Rgb(adaptedRgb, fl);
    }

    /// <summary>
    /// Converts a chromatically adapted CAM16 RGB color to its final perceptual form by applying compression
    /// and post-adaptation scaling.
    /// </summary>
    /// <param name="adaptedRgb">
    /// The chromatically adapted linear RGB color (D-adapted), typically from <see cref="ViewingConditions.RgbD"/>.
    /// </param>
    /// <param name="fl">
    /// The luminance-level adaptation factor (<c>Fl</c>) from the viewing conditions.
    /// </param>
    /// <returns>
    /// A perceptually scaled <see cref="Cam16Rgb"/> color under CAM16.
    /// </returns>
    public Cam16Rgb ToCam16Rgb(Cam16Rgb adaptedRgb, double fl)
    {
        // Step 2: Nonlinear brightness response compression
        Cam16Rgb compressed = ColorUtils.ApplyCompression(this, adaptedRgb, fl);

        // Step 3: Sigmoid scaling to simulate perceptual saturation
        return ColorUtils.PostAdaptationScale(compressed, adaptedRgb);
    }

    /// <summary>
    /// Converts this pre-adapted CAM16 RGB color to CIE XYZ using the
    /// <see cref="ColorTransforms.Cam16PreAdaptRgbToCieXyz"/> matrix.
    /// </summary>
    /// <returns>
    /// A <see cref="CieXyz"/> instance representing the color in CIE XYZ space.
    /// </returns>
    public CieXyz ToCieXyz()
    {
        var xyz = ColorTransforms.Cam16PreAdaptRgbToCieXyz * this;

        return new(xyz);
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

        return new(linrgb);
    }

    public static implicit operator Vector3d(Cam16PreAdaptRgb cam16)
    {
        return new Vector3d(cam16.R, cam16.G, cam16.B);
    }

    public static implicit operator Cam16PreAdaptRgb(Vector3d vector) => new(vector);
}
