using HizenLabs.Extensions.UserPreference.Material.Structs;

namespace HizenLabs.Extensions.UserPreference.Material.Constants;

/// <summary>
/// Contains color transformation matrices for converting between linear sRGB and CIE XYZ color spaces.
/// </summary>
internal static class ColorTransforms
{
    /// <summary>
    /// 3×3 matrix for converting from linear sRGB to CIE XYZ using a D65 white point.
    /// Each row corresponds to the contribution of R, G, and B to X, Y, and Z respectively.
    /// </summary>
    public static readonly ColorConversionMatrix LinearRgbToCieXyz = new
    (
        0.41233895d, 0.35762064d, 0.18051042d,
        0.2126d, 0.7152d, 0.0722d,
        0.01932141d, 0.11916382d, 0.95034478d
    );

    /// <summary>
    /// 3×3 matrix for converting from CIE XYZ (D65) to linear sRGB.
    /// Each row maps X, Y, and Z to R, G, and B output channels.
    /// </summary>
    public static readonly ColorConversionMatrix CieXyzToLinearRgb = new
    (
        3.2413774792388685, -1.5376652402851851, -0.49885366846268053,
        -0.9691452513005321, 1.8758853451067872, 0.04156585616912061,
        0.05562093689691305, -0.20395524564742123, 1.0571799111220335
    );

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
}
