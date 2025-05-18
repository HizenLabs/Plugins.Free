using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using System;
using System.Collections.Generic;

namespace HizenLabs.Extensions.UserPreference.Material.Palettes;

/// <summary>
/// Represents a key color that defines the hue and chroma of a tonal palette.
/// </summary>
internal sealed class KeyColor
{
    public double Hue { get; }
    public double RequestedChroma { get; }

    private const double MaxChromaValue = 200.0;
    private const double Epsilon = 0.01;
    private const int PivotTone = 50;
    private const int ToneStep = 1;

    private readonly Dictionary<int, double> _chromaCache = new();

    public KeyColor(double hue, double requestedChroma)
    {
        Hue = hue;
        RequestedChroma = requestedChroma;
    }

    /// <summary>
    /// Creates the HCT color that best matches the requested hue and chroma.
    /// </summary>
    public Hct Create()
    {
        int lowerTone = 0;
        int upperTone = 100;

        while (lowerTone < upperTone)
        {
            int midTone = (lowerTone + upperTone) / 2;
            bool isAscending = MaxChroma(midTone) < MaxChroma(midTone + ToneStep);
            bool sufficientChroma = MaxChroma(midTone) >= RequestedChroma - Epsilon;

            if (sufficientChroma)
            {
                if (Math.Abs(lowerTone - PivotTone) < Math.Abs(upperTone - PivotTone))
                {
                    upperTone = midTone;
                }
                else
                {
                    if (lowerTone == midTone)
                        return Hct.Create(Hue, RequestedChroma, lowerTone);

                    lowerTone = midTone;
                }
            }
            else
            {
                if (isAscending)
                    lowerTone = midTone + ToneStep;
                else
                    upperTone = midTone;
            }
        }

        return Hct.Create(Hue, RequestedChroma, lowerTone);
    }

    private double MaxChroma(int tone)
    {
        if (!_chromaCache.TryGetValue(tone, out var chroma))
        {
            using var hct = Hct.Create(Hue, MaxChromaValue, tone);
            chroma = hct.Chroma;
            _chromaCache[tone] = chroma;
        }

        return chroma;
    }
}
