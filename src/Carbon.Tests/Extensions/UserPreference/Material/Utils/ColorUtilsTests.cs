using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Hct;
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

    #region Chromatic Adaptation

    [TestMethod]
    public void ApplyCompression_Component_Positive_ReturnsExpected()
    {
        double input = 50.0;
        double expected = Math.Pow(_vc.Fl * input / 100.0, Cam16Constants.NonlinearResponseExponent);
        double actual = ColorUtils.ApplyCompression(input, _vc.Fl);

        Assert.AreEqual(expected, actual, 1e-10);
    }

    [TestMethod]
    public void ApplyCompression_Component_Negative_IsSymmetric()
    {
        double pos = ColorUtils.ApplyCompression(50.0, _vc.Fl);
        double neg = ColorUtils.ApplyCompression(-50.0, _vc.Fl);

        Assert.AreEqual(pos, neg, 1e-10);
    }

    [TestMethod]
    public void ApplyCompression_Color_AllChannelsCompressed()
    {
        var input = new Cam16Rgb(20, -30, 40);
        Cam16Rgb result = ColorUtils.ApplyCompression(input, _vc.Fl);

        Assert.IsTrue(result.R > 0);
        Assert.IsTrue(result.G > 0);
        Assert.IsTrue(result.B > 0);
    }

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

    [TestMethod]
    public void ChromaticAdaptation_ValidColor_TransformsSuccessfully()
    {
        var input = new Cam16PreAdaptRgb(20, -30, 40);
        Cam16Rgb result = ColorUtils.ChromaticAdaptation(input, _vc);

        Assert.IsTrue(result.R >= -400 && result.R <= 400);
        Assert.IsTrue(result.G >= -400 && result.G <= 400);
        Assert.IsTrue(result.B >= -400 && result.B <= 400);
    }

    [TestMethod]
    public void ChromaticAdaptation_ZeroInput_ReturnsZero()
    {
        var input = new Cam16PreAdaptRgb(0, 0, 0);
        Cam16Rgb result = ColorUtils.ChromaticAdaptation(input, _vc);

        Assert.AreEqual(0, result.R, 1e-10);
        Assert.AreEqual(0, result.G, 1e-10);
        Assert.AreEqual(0, result.B, 1e-10);
    }

    #endregion
}
