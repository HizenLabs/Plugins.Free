using Facepunch;
using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using System;
using System.Collections.Generic;

namespace HizenLabs.Extensions.UserPreference.Material.Palettes;

public sealed class TonalPalette : IDisposable, Pool.IPooled
{
    private Dictionary<int, int> _cache;
    private Hct _keyColor;
    private double _hue;
    private double _chroma;

    public static TonalPalette Create(StandardRgb sRgb)
    {
        var palette = Pool.Get<TonalPalette>();
        palette._keyColor = Hct.Create(sRgb);

        throw new NotImplementedException();
    }

    public void Dispose()
    {
        var obj = this;
        Pool.Free(ref obj);
    }

    public void EnterPool()
    {
        Pool.FreeUnmanaged(ref _cache);

        _keyColor = null;
        _hue = default;
        _chroma = default;
    }

    public void LeavePool()
    {
        _cache = Pool.Get<Dictionary<int, int>>();
    }
}
