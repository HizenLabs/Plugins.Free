using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a color in ARGB format (Alpha, Red, Green, Blue), packed into a 32-bit unsigned integer.
/// Provides both individual component access and color space conversion utilities.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal readonly struct StandardRgb : IEquatable<StandardRgb>
{
    [FieldOffset(0)] public readonly uint Value;
    [FieldOffset(0)] public readonly byte B;
    [FieldOffset(1)] public readonly byte G;
    [FieldOffset(2)] public readonly byte R;
    [FieldOffset(3)] public readonly byte A;

    /// <summary>
    /// Returns true if the alpha component is fully opaque (255).
    /// </summary>
    public bool IsOpaque => A == 255;

    /// <summary>
    /// Initializes from a packed ARGB value.
    /// </summary>
    public StandardRgb(uint argb)
    {
        B = G = R = A = 0; // Required before setting `Value` due to readonly struct + FieldOffset
        Value = argb;
    }

    /// <summary>
    /// Initializes from individual ARGB components.
    /// </summary>
    public StandardRgb(byte a, byte r, byte g, byte b)
    {
        Value = 0; // Required for readonly struct + FieldOffset
        A = a;
        R = r;
        G = g;
        B = b;
    }

    /// <summary>
    /// Initializes from RGB, with alpha defaulting to 255 (opaque).
    /// </summary>
    public StandardRgb(byte r, byte g, byte b) : this(255, r, g, b) { }

    /// <summary>
    /// Initializes from another RGB and replaces the alpha value.
    /// </summary>
    public StandardRgb(StandardRgb rgb, byte a) : this(a, rgb.R, rgb.G, rgb.B) { }

    /// <summary>
    /// Converts to CIE XYZ using linear RGB conversion.
    /// </summary>
    public CieXyz ToCieXyz() => ToLinearRgb().ToColorXyz();

    /// <summary>
    /// Converts to Lab color space.
    /// </summary>
    public Lab ToLab()
    {
        var xyz = ToCieXyz();
        var normalized = xyz / WhitePoints.D65;
        var labFxyz = ColorUtils.LabF(normalized);
        return labFxyz.ToLab();
    }

    /// <summary>
    /// Converts to linear RGB.
    /// </summary>
    public LinearRgb ToLinearRgb() => new(
        ColorUtils.LinearizeComponent(R),
        ColorUtils.LinearizeComponent(G),
        ColorUtils.LinearizeComponent(B)
    );

    public override string ToString()
    {
        return StringUtils.HexFromColor(this);
    }

    /// <summary>
    /// Implicit conversion from uint (packed ARGB) to <see cref="StandardRgb"/>.
    /// </summary>
    public static implicit operator StandardRgb(uint argb) => new(argb);

    // Equality and hash code implementation
    public static bool operator ==(StandardRgb left, StandardRgb right) => left.Value == right.Value;
    public static bool operator !=(StandardRgb left, StandardRgb right) => left.Value != right.Value;

    public bool Equals(StandardRgb other) => Value == other.Value;
    public override bool Equals(object obj) => obj is StandardRgb other && Equals(other);
    public override int GetHashCode() => (int)Value;
}
