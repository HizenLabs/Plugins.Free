using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using System;
using System.Collections.Generic;

namespace HizenLabs.Extensions.UserPreference.Material.Palettes;

public sealed class TonalPalette
{
    private Dictionary<int, int> _cache;
    private Hct _keyColor;
    private double _hue;
    private double _chroma;

    public static TonalPalette Create(StandardRgb sRgb)
    {
        var hct = Hct.Create(sRgb);

        throw new NotImplementedException();
    }
}
