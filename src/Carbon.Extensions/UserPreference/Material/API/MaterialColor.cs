using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.UserPreference.Material.API;

/// <summary>
/// Represents a UI-facing color value in ARGB format, with convenience accessors
/// for individual channels and normalized [0,1] float components for UI display.
/// </summary>
public struct MaterialColor
{
    /// <summary>
    /// Gets the packed ARGB value as a 32-bit unsigned integer.
    /// </summary>
    [JsonProperty]
    public uint Value { get; private set; }

    /// <summary>
    /// Gets the alpha (opacity) channel component (0–255).
    /// </summary>
    [JsonIgnore]
    public readonly byte Alpha => (byte)((Value >> 24) & 0xFF);

    /// <summary>
    /// Gets the red channel component (0–255).
    /// </summary>
    [JsonIgnore]
    public readonly byte Red => (byte)((Value >> 16) & 0xFF);

    /// <summary>
    /// Gets the green channel component (0–255).
    /// </summary>
    [JsonIgnore]
    public readonly byte Green => (byte)((Value >> 8) & 0xFF);

    /// <summary>
    /// Gets the blue channel component (0–255).
    /// </summary>
    [JsonIgnore]
    public readonly byte Blue => (byte)(Value & 0xFF);

    /// <summary>
    /// Gets the normalized alpha (opacity) component in the range [0, 1].
    /// </summary>
    [JsonIgnore]
    public readonly float AlphaF => Normalize(Alpha);

    /// <summary>
    /// Gets the normalized red component in the range [0, 1].
    /// </summary>
    [JsonIgnore]
    public readonly float RedF => Normalize(Red);

    /// <summary>
    /// Gets the normalized green component in the range [0, 1].
    /// </summary>
    [JsonIgnore]
    public readonly float GreenF => Normalize(Green);

    /// <summary>
    /// Gets the normalized blue component in the range [0, 1].
    /// </summary>
    [JsonIgnore]
    public readonly float BlueF => Normalize(Blue);

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialColor"/> struct with the specified ARGB value.
    /// </summary>
    /// <param name="value">The 32-bit ARGB color value.</param>
    public MaterialColor(uint value)
    {
        Value = value;
    }
    
    public readonly string ToRgbHex()
    {
        return $"#{Red:X2}{Green:X2}{Blue:X2}";
    }

    public readonly string ToRgbaHex()
    {
        return $"#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}";
    }

    /// <summary>
    /// Returns the normalized RGBA components as a space-separated string with 3 decimal precision.
    /// </summary>
    /// <returns>A string in the form "R G B A" with float values between 0 and 1.</returns>
    public override readonly string ToString()
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

    public static implicit operator MaterialColor(uint value)
    {
        return new(value);
    }

    public static implicit operator uint(MaterialColor color)
    {
        return color.Value;
    }

    public static implicit operator string(MaterialColor color)
    {
        return color.ToString();
    }
}
