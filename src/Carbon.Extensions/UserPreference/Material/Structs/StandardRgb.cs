using HizenLabs.Extensions.UserPreference.Material.Utils;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a color in ARGB format.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct StandardRgb
{
    /// <summary>
    /// The ARGB value of the color.
    /// </summary>
    [FieldOffset(0)]
    public readonly uint Value;

    /// <summary>
    /// The blue component of the color (0-255).
    /// </summary>
    [FieldOffset(0)]
    public readonly byte B;

    /// <summary>
    /// The green component of the color (0-255).
    /// </summary>
    [FieldOffset(1)]
    public readonly byte G;

    /// <summary>
    /// The red component of the color (0-255).
    /// </summary>
    [FieldOffset(2)]
    public readonly byte R;

    /// <summary>
    /// The alpha component of the color (0-255).
    /// </summary>
    [FieldOffset(3)]
    public readonly byte A;

    /// <summary>
    /// Returns true if the color is opaque (alpha = 255).
    /// </summary>
    public bool IsOpaque => A == 255;

    /// <summary>
    /// Initializes a new instance of the <see cref="StandardRgb"/> struct with the specified ARGB value.
    /// </summary>
    /// <param name="argb">The ARGB value of the color.</param>
    public StandardRgb(uint argb)
    {
        B = G = R = A = 0;
        Value = argb;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StandardRgb"/> struct with the specified alpha, red, green, and blue values.
    /// </summary>
    /// <param name="a">The alpha component of the color (0-255).</param>
    /// <param name="r">The red component of the color (0-255).</param>
    /// <param name="g">The green component of the color (0-255).</param>
    /// <param name="b">The blue component of the color (0-255).</param>
    public StandardRgb(byte a, byte r, byte g, byte b)
    {
        Value = 0;
        A = a;
        R = r;
        G = g;
        B = b;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StandardRgb"/> struct with the specified red, green, and blue values.
    /// </summary>
    /// <param name="r">The red component of the color (0-255).</param>
    /// <param name="g">The green component of the color (0-255).</param>
    /// <param name="b">The blue component of the color (0-255).</param>
    public StandardRgb(byte r, byte g, byte b) : this(255, r, g, b)
    {
    }

    /// <summary>
    /// Converts the <see cref="StandardRgb"/> struct to a <see cref="LinearRgb"/> object.
    /// </summary>
    /// <returns></returns>
    public LinearRgb ToLinearRgb()
    {
        return new
        (
            ColorUtils.LinearizeComponent(R),
            ColorUtils.LinearizeComponent(G),
            ColorUtils.LinearizeComponent(B)
        );
    }

    /// <summary>
    /// Converts the <see cref="StandardRgb"/> struct to a <see cref="CieXyz"/> object.
    /// </summary>
    /// <returns></returns>
    public CieXyz ToCieXyz()
    {
        return ToLinearRgb().ToColorXyz();
    }

    /// <summary>
    /// Converts the <see cref="StandardRgb"/> struct to a <see cref="Color"/> object.
    /// </summary>
    /// <param name="argb"></param>
    public static implicit operator StandardRgb(uint argb) => new(argb);
}
