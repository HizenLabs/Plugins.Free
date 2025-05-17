﻿/*
 * Copyright 2022 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Facepunch;
using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HizenLabs.Extensions.UserPreference.Material.Temperature;

/// <summary>
/// Design utilities using color temperature theory.
///
/// <para>Analogous colors, complementary color, and cache to efficiently, lazily, generate data for
/// calculations when needed.</para>
/// </summary>
public sealed class TemperatureCache : Pool.IPooled
{
    public Hct Input { get; private set; }

    public Hct PrecomputedComplement;
    public IReadOnlyList<Hct> PrecomputedHctsByTemp;
    public IReadOnlyList<Hct> PrecomputedHctsByHue;
    public IReadOnlyDictionary<Hct, double> PrecomputedTempsByHct;

    /// <summary>
    /// Create a cache that allows calculation of ex. complementary and analogous colors.
    /// </summary>
    /// <param name="input">Color to find complement/analogous colors of. Any colors will have the same tone,
    /// and chroma as the input color, modulo any restrictions due to the other hues having lower 
    /// limits on chroma.</param>
    public static TemperatureCache Create(Hct input)
    {
        var cache = Pool.Get<TemperatureCache>();
        cache.Input = input;
        return cache;
    }

    /// <summary>
    /// A color that complements the input color aesthetically.
    ///
    /// <para>In art, this is usually described as being across the color wheel. History of this shows
    /// intent as a color that is just as cool-warm as the input color is warm-cool.</para>
    /// </summary>
    public Hct GetComplement()
    {
        if (PrecomputedComplement != null)
        {
            return PrecomputedComplement;
        }

        double coldestHue = GetColdest().Hue;
        double coldestTemp = GetTempsByHct()[GetColdest()];

        double warmestHue = GetWarmest().Hue;
        double warmestTemp = GetTempsByHct()[GetWarmest()];
        double range = warmestTemp - coldestTemp;
        bool startHueIsColdestToWarmest = IsBetween(Input.Hue, coldestHue, warmestHue);
        double startHue = startHueIsColdestToWarmest ? warmestHue : coldestHue;
        double endHue = startHueIsColdestToWarmest ? coldestHue : warmestHue;
        double directionOfRotation = 1.0;
        double smallestError = 1000.0;
        Hct answer = GetHctsByHue()[(int)Math.Round(Input.Hue)];

        double complementRelativeTemp = 1.0 - GetRelativeTemperature(Input);
        // Find the color in the other section, closest to the inverse percentile
        // of the input color. This is the complement.
        for (double hueAddend = 0.0; hueAddend <= 360.0; hueAddend += 1.0)
        {
            double hue = Utils.MathUtils.SanitizeDegrees(
                startHue + directionOfRotation * hueAddend);
            if (!IsBetween(hue, startHue, endHue))
            {
                continue;
            }
            Hct possibleAnswer = GetHctsByHue()[(int)Math.Round(hue)];
            double relativeTemp =
                (GetTempsByHct()[possibleAnswer] - coldestTemp) / range;
            double error = Math.Abs(complementRelativeTemp - relativeTemp);
            if (error < smallestError)
            {
                smallestError = error;
                answer = possibleAnswer;
            }
        }
        PrecomputedComplement = answer;
        return PrecomputedComplement;
    }

    /// <summary>
    /// 5 colors that pair well with the input color.
    ///
    /// <para>The colors are equidistant in temperature and adjacent in hue.</para>
    /// </summary>
    public List<Hct> GetAnalogousColors()
    {
        return GetAnalogousColors(5, 12);
    }

    /// <summary>
    /// A set of colors with differing hues, equidistant in temperature.
    ///
    /// <para>In art, this is usually described as a set of 5 colors on a color wheel divided into 12
    /// sections. This method allows provision of either of those values.</para>
    ///
    /// <para>Behavior is undefined when count or divisions is 0. When divisions &lt; count, colors repeat.</para>
    /// </summary>
    /// <param name="count">The number of colors to return, includes the input color.</param>
    /// <param name="divisions">The number of divisions on the color wheel.</param>
    public List<Hct> GetAnalogousColors(int count, int divisions)
    {
        // The starting hue is the hue of the input color.
        int startHue = (int)Math.Round(Input.Hue);
        Hct startHct = GetHctsByHue()[startHue];
        double lastTemp = GetRelativeTemperature(startHct);

        List<Hct> allColors = new()
        {
            startHct
        };

        double absoluteTotalTempDelta = 0.0;
        for (int i = 0; i < 360; i++)
        {
            int hue = Utils.MathUtils.SanitizeDegrees(startHue + i);
            Hct hct = GetHctsByHue()[hue];
            double temp = GetRelativeTemperature(hct);
            double tempDelta = Math.Abs(temp - lastTemp);
            lastTemp = temp;
            absoluteTotalTempDelta += tempDelta;
        }

        int hueAddend = 1;
        double tempStep = absoluteTotalTempDelta / divisions;
        double totalTempDelta = 0.0;
        lastTemp = GetRelativeTemperature(startHct);
        while (allColors.Count < divisions)
        {
            int hue = Utils.MathUtils.SanitizeDegrees(startHue + hueAddend);
            Hct hct = GetHctsByHue()[hue];
            double temp = GetRelativeTemperature(hct);
            double tempDelta = Math.Abs(temp - lastTemp);
            totalTempDelta += tempDelta;

            double desiredTotalTempDeltaForIndex = allColors.Count * tempStep;
            bool indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex;
            int indexAddend = 1;
            // Keep adding this hue to the answers until its temperature is
            // insufficient. This ensures consistent behavior when there aren't
            // `divisions` discrete steps between 0 and 360 in hue with `tempStep`
            // delta in temperature between them.
            //
            // For example, white and black have no analogues: there are no other
            // colors at T100/T0. Therefore, they should just be added to the array
            // as answers.
            while (indexSatisfied && allColors.Count < divisions)
            {
                allColors.Add(hct);
                desiredTotalTempDeltaForIndex = (allColors.Count + indexAddend) * tempStep;
                indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex;
                indexAddend++;
            }
            lastTemp = temp;
            hueAddend++;

            if (hueAddend > 360)
            {
                while (allColors.Count < divisions)
                {
                    allColors.Add(hct);
                }
                break;
            }
        }

        List<Hct> answers = new()
        {
            Input
        };

        int ccwCount = (int)Math.Floor((count - 1.0) / 2.0);
        for (int i = 1; i < ccwCount + 1; i++)
        {
            int index = 0 - i;
            while (index < 0)
            {
                index = allColors.Count + index;
            }
            if (index >= allColors.Count)
            {
                index %= allColors.Count;
            }
            answers.Insert(0, allColors[index]);
        }

        int cwCount = count - ccwCount - 1;
        for (int i = 1; i < cwCount + 1; i++)
        {
            int index = i;
            while (index < 0)
            {
                index = allColors.Count + index;
            }
            if (index >= allColors.Count)
            {
                index %= allColors.Count;
            }
            answers.Add(allColors[index]);
        }

        return answers;
    }

    /// <summary>
    /// Temperature relative to all colors with the same chroma and tone.
    /// </summary>
    /// <param name="hct">HCT to find the relative temperature of.</param>
    /// <returns>Value on a scale from 0 to 1.</returns>
    public double GetRelativeTemperature(Hct hct)
    {
        double range = GetTempsByHct()[GetWarmest()] - GetTempsByHct()[GetColdest()];
        double differenceFromColdest =
            GetTempsByHct()[hct] - GetTempsByHct()[GetColdest()];
        // Handle when there's no difference in temperature between warmest and
        // coldest: for example, at T100, only one color is available, white.
        if (range == 0.0)
        {
            return 0.5;
        }
        return differenceFromColdest / range;
    }

    /// <summary>
    /// Value representing cool-warm factor of a color. Values below 0 are considered cool, above,
    /// warm.
    ///
    /// <para>Color science has researched emotion and harmony, which art uses to select colors. Warm-cool
    /// is the foundation of analogous and complementary colors. See: - Li-Chen Ou's Chapter 19 in
    /// Handbook of Color Psychology (2015). - Josef Albers' Interaction of Color chapters 19 and 21.</para>
    ///
    /// <para>Implementation of Ou, Woodcock and Wright's algorithm, which uses Lab/LCH color space.
    /// Return value has these properties:</para>
    /// <list type="bullet">
    /// <item><description>Values below 0 are cool, above 0 are warm.</description></item>
    /// <item><description>Lower bound: -9.66. Chroma is infinite. Assuming max of Lab chroma 130.</description></item>
    /// <item><description>Upper bound: 8.61. Chroma is infinite. Assuming max of Lab chroma 130.</description></item>
    /// </list>
    /// </summary>
    public static double RawTemperature(Hct hct)
    {
        var color = hct.Color;
        var lab = color.ToLab();

        double hue = Utils.MathUtils.SanitizeDegrees(Math.Atan2(lab.B, lab.A) * 180.0 / Math.PI);
        double chroma = Math.Sqrt(lab.A * lab.A + lab.B * lab.B);
        return -0.5
            + 0.02
                * Math.Pow(chroma, 1.07)
                * Math.Cos(Utils.MathUtils.SanitizeDegrees(hue - 50.0) * Math.PI / 180.0);
    }

    /// <summary>
    /// Coldest color with same chroma and tone as input.
    /// </summary>
    public Hct GetColdest()
    {
        return GetHctsByTemp()[0];
    }

    /// <summary>
    /// HCTs for all colors with the same chroma/tone as the input.
    ///
    /// <para>Sorted by hue, ex. index 0 is hue 0.</para>
    /// </summary>
    public IReadOnlyList<Hct> GetHctsByHue()
    {
        if (PrecomputedHctsByHue != null)
        {
            return PrecomputedHctsByHue;
        }
        List<Hct> hcts = new();
        for (double hue = 0.0; hue <= 360.0; hue += 1.0)
        {
            Hct colorAtHue = Hct.Create(hue, Input.Chroma, Input.Tone);
            hcts.Add(colorAtHue);
        }
        PrecomputedHctsByHue = hcts;
        return PrecomputedHctsByHue;
    }

    /// <summary>
    /// HCTs for all colors with the same chroma/tone as the input.
    ///
    /// <para>Sorted from coldest first to warmest last.</para>
    /// </summary>
    public IReadOnlyList<Hct> GetHctsByTemp()
    {
        if (PrecomputedHctsByTemp != null)
        {
            return PrecomputedHctsByTemp;
        }

        List<Hct> hcts = new(GetHctsByHue())
        {
            Input
        };
        PrecomputedHctsByTemp = hcts.OrderBy(hct => GetTempsByHct()[hct]).ToList();
        return PrecomputedHctsByTemp;
    }

    /// <summary>
    /// Keys of HCTs in GetHctsByTemp, values of raw temperature.
    /// </summary>
    public IReadOnlyDictionary<Hct, double> GetTempsByHct()
    {
        if (PrecomputedTempsByHct != null)
        {
            return PrecomputedTempsByHct;
        }

        List<Hct> allHcts = new(GetHctsByHue())
        {
            Input
        };

        Dictionary<Hct, double> temperaturesByHct = new();
        foreach (Hct hct in allHcts)
        {
            temperaturesByHct[hct] = RawTemperature(hct);
        }

        PrecomputedTempsByHct = temperaturesByHct;
        return PrecomputedTempsByHct;
    }

    /// <summary>
    /// Warmest color with same chroma and tone as input.
    /// </summary>
    public Hct GetWarmest()
    {
        return GetHctsByTemp()[GetHctsByTemp().Count - 1];
    }

    /// <summary>
    /// Determines if an angle is between two other angles, rotating clockwise.
    /// </summary>
    public static bool IsBetween(double angle, double a, double b)
    {
        if (a < b)
        {
            return a <= angle && angle <= b;
        }
        return a <= angle || angle <= b;
    }

    public void EnterPool()
    {
        Input = null;
        PrecomputedComplement = null;
        PrecomputedHctsByTemp = null;
        PrecomputedHctsByHue = null;
        PrecomputedTempsByHct = null;
    }

    public void LeavePool()
    {
    }
}