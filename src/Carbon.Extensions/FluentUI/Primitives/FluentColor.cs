using System;

namespace HizenLabs.FluentUI.Primitives;

/// <summary>
/// Represents an RGBA color used in the Fluent UI system.
/// </summary>
public readonly struct FluentColor
{
    // Predefined colors
    public static readonly FluentColor Transparent = new(0, 0, 0, 0);
    public static readonly FluentColor TransparentBlur = new(0, 0, 0, 0.1f);
    public static readonly FluentColor Black = new(0, 0, 0, 1);
    public static readonly FluentColor White = new(255, 255, 255, 1);
    public static readonly FluentColor Red = new(255, 0, 0, 1);
    public static readonly FluentColor Green = new(0, 255, 0, 1);
    public static readonly FluentColor Blue = new(0, 0, 255, 1);

    /// <summary>
    /// The red component (0-255).
    /// </summary>
    public byte R { get; }

    /// <summary>
    /// The green component (0-255).
    /// </summary>
    public byte G { get; }

    /// <summary>
    /// The blue component (0-255).
    /// </summary>
    public byte B { get; }

    /// <summary>
    /// The alpha (opacity) component (0.0-1.0).
    /// </summary>
    public float Alpha { get; }

    /// <summary>
    /// Creates a new <see cref="FluentColor"/> with the specified RGBA components.
    /// </summary>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="alpha">Alpha (opacity) component (0.0-1.0).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if alpha is outside the 0.0–1.0 range.</exception>
    public FluentColor(byte r, byte g, byte b, float alpha = 1f)
    {
        if (alpha is < 0f or > 1f)
            throw new ArgumentOutOfRangeException(nameof(alpha), "Alpha must be between 0.0 and 1.0.");

        R = r;
        G = g;
        B = b;
        Alpha = alpha;
    }

    /// <summary>
    /// Creates a new <see cref="FluentColor"/> with the same RGB values but a different alpha value.
    /// </summary>
    /// <param name="alpha">The new alpha value (0.0-1.0).</param>
    /// <returns>A new <see cref="FluentColor"/> with the specified alpha.</returns>
    public readonly FluentColor SetAlpha(float alpha) =>
        new(R, G, B, alpha);

    /// <summary>
    /// Converts a <see cref="FluentColor"/> to a string in the format "R G B A".
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>A string representation of the color in the format "R G B A".</returns>
    public static implicit operator string(FluentColor color) =>
        $"{color.R} {color.G} {color.B} {color.Alpha:0.#}";

    /// <summary>
    /// Parses a string in the format "R G B [A]" into a <see cref="FluentColor"/>.
    /// Alpha is optional and defaults to 1.0.
    /// </summary>
    /// <param name="color">String to parse in the format "R G B [A]".</param>
    /// <returns>The parsed <see cref="FluentColor"/>.</returns>
    /// <exception cref="FormatException">Thrown if the format is invalid.</exception>
    public static implicit operator FluentColor(string color)
    {
        var parts = color.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || parts.Length > 4)
            throw new FormatException("Expected format: 'R G B [A]'");

        return new FluentColor(
            byte.Parse(parts[0]),
            byte.Parse(parts[1]),
            byte.Parse(parts[2]),
            parts.Length == 4 ? float.Parse(parts[3]) : 1f
        );
    }

    public static FluentColor ParseName(string color) => color.ToLower() switch
    {
        "black" => Black,
        "white" => White,
        "red" => Red,
        "green" => Green,
        "blue" => Blue,
        _ => throw new FormatException($"Unrecognized color: '{color}'")
    };
}