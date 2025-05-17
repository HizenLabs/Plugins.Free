using Facepunch;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using System;

namespace HizenLabs.Extensions.UserPreference.Material.ColorSpaces;

/// <summary>
/// Represents a color in the HCT (Hue, Chroma, Tone) color space.
/// This model is perceptually uniform and derived from CAM16, suitable for
/// dynamic theming based on user-perceived color differences.
/// </summary>
public class Hct : IDisposable, Pool.IPooled
{
    /// <summary>
    /// Hue angle of the color in degrees (0–360).
    /// </summary>
    public double Hue { get; private set; }

    /// <summary>
    /// Chroma (colorfulness) of the color.
    /// </summary>
    public double Chroma { get; private set; }

    /// <summary>
    /// Tone (lightness) of the color, corresponding to L* (0–100).
    /// </summary>
    public double Tone { get; private set; }

    /// <summary>
    /// The original sRGB color used to compute the HCT values.
    /// </summary>
    public StandardRgb Color { get; private set; }

    /// <summary>
    /// Creates a pooled instance of <see cref="Hct"/> from an sRGB color.
    /// </summary>
    /// <param name="seed">The input sRGB color.</param>
    /// <returns>A pooled <see cref="Hct"/> instance representing the color.</returns>
    public static Hct Create(StandardRgb seed)
    {
        var hct = Pool.Get<Hct>();
        hct.SetInternalState(seed);
        return hct;
    }

    public static Hct Create(double hue, double chroma, double tone)
    {
        var color = HctSolver.SolveToColor(hue, chroma, tone);

        return Create(color);
    }

    /// <summary>
    /// Sets the internal HCT state based on the provided sRGB color.
    /// </summary>
    /// <param name="color">The input color in sRGB space.</param>
    public void SetInternalState(StandardRgb color)
    {
        Color = color;

        using var cam = Cam16.FromColor(color);
        Hue = cam.Hue;
        Chroma = cam.Chroma;
        Tone = ColorUtils.LstarFromArgb(color);
    }

    /// <summary>
    /// Returns this object to the pool.
    /// </summary>
    public void Dispose()
    {
        var obj = this;
        Pool.Free(ref obj);
    }

    /// <summary>
    /// Called when the object enters the pool. Resets its state.
    /// </summary>
    public void EnterPool()
    {
        Hue = default;
        Chroma = default;
        Tone = default;
        Color = default;
    }

    /// <summary>
    /// Called when the object is retrieved from the pool.
    /// </summary>
    public void LeavePool() { }
}
