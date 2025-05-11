using HizenLabs.Extensions.UserPreference.Material.Structs;

namespace HizenLabs.Extensions.UserPreference.Material.Constants;

/// <summary>
/// Contains color transformation matrices for converting between linear sRGB and CIE XYZ color spaces.
/// </summary>
internal static class ColorTransforms
{
    private const double
        sRgbRx = 0.64, sRgbRy = 0.33,
        sRgbGx = 0.3, sRgbGy = 0.6,
        sRgbBx = 0.15, sRgbBy = 0.06;

    static ColorTransforms()
    {
        LinearRgbToCieXyz = CreateCieXyzToLinearRgbConversionMatrix();

        CieXyzToLinearRgb = LinearRgbToCieXyz.ToInverted();
    }

    /// <summary>
    /// 3×3 matrix for converting from linear sRGB to CIE XYZ using a D65 white point.
    /// Each row corresponds to the contribution of R, G, and B to X, Y, and Z respectively.
    /// </summary>
    public static readonly ColorConversionMatrix LinearRgbToCieXyz;

    /// <summary>
    /// 3×3 matrix for converting from CIE XYZ (D65) to linear sRGB.
    /// Each row maps X, Y, and Z to R, G, and B output channels.
    /// </summary>
    public static readonly ColorConversionMatrix CieXyzToLinearRgb;

    /// <summary>
    /// 3×3 matrix for converting from CIE XYZ to CAM16 RGB.
    /// This matrix applies the forward linear transformation for CAM16 adaptation.
    /// </summary>
    public static readonly ColorConversionMatrix CieXyzToCam16PreAdaptRgb = new
    (
        0.401288, 0.650173, -0.051461,
        -0.250268, 1.204414, 0.045854,
        -0.002079, 0.048952, 0.953127
    );

    /// <summary>
    /// 3×3 matrix for converting from CAM16 RGB back to CIE XYZ.
    /// This is the inverse of <see cref="CieXyzToCam16PreAdaptRgb"/>.
    /// </summary>
    public static readonly ColorConversionMatrix Cam16PreAdaptRgbToCieXyz = new
    (
        1.8620678, -1.0112547, 0.14918678,
        0.38752654, 0.62144744, -0.00897398,
        -0.01584150, -0.03412294, 1.0499644
    );

    /// <summary>
    /// 3×3 matrix used in CAM16 for converting from linear sRGB to scaled and discounted CAM16 RGB values.
    /// Applies both chromatic adaptation and perceptual scaling for appearance modeling.
    /// </summary>
    public static readonly ColorConversionMatrix LinearRgbToCam16ScaledDiscount = new
    (
        0.001200833568784504, 0.002389694492170889, 0.0002795742885861124,
        0.0005891086651375999, 0.0029785502573438758, 0.0003270666104008398,
        0.00010146692491640572, 0.0005364214359186694, 0.0032979401770712076
    );

    /// <summary>
    /// 3×3 matrix used in CAM16 for converting from scaled and discounted CAM16 RGB values back to linear sRGB.
    /// This is the inverse of <see cref="LinearRgbToCam16ScaledDiscount"/>.
    /// </summary>
    public static readonly ColorConversionMatrix Cam16ScaledDiscountToLinearRgb = new
    (
        1373.2198709594231, -1100.4251190754821, -7.278681089101213,
        -271.815969077903, 559.6580465940733, -32.46047482791194,
        1.9622899599665666, -57.173814538844006, 308.7233197812385
    );

    /// <summary>
    /// Creates a conversion matrix from CIE XYZ to linear RGB using the sRGB color space.
    /// </summary>
    /// <returns>A <see cref="ColorConversionMatrix"/> representing the conversion from CIE XYZ to linear RGB.</returns>
    private static ColorConversionMatrix CreateCieXyzToLinearRgbConversionMatrix()
    {
        var r = CieXyz.FromChromaticity(sRgbRx, sRgbRy);
        var g = CieXyz.FromChromaticity(sRgbGx, sRgbGy);
        var b = CieXyz.FromChromaticity(sRgbBx, sRgbBy);

        var m = new ColorConversionMatrix
        (
            r.X, g.X, b.X,
            r.Y, g.Y, b.Y,
            r.Z, g.Z, b.Z
        );

        var d65Normalized = WhitePoints.D65 / Gamma.LuminanceScale;
        var s = m.ToInverted() * d65Normalized;

        var colR = r * s.X;
        var colG = g * s.Y;
        var colB = b * s.Z;

        return new
        (
            colR.X, colG.X, colB.X,
            colR.Y, colG.Y, colB.Y,
            colR.Z, colG.Z, colB.Z
        );
    }
}
