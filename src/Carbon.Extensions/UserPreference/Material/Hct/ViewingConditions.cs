using Facepunch;
using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using System;

namespace HizenLabs.Extensions.UserPreference.Material.Hct;

/// <summary>
/// Represents a set of viewing conditions used in the CAM16 color appearance model.
/// Caches intermediate values to accelerate repeated CAM16 conversions.
/// </summary>
public sealed class ViewingConditions : IDisposable, Pool.IPooled
{
    /// <summary>
    /// Default viewing conditions with a background lightness (L*) of 50.0.
    /// </summary>
    public static readonly ViewingConditions Default = CreateDefaultWithBackgroundLstar(50.0);

    private double _n;
    private double _aw;
    private double _nbb;
    private double _ncb;
    private double _c;
    private double _nc;
    private Cam16Rgb _rgbD;
    private double _fl;
    private double _flRoot;
    private double _z;

    /// <summary>
    /// Ratio of background luminance to the luminance of the adapting field (Y_b / Y_w).
    /// </summary>
    public double N => _n;

    /// <summary>
    /// The achromatic response to the white point, used in the computation of perceived lightness.
    /// </summary>
    public double Aw => _aw;

    /// <summary>
    /// Background induction factor used in chromatic adaptation.
    /// </summary>
    public double Nbb => _nbb;

    /// <summary>
    /// Chromatic induction factor used for the color opponent channels.
    /// </summary>
    public double Ncb => _ncb;

    /// <summary>
    /// Surround exponential non-linearity (C), describing the degree of chromatic adaptation.
    /// </summary>
    public double C => _c;

    /// <summary>
    /// Degree of adaptation (Nc), based on surround conditions.
    /// </summary>
    public double Nc => _nc;

    /// <summary>
    /// The RGB discounting factors used to simulate incomplete adaptation to the illuminant.
    /// </summary>
    public Cam16Rgb RgbD => _rgbD;

    /// <summary>
    /// Luminance-level adaptation factor (FL), accounting for eye response to different luminance levels.
    /// </summary>
    public double Fl => _fl;

    /// <summary>
    /// The fourth root of <see cref="Fl"/>; used to simplify appearance calculations.
    /// </summary>
    public double FlRoot => _flRoot;

    /// <summary>
    /// Base exponential non-linearity used in computing brightness and chroma.
    /// </summary>
    public double Z => _z;

    /// <summary>
    /// Gets an instance of <see cref="ViewingConditions"/> from the pool and initializes it with the provided parameters.
    /// </summary>
    /// <param name="n">Ratio of background luminance to the luminance of the adapting field.</param>
    /// <param name="aw">Achromatic response to the white point.</param>
    /// <param name="nbb">Background induction factor.</param>
    /// <param name="ncb">Chromatic induction factor.</param>
    /// <param name="c">Surround exponential non-linearity.</param>
    /// <param name="nc">Degree of adaptation.</param>
    /// <param name="rgbD">RGB discounting factors.</param>
    /// <param name="fl">Luminance-level adaptation factor.</param>
    /// <param name="flRoot">Fourth root of <see cref="Fl"/>.</param>
    /// <param name="z">Base exponential non-linearity.</param>
    /// <returns>A new instance of <see cref="ViewingConditions"/>.</returns>
    private static ViewingConditions Create(
        double n,
        double aw,
        double nbb,
        double ncb,
        double c,
        double nc,
        Cam16Rgb rgbD,
        double fl,
        double flRoot,
        double z)
    {
        var viewingConditions = Pool.Get<ViewingConditions>();
        viewingConditions._n = n;
        viewingConditions._aw = aw;
        viewingConditions._nbb = nbb;
        viewingConditions._ncb = ncb;
        viewingConditions._c = c;
        viewingConditions._nc = nc;
        viewingConditions._rgbD = rgbD;
        viewingConditions._fl = fl;
        viewingConditions._flRoot = flRoot;
        viewingConditions._z = z;
        return viewingConditions;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ViewingConditions"/> with the specified parameters.
    /// </summary>
    /// <param name="whitePoint">The white point of the viewing conditions.</param>
    /// <param name="adaptingLuminance">The adapting luminance of the viewing conditions.</param>
    /// <param name="backgroundLstar">The background lightness (L*) of the viewing conditions.</param>
    /// <param name="surround">The surround factor of the viewing conditions.</param>
    /// <param name="discountingIlluminant">Whether to discount the illuminant.</param>
    /// <returns>A new instance of <see cref="ViewingConditions"/>.</returns>
    public static ViewingConditions Create(
        ColorXyz whitePoint,
        double adaptingLuminance,
        double backgroundLstar,
        double surround,
        bool discountingIlluminant)
    {
        backgroundLstar = Math.Max(0.1, backgroundLstar);

        var rgbW = whitePoint.ToCam16Rgb();

        double f = 0.8 + (surround / 10.0);
        double c = f >= 0.9
            ? MathUtils.Lerp(0.59, 0.69, (f - 0.9) * 10.0)
            : MathUtils.Lerp(0.525, 0.59, (f - 0.8) * 10.0);

        double d = discountingIlluminant
            ? 1.0
            : f * (1.0 - (1.0 / 3.6 * Math.Exp((-adaptingLuminance - 42.0) / 92.0)));
        d = MathUtils.Clamp(0.0, 1.0, d);

        double nc = f;

        Cam16Rgb rgbD = new
        (
            d * (100.0 / rgbW.R) + 1.0 - d,
            d * (100.0 / rgbW.G) + 1.0 - d,
            d * (100.0 / rgbW.B) + 1.0 - d
        );

        double k = 1.0 / (5.0 * adaptingLuminance + 1.0);
        double k4 = k * k * k * k;
        double k4F = 1.0 - k4;
        double fl = (k4 * adaptingLuminance) + (0.1 * k4F * k4F * Math.Pow(5.0 * adaptingLuminance, 1d / 3d));

        double n = ColorUtils.YFromLstar(backgroundLstar) / whitePoint[1];
        double z = 1.48 + Math.Sqrt(n);
        double nbb = 0.725 / Math.Pow(n, 0.2);
        double ncb = nbb;

        var rgbA = rgbD.ToChromaticAdaptation(fl, rgbW, false);

        double aw = (2.0 * rgbA.R + rgbA.G + 0.05 * rgbA.B) * nbb;

        return Create(n, aw, nbb, ncb, c, nc, rgbD, fl, Math.Pow(fl, 0.25), z);
    }

    public static ViewingConditions CreateDefaultWithBackgroundLstar(double lstar)
    {
        return Create(
            WhitePoints.D65,
            200.0 / Math.PI * ColorUtils.YFromLstar(50.0) / 100.0,
            lstar,
            2.0,
            false
        );
    }

    /// <summary>
    /// Frees the <see cref="ViewingConditions"/> object, sending it back to the pool.
    /// </summary>
    public void Dispose()
    {
        if (ReferenceEquals(this, Default)) // Do not free the default instance; it is very mean to do so.
            return;

        var obj = this;
        Pool.Free(ref obj);
    }

    /// <summary>
    /// Enters the pool, resetting the object for reuse.
    /// </summary>
    public void EnterPool()
    {
        _n = default;
        _aw = default;
        _nbb = default;
        _ncb = default;
        _c = default;
        _nc = default;
        _rgbD = default;
        _fl = default;
        _flRoot = default;
        _z = default;
    }

    /// <summary>
    /// Leaves the pool, preparing the object to be used.
    /// </summary>
    public void LeavePool() { }
}
