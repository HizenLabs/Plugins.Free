using System;
using System.Collections.Generic;
using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Palettes;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;

namespace HizenLabs.Extensions.UserPreference.Material.DynamicColors;

public sealed class DynamicColor
{
    public string Name { get; }

    public Func<DynamicScheme, TonalPalette> Palette { get; }

    public Func<DynamicScheme, double> Tone { get; }

    public bool IsBackground { get; }

    public Func<DynamicScheme, double> ChromaMultiplier { get; }

    public Func<DynamicScheme, DynamicColor> Background { get; }

    public Func<DynamicScheme, DynamicColor> SecondBackground { get; }

    public Func<DynamicScheme, ContrastCurve> ContrastCurve { get; }

    public Func<DynamicScheme, ToneDeltaPair> ToneDeltaPair { get; }

    public Func<DynamicScheme, double> Opacity { get; }

    private readonly Dictionary<DynamicScheme, Hct> _hctCache = new();

    public DynamicColor(
        string name,
        Func<DynamicScheme, TonalPalette> palette,
        Func<DynamicScheme, double> tone,
        bool isBackground,
        Func<DynamicScheme, DynamicColor> background = null,
        Func<DynamicScheme, DynamicColor> secondBackground = null,
        ContrastCurve contrastCurve = null,
        Func<DynamicScheme, ToneDeltaPair> toneDeltaPair = null
    ) : this(
        name, palette, tone, isBackground, null,
        background, secondBackground,
        contrastCurve is not null ? _ => contrastCurve : null,
        toneDeltaPair, null)
    { }

    public DynamicColor(
        string name,
        Func<DynamicScheme, TonalPalette> palette,
        Func<DynamicScheme, double> tone,
        bool isBackground,
        Func<DynamicScheme, double> chromaMultiplier,
        Func<DynamicScheme, DynamicColor> background,
        Func<DynamicScheme, DynamicColor> secondBackground,
        Func<DynamicScheme, ContrastCurve> contrastCurve,
        Func<DynamicScheme, ToneDeltaPair> toneDeltaPair,
        Func<DynamicScheme, double> opacity
    )
    {
        Name = name;
        Palette = palette;
        Tone = tone;
        IsBackground = isBackground;
        ChromaMultiplier = chromaMultiplier;
        Background = background;
        SecondBackground = secondBackground;
        ContrastCurve = contrastCurve;
        ToneDeltaPair = toneDeltaPair;
        Opacity = opacity;
    }

    public static DynamicColor FromPalette(string name, Func<DynamicScheme, TonalPalette> palette, Func<DynamicScheme, double> tone) =>
        new(name, palette, tone, false);

    /*
    public static DynamicColor FromPalette(string name, Func<DynamicScheme, TonalPalette> palette, Func<DynamicScheme, double> tone, bool isBackground) =>
        new(name, palette, tone, isBackground);

    public static DynamicColor FromArgb(string name, StandardRgb color)
    {
        var hct = Hct.Create(color);
        var palette = TonalPalette.Create(color);
        return FromPalette(name, _ => palette, _ => hct.Tone);
    }
    */

    public StandardRgb GetColor(DynamicScheme scheme)
    {
        using var hct = GetHct(scheme);
        var color = hct.Color;
        if (Opacity?.Invoke(scheme) is not double o)
            return color;

        byte alpha = (byte)MathUtils.Clamp(Math.Round(o * 255), 0, 255);
        return new(color, alpha);
    }

    public Hct GetHct(DynamicScheme scheme)
    {
        if (_hctCache.TryGetValue(scheme, out var cached))
            return cached;

        var computed = ColorSpecs.Get(scheme.SpecVersion).GetHct(scheme, this);
        if (_hctCache.Count > 4)
            _hctCache.Clear();

        _hctCache[scheme] = computed;
        return computed;
    }

    public double GetTone(DynamicScheme scheme) =>
        ColorSpecs.Get(scheme.SpecVersion).GetTone(scheme, this);

    public static double ForegroundTone(double bgTone, double ratio)
    {
        double lighter = Contrast.LighterUnsafe(bgTone, ratio);
        double darker = Contrast.DarkerUnsafe(bgTone, ratio);
        double lightRatio = Contrast.RatioOfTones(lighter, bgTone);
        double darkRatio = Contrast.RatioOfTones(darker, bgTone);
        bool preferLight = TonePrefersLightForeground(bgTone);

        bool negligible = Math.Abs(lightRatio - darkRatio) < 0.1 && lightRatio < ratio && darkRatio < ratio;
        if (preferLight)
            return (lightRatio >= ratio || lightRatio >= darkRatio || negligible) ? lighter : darker;
        else
            return (darkRatio >= ratio || darkRatio >= lightRatio) ? darker : lighter;
    }

    public static double EnableLightForeground(double tone) =>
        TonePrefersLightForeground(tone) && !ToneAllowsLightForeground(tone) ? 49.0 : tone;

    public static bool TonePrefersLightForeground(double tone) =>
        Math.Round(tone) < 60;

    public static bool ToneAllowsLightForeground(double tone) =>
        Math.Round(tone) <= 49;

    public static Func<DynamicScheme, double> GetInitialToneFromBackground(Func<DynamicScheme, DynamicColor> background) =>
        background is null ? _ => 50.0 : s => background(s)?.GetTone(s) ?? 50.0;

    public Builder ToBuilder() => new(this);

    public sealed class Builder
    {
        public string Name;
        public Func<DynamicScheme, TonalPalette> Palette;
        public Func<DynamicScheme, double> Tone;
        public bool IsBackground;
        public Func<DynamicScheme, double> ChromaMultiplier;
        public Func<DynamicScheme, DynamicColor> Background;
        public Func<DynamicScheme, DynamicColor> SecondBackground;
        public Func<DynamicScheme, ContrastCurve> ContrastCurve;
        public Func<DynamicScheme, ToneDeltaPair> ToneDeltaPair;
        public Func<DynamicScheme, double> Opacity;

        public Builder() { }

        public Builder(DynamicColor original)
        {
            Name = original.Name;
            Palette = original.Palette;
            Tone = original.Tone;
            IsBackground = original.IsBackground;
            ChromaMultiplier = original.ChromaMultiplier;
            Background = original.Background;
            SecondBackground = original.SecondBackground;
            ContrastCurve = original.ContrastCurve;
            ToneDeltaPair = original.ToneDeltaPair;
            Opacity = original.Opacity;
        }

        public DynamicColor Build()
        {
            if (Background is null && SecondBackground is not null)
                throw new InvalidOperationException($"{Name} has secondBackground defined but not background.");
            if (Background is null && ContrastCurve is not null)
                throw new InvalidOperationException($"{Name} has contrastCurve defined but not background.");
            if (Background is not null && ContrastCurve is null)
                throw new InvalidOperationException($"{Name} has background defined but no contrastCurve.");

            return new DynamicColor(
                Name,
                Palette,
                Tone ?? GetInitialToneFromBackground(Background),
                IsBackground,
                ChromaMultiplier,
                Background,
                SecondBackground,
                ContrastCurve,
                ToneDeltaPair,
                Opacity
            );
        }

        public Builder SetName(string name) { Name = name; return this; }
        public Builder SetPalette(Func<DynamicScheme, TonalPalette> palette) { Palette = palette; return this; }
        public Builder SetTone(Func<DynamicScheme, double> tone) { Tone = tone; return this; }
        public Builder SetIsBackground(bool isBackground) { IsBackground = isBackground; return this; }
        public Builder SetChromaMultiplier(Func<DynamicScheme, double> chromaMultiplier) { ChromaMultiplier = chromaMultiplier; return this; }
        public Builder SetBackground(Func<DynamicScheme, DynamicColor> background) { Background = background; return this; }
        public Builder SetSecondBackground(Func<DynamicScheme, DynamicColor> secondBackground) { SecondBackground = secondBackground; return this; }
        public Builder SetContrastCurve(Func<DynamicScheme, ContrastCurve> contrastCurve) { ContrastCurve = contrastCurve; return this; }
        public Builder SetToneDeltaPair(Func<DynamicScheme, ToneDeltaPair> toneDeltaPair) { ToneDeltaPair = toneDeltaPair; return this; }
        public Builder SetOpacity(Func<DynamicScheme, double> opacity) { Opacity = opacity; return this; }
    }
}
