using Facepunch;
using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using System;

namespace HizenLabs.Extensions.UserPreference.Material.Hct;

/// <summary>
/// Represents a color in the CAM16 color appearance model, which describes perceived color under varying viewing conditions.
/// </summary>
public sealed class Cam16 : IDisposable, Pool.IPooled
{
    private double _hue;
    private double _chroma;
    private double _j;
    private double _q;
    private double _m;
    private double _s;
    private double _jstar;
    private double _astar;
    private double _bstar;

    /// <summary>
    /// Gets the hue angle in degrees (0–360), representing perceived color type.
    /// </summary>
    public double Hue => _hue;

    /// <summary>
    /// Gets the chroma, indicating perceived colorfulness relative to brightness.
    /// </summary>
    public double Chroma => _chroma;

    /// <summary>
    /// Gets the lightness (J), representing perceived brightness relative to white.
    /// </summary>
    public double Lightness => _j;

    /// <summary>
    /// Gets the brightness (Q), indicating absolute perceived brightness.
    /// </summary>
    public double Brightness => _q;

    /// <summary>
    /// Gets the colorfulness (M), representing intensity of perceived color.
    /// </summary>
    public double Colorfulness => _m;

    /// <summary>
    /// Gets the saturation (S), indicating perceived intensity of hue relative to brightness.
    /// </summary>
    public double Saturation => _s;

    /// <summary>
    /// Gets the CAM16-UCS J* component (perceptual lightness in uniform space).
    /// </summary>
    public double JStar => _jstar;

    /// <summary>
    /// Gets the CAM16-UCS a* component (red–green axis in uniform space).
    /// </summary>
    public double AStar => _astar;

    /// <summary>
    /// Gets the CAM16-UCS b* component (yellow–blue axis in uniform space).
    /// </summary>
    public double BStar => _bstar;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cam16"/> class with the specified CAM16 and CAM16-UCS components.
    /// </summary>
    /// <param name="hue">The hue angle in degrees (0–360), representing the perceived color type.</param>
    /// <param name="chroma">The chroma (C), indicating perceived colorfulness relative to brightness.</param>
    /// <param name="j">The lightness (J), representing perceived brightness relative to a white point.</param>
    /// <param name="q">The brightness (Q), representing absolute perceived brightness.</param>
    /// <param name="m">The colorfulness (M), representing the intensity of the color appearance.</param>
    /// <param name="s">The saturation (S), representing perceived hue intensity relative to brightness.</param>
    /// <param name="jstar">The J* component in CAM16-UCS (uniform lightness).</param>
    /// <param name="astar">The a* component in CAM16-UCS (red–green axis).</param>
    /// <param name="bstar">The b* component in CAM16-UCS (yellow–blue axis).</param>
    private static Cam16 Create(
        double hue,
        double chroma,
        double j,
        double q,
        double m,
        double s,
        double jstar,
        double astar,
        double bstar)
    {
        var cam16 = Pool.Get<Cam16>();
        cam16._hue = hue;
        cam16._chroma = chroma;
        cam16._j = j;
        cam16._q = q;
        cam16._m = m;
        cam16._s = s;
        cam16._jstar = jstar;
        cam16._astar = astar;
        cam16._bstar = bstar;
        return cam16;
    }

    /// <summary>
    /// Create a CAM16 color from a color, assuming the color was viewed in default viewing conditions.
    /// </summary>
    /// <param name="color">The color to convert to CAM16.</param>
    /// <returns>A CAM16 object representing the color.</returns>
    public static Cam16 FromColor(ColorArgb color)
    {
        return FromColorInViewingConditions(color, ViewingConditions.Default);
    }

    /// <summary>
    /// Create a CAM16 color from a color, assuming the color was viewed in the specified viewing conditions.
    /// </summary>
    /// <param name="color">The color to convert to CAM16.</param>
    /// <param name="viewingConditions">The viewing conditions under which the color was viewed.</param>
    /// <returns>A CAM16 object representing the color.</returns>
    public static Cam16 FromColorInViewingConditions(ColorArgb color, ViewingConditions viewingConditions)
    {
        // Transform ARGB int to XYZ
        var colorXyz = ColorUtils.XyzFromArgb(color);

        return FromXyzInViewingConditions(colorXyz, viewingConditions);
    }

    public static Cam16 FromXyzInViewingConditions(ColorXyz color, ViewingConditions viewingConditions)
    {
        // Transform XYZ to 'cone'/'rgb' responses
        var matrix = ColorTransforms.XyzToCam16rgb;
        var rgbT = color * matrix;

        // Discount illuminant
        var rgbD = viewingConditions.RgbD * rgbT;

        // Chromatic adaptation
        double rAF = Math.Pow(viewingConditions.Fl * Math.Abs(rgbD.X) / 100.0, 0.42);
        double gAF = Math.Pow(viewingConditions.Fl * Math.Abs(rgbD.Y) / 100.0, 0.42);
        double bAF = Math.Pow(viewingConditions.Fl * Math.Abs(rgbD.Z) / 100.0, 0.42);
        double rA = Math.Sign(rgbD.X) * 400.0 * rAF / (rAF + 27.13);
        double gA = Math.Sign(rgbD.Y) * 400.0 * gAF / (gAF + 27.13);
        double bA = Math.Sign(rgbD.Z) * 400.0 * bAF / (bAF + 27.13);

        // redness-greenness
        double a = (11.0 * rA + -12.0 * gA + bA) / 11.0;
        // yellowness-blueness
        double b = (rA + gA - 2.0 * bA) / 9.0;

        // auxiliary components
        double u = (20.0 * rA + 20.0 * gA + 21.0 * bA) / 20.0;
        double p2 = (40.0 * rA + 20.0 * gA + bA) / 20.0;

        // hue
        double atan2 = Math.Atan2(b, a);
        double atanDegrees = MathUtils.ToDegrees(atan2);
        double hue =
            atanDegrees < 0
                ? atanDegrees + 360.0
                : atanDegrees >= 360 ? atanDegrees - 360.0 : atanDegrees;
        double hueRadians = MathUtils.ToRadians(hue);

        // achromatic response to color
        double ac = p2 * viewingConditions.Nbb;

        // CAM16 lightness and brightness
        double j =
            100.0
                * Math.Pow(
                    ac / viewingConditions.Aw,
                    viewingConditions.C * viewingConditions.Z);
        double q =
            4.0
                / viewingConditions.C
                * Math.Sqrt(j / 100.0)
                * (viewingConditions.Aw + 4.0)
                * viewingConditions.FlRoot;

        // CAM16 chroma, colorfulness, and saturation.
        double huePrime = (hue < 20.14) ? hue + 360 : hue;
        double eHue = 0.25 * (Math.Cos(MathUtils.ToRadians(huePrime) + 2.0) + 3.8);
        double p1 = 50000.0 / 13.0 * eHue * viewingConditions.Nc * viewingConditions.Ncb;
        double t = p1 * MathUtils.Hypotenuse(a, b) / (u + 0.305);
        double alpha =
            Math.Pow(1.64 - Math.Pow(0.29, viewingConditions.N), 0.73) * Math.Pow(t, 0.9);
        // CAM16 chroma, colorfulness, saturation
        double c = alpha * Math.Sqrt(j / 100.0);
        double m = c * viewingConditions.FlRoot;
        double s =
            50.0 * Math.Sqrt((alpha * viewingConditions.C) / (viewingConditions.Aw + 4.0));

        // CAM16-UCS components
        double jstar = (1.0 + 100.0 * 0.007) * j / (1.0 + 0.007 * j);
        double mstar = 1.0 / 0.0228 * Math.Log(1.0 + 0.0228 * m);
        double astar = mstar * Math.Cos(hueRadians);
        double bstar = mstar * Math.Sin(hueRadians);

        return Create(hue, c, j, q, m, s, jstar, astar, bstar);
    }

    /// <summary>
    /// Frees the Cam16 object, sending it back to the pool.
    /// </summary>
    public void Dispose()
    {
        var obj = this;
        Pool.Free(ref obj);
    }

    /// <summary>
    /// Resets the properties of the Cam16 object to their default values before it returns to the pool.
    /// </summary>
    public void EnterPool()
    {
        _hue = default;
        _chroma = default;
        _j = default;
        _q = default;
        _m = default;
        _s = default;
        _jstar = default;
        _astar = default;
        _bstar = default;
    }

    /// <summary>
    /// Prepares the Cam16 object for reuse after being retrieved from the pool.
    /// </summary>
    public void LeavePool() { }
}
