using Facepunch;
using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Pooling;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.UserPreference.Material.Palettes;

public sealed class TonalPalette : IDisposable, ITrackedPooled
{
    public Guid TrackingId { get; set; }

    private TonalPaletteCache _cache;

    public Hct KeyColor { get; private set; }

    public double Hue { get; private set; }

    public double Chroma { get; private set; }

    public static TonalPalette Create(StandardRgb sRgb)
    {
        var hct = Hct.Create(sRgb);

        return Create(hct);
    }

    public static TonalPalette FromHueAndChroma(double hue, double chroma)
    {
        Hct keyColor = new KeyColor(hue, chroma).Create();

        return Create(hue, chroma, keyColor);
    }

    public static TonalPalette Create(Hct hct)
    {
        return Create(hct.Hue, hct.Chroma, hct);
    }

    public static TonalPalette Create(double hue, double chroma, Hct keyColor)
    {
        var palette = TrackedPool.Get<TonalPalette>();

        palette.Hue = hue;
        palette.Chroma = chroma;
        palette.KeyColor = keyColor;

        return palette;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Hct GetHct(double tone)
    {
        return Hct.Create(Hue, Chroma, tone);
    }

    public void Dispose()
    {
        var obj = this;
        TrackedPool.Free(ref obj);
    }

    public void EnterPool()
    {
        _cache?.Dispose();
        KeyColor?.Dispose();

        KeyColor = null;
        Hue = default;
        Chroma = default;
    }

    public void LeavePool()
    {
        _cache = TrackedPool.Get<TonalPaletteCache>();
    }
}
