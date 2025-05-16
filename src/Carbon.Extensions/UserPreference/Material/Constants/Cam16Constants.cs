namespace HizenLabs.Extensions.UserPreference.Material.Constants;

/// <summary>
/// Constants used in the nonlinear response compression and adaptation steps of the CAM16 color appearance model.
/// </summary>
public static class Cam16Constants
{
    /// <summary>
    /// Exponent used in the nonlinear compression of cone responses.
    /// Models the human eye’s contrast sensitivity to different luminance levels.
    /// </summary>
    public const double NonlinearResponseExponent = 0.42;

    /// <summary>
    /// Maximum output value of the adapted nonlinear response (rA, gA, bA).
    /// Scales the perceptual response to fit within a range that approximates human vision.
    /// </summary>
    public const double MaxAdaptedResponse = 400.0;

    /// <summary>
    /// Constant added to the nonlinear response denominator.
    /// Shapes the compression curve to reflect perceptual saturation behavior.
    /// </summary>
    public const double AdaptedResponseOffset = 27.13;
}
