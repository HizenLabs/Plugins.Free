using System;
using UnityEngine;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a color in ARGB format.
/// </summary>
public readonly struct ColorArgb
{
    /// <summary>
    /// The ARGB value of the color.
    /// </summary>
    public readonly uint Value;

    /// <summary>
    /// The alpha component of the color (0-255).
    /// </summary>
    public readonly byte A;

    /// <summary>
    /// The red component of the color (0-255).
    /// </summary>
    public readonly byte R;

    /// <summary>
    /// The green component of the color (0-255).
    /// </summary>
    public readonly byte G;

    /// <summary>
    /// The blue component of the color (0-255).
    /// </summary>
    public readonly byte B;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorArgb"/> struct with the specified ARGB value.
    /// </summary>
    /// <param name="argb">The ARGB value of the color.</param>
    public ColorArgb(uint argb)
    {
        Value = argb;

        A = (byte)(argb >> 24);
        R = (byte)(argb >> 16);
        G = (byte)(argb >> 8);
        B = (byte)argb;
    }

    public ColorArgb(byte a, byte r, byte g, byte b)
    {
        Value = (uint)((a << 24) | (r << 16) | (g << 8) | b);

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
