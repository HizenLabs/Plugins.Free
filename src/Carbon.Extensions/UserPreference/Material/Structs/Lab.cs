namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a color in the CIE L*a*b* color space.
/// </summary>
internal readonly struct Lab
{
    /// <summary>
    /// The lightness component of the color (0-100).
    /// </summary>
    public readonly double L;
    /// <summary>
    /// The a* component of the color (-128 to 127).
    /// </summary>
    public readonly double A;
    /// <summary>
    /// The b* component of the color (-128 to 127).
    /// </summary>
    public readonly double B;
    /// <summary>
    /// Initializes a new instance of the <see cref="Lab"/> struct with the specified component values.
    /// </summary>
    /// <param name="l">The lightness component (0-100).</param>
    /// <param name="a">The a* component (-128 to 127).</param>
    /// <param name="b">The b* component (-128 to 127).</param>
    public Lab(double l, double a, double b)
    {
        L = l;
        A = a;
        B = b;
    }
}
