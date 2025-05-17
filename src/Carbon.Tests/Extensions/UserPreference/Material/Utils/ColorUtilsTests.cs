using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Carbon.Tests.Extensions.UserPreference.Material.Utils;

/// <summary>
/// Unit tests for <see cref="ColorUtils"/> covering conversions between ARGB, XYZ, and Lab color spaces.
/// </summary>
[TestClass]
public class ColorUtilsTests
{
    private readonly ViewingConditions _vc = ViewingConditions.Default;

    #region Lab

    [TestMethod]
    [DataRow((byte)0)]
    [DataRow((byte)12)]
    [DataRow((byte)30)]
    [DataRow((byte)75)]
    [DataRow((byte)128)]
    [DataRow((byte)200)]
    [DataRow((byte)255)]
    public void Rgb_RoundTrip_ReturnsOriginal(byte component)
    {
        double linear = ColorUtils.LinearizeComponent(component);
        byte result = ColorUtils.DelinearizeComponent(linear);

        Assert.AreEqual(component, result);
    }

    [TestMethod]
    [DataRow(0.0)]
    [DataRow(1.0)]
    [DataRow(10.0)]
    [DataRow(18.418)]
    [DataRow(50.0)]
    [DataRow(75.0)]
    [DataRow(100.0)]
    public void Lstar_RoundTrip_ReturnsOriginal(double lstar)
    {
        double y = ColorUtils.YFromLstar(lstar);
        double result = ColorUtils.LstarFromY(y);

        Assert.AreEqual(lstar, result, 1e-3);
    }

    [TestMethod]
    public void YFromLstar_ReturnsExpected()
    {
        double expected = 18.418;
        double actual = ColorUtils.YFromLstar(50.0);

        Assert.AreEqual(expected, actual, 1e-3);
    }

    [TestMethod]
    public void LstarFromY_ReturnsExpected()
    {
        double expected = 50.0;
        double actual = ColorUtils.LstarFromY(18.418);

        Assert.AreEqual(expected, actual, 1e-3);
    }

    #endregion

    #region Cam16 Conversions

    [TestMethod]
    public void PostAdaptationScale_Component_PositiveYieldsPositive()
    {
        double compressed = 20.0;
        double original = 30.0;

        double expected = Math.Sign(original) * Cam16Constants.MaxAdaptedResponse * compressed
                        / (compressed + Cam16Constants.AdaptedResponseOffset);
        double result = ColorUtils.PostAdaptationScale(compressed, original);

        Assert.AreEqual(expected, result, 1e-10);
    }

    [TestMethod]
    public void PostAdaptationScale_Component_NegativeYieldsNegative()
    {
        double compressed = 20.0;
        double original = -30.0;

        double result = ColorUtils.PostAdaptationScale(compressed, original);

        Assert.IsTrue(result < 0);
    }

    [TestMethod]
    public void PostAdaptationScale_Color_AppliesPerChannel()
    {
        var compressed = new Cam16Rgb(20, 30, 40);
        var original = new Cam16Rgb(-20, 30, -40);

        Cam16Rgb result = ColorUtils.PostAdaptationScale(compressed, original);

        Assert.IsTrue(result.R < 0);
        Assert.IsTrue(result.G > 0);
        Assert.IsTrue(result.B < 0);
    }

    #endregion
}
