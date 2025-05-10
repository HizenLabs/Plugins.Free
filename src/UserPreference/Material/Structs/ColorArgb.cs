using System.Runtime.InteropServices;
using UnityEngine;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a color in ARGB format.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct ColorArgb
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
    /// Initializes a new instance of the <see cref="ColorArgb"/> struct with the specified ARGB value.
    /// </summary>
    /// <param name="argb">The ARGB value of the color.</param>
    public ColorArgb(uint argb)
    {
        B = G = R = A = 0;
        Value = argb;
    }

    public ColorArgb(byte a, byte r, byte g, byte b)
    {
        Value = 0;
        A = a;
        R = r;
        G = g;
        B = b;
    }

    public ColorArgb(byte r, byte g, byte b) : this(255, r, g, b)
    {
    }

    /// <summary>
    /// Converts the <see cref="ColorArgb"/> struct to a <see cref="Color"/> object.
    /// </summary>
    /// <param name="argb"></param>
    public static implicit operator ColorArgb(uint argb) => new(argb);
}
