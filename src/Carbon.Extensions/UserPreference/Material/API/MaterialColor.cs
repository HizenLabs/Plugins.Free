using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.UserPreference.Material.API;

/// <summary>
/// Represents a UI-facing color value in ARGB format, with convenience accessors
/// for individual channels and normalized [0,1] float components for UI display.
/// </summary>
public readonly struct MaterialColor
{
    /// <summary>
    /// Gets the packed ARGB value as a 32-bit unsigned integer.
    /// </summary>
    public readonly uint Value { get; }

    /// <summary>
    /// Gets the alpha (opacity) channel component (0–255).
    /// </summary>
    public byte Alpha => (byte)((Value >> 24) & 0xFF);

    /// <summary>
    /// Gets the red channel component (0–255).
    /// </summary>
    public byte Red => (byte)((Value >> 16) & 0xFF);

    /// <summary>
    /// Gets the green channel component (0–255).
    /// </summary>
    public byte Green => (byte)((Value >> 8) & 0xFF);

    /// <summary>
    /// Gets the blue channel component (0–255).
    /// </summary>
    public byte Blue => (byte)(Value & 0xFF);

    /// <summary>
    /// Gets the normalized alpha (opacity) component in the range [0, 1].
    /// </summary>
    public float AlphaF => Normalize(Alpha);

    /// <summary>
    /// Gets the normalized red component in the range [0, 1].
    /// </summary>
    public float RedF => Normalize(Red);

    /// <summary>
    /// Gets the normalized green component in the range [0, 1].
    /// </summary>
    public float GreenF => Normalize(Green);

    /// <summary>
    /// Gets the normalized blue component in the range [0, 1].
    /// </summary>
    public float BlueF => Normalize(Blue);

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialColor"/> struct with the specified ARGB value.
    /// </summary>
    /// <param name="value">The 32-bit ARGB color value.</param>
    public MaterialColor(uint value)
    {
        Value = value;
    }
    
    public string ToRgbHex()
    {
        return $"#{Red:X2}{Green:X2}{Blue:X2}";
    }

    /// <summary>
    /// Returns the normalized RGBA components as a space-separated string with 3 decimal precision.
    /// </summary>
    /// <returns>A string in the form "R G B A" with float values between 0 and 1.</returns>
    public override string ToString()
    {
        return $"{RedF:F3} {GreenF:F3} {BlueF:F3} {AlphaF:F3}";
    }

    /// <summary>
    /// Converts a byte channel value to a normalized float in the range [0, 1].
    /// </summary>
    /// <param name="channel">The byte channel value (0–255).</param>
    /// <returns>The normalized float value.</returns>
    private static float Normalize(byte channel)
    {
        return channel / 255f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MaterialColor(uint value)
    {
        return new(value);
    }
}
