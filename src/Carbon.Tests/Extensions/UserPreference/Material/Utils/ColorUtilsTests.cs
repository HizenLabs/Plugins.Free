using HizenLabs.Extensions.UserPreference.Material.Constants;
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
    #region ArgbFromLinearArgb

    /// <summary>
    /// Tests that <see cref="ColorUtils.ArgbFromLinearArgb"/> correctly converts linear RGB to sRGB ARGB.
    /// </summary>
    [TestMethod]
    public void ArgbFromLinearArgb_ShouldConvertCorrectly()
    {
        var linearArgb = new LinearRgb(100.0, 0.0, 0.0);
        Assert.AreEqual(0xFFFF0000, ColorUtils.ArgbFromLinearArgb(linearArgb));

        linearArgb = new LinearRgb(0.0, 100.0, 0.0);
        Assert.AreEqual(0xFF00FF00, ColorUtils.ArgbFromLinearArgb(linearArgb));
    }

    #endregion

    #region ArgbFromXyz

    /// <summary>
    /// Tests that <see cref="ColorUtils.ArgbFromXyz"/> and vector variants convert from XYZ to ARGB correctly.
    /// </summary>
    [TestMethod]
    public void ArgbFromXyz_ShouldConvertCorrectly()
    {
        var whiteArgb = ColorUtils.ArgbFromXyz(WhitePoints.D65);
        Assert.AreEqual(255, whiteArgb.R);
        Assert.AreEqual(255, whiteArgb.G);
        Assert.AreEqual(255, whiteArgb.B);

        var redArgb = ColorUtils.ArgbFromXyz(new(41.24, 21.26, 1.93));
        Assert.AreEqual(255, redArgb.R);
        Assert.IsTrue(redArgb.G < 10);
        Assert.IsTrue(redArgb.B < 10);
    }

    #endregion

    #region XyzFromArgb

    /// <summary>
    /// Tests round-trip and specific RGB-to-XYZ conversion.
    /// </summary>
    [TestMethod]
    public void XyzFromArgb_ShouldConvertCorrectly()
    {
        var redXyz = ColorUtils.XyzFromArgb(new ColorArgb(255, 0, 0));
        Assert.AreEqual(41.24, redXyz.X, 0.1);
        Assert.AreEqual(21.26, redXyz.Y, 0.1);
        Assert.AreEqual(1.93, redXyz.Z, 0.1);

        var whiteArgb = new ColorArgb(255, 255, 255);
        var whiteXyz = ColorUtils.XyzFromArgb(whiteArgb);
        var backToWhite = ColorUtils.ArgbFromXyz(whiteXyz);

        Assert.AreEqual(255, backToWhite.R);
        Assert.AreEqual(255, backToWhite.G);
        Assert.AreEqual(255, backToWhite.B);
    }

    #endregion

    #region Lab Conversions

    /// <summary>
    /// Tests that <see cref="ColorUtils.ArgbFromLab"/> produces correct RGB approximations.
    /// </summary>
    [TestMethod]
    public void ArgbFromLab_ShouldConvertCorrectly()
    {
        var red = ColorUtils.ArgbFromLab(new(53.24, 80.09, 67.20));
        Assert.AreEqual(255, red.R, 5);
        Assert.IsTrue(red.G < 20);
        Assert.IsTrue(red.B < 20);

        var white = ColorUtils.ArgbFromLab(new(100, 0, 0));
        Assert.AreEqual(255, white.R, 5);
        Assert.AreEqual(255, white.G, 5);
        Assert.AreEqual(255, white.B, 5);
    }

    /// <summary>
    /// Tests that <see cref="ColorUtils.LabFromArgb"/> produces expected Lab values.
    /// </summary>
    [TestMethod]
    public void LabFromArgb_ShouldConvertCorrectly()
    {
        var redLab = ColorUtils.LabFromArgb(0xFFFF0000);
        Assert.AreEqual(53.24, redLab[0], 0.1);
        Assert.AreEqual(80.09, redLab[1], 0.1);
        Assert.AreEqual(67.20, redLab[2], 0.1);

        var whiteLab = ColorUtils.LabFromArgb(0xFFFFFFFF);
        Assert.AreEqual(100.0, whiteLab[0], 0.1);
        Assert.AreEqual(0.0, whiteLab[1], 0.1);
        Assert.AreEqual(0.0, whiteLab[2], 0.1);
    }

    #endregion

    #region L* Conversions

    [TestMethod]
    [DataRow(0.0, (byte)0)]
    [DataRow(50.0, (byte)119)]
    [DataRow(100.0, (byte)255)]
    public void ArgbFromLstar_ShouldConvertCorrectly(double lstar, byte expected)
    {
        var color = ColorUtils.ArgbFromLstar(lstar);
        Assert.AreEqual(expected, color.R, 1);
        Assert.AreEqual(expected, color.G, 1);
        Assert.AreEqual(expected, color.B, 1);
    }

    [TestMethod]
    [DataRow(0xFF000000, 0.0)]
    [DataRow(0xFF777777, 50.0, 1.0)]
    [DataRow(0xFFFFFFFF, 100.0)]
    public void LstarFromArgb_ShouldConvertCorrectly(uint argb, double expected, double delta = 0.1)
    {
        Assert.AreEqual(expected, ColorUtils.LstarFromArgb(argb), delta);
    }

    [TestMethod]
    [DataRow(0.0, 0.0)]
    [DataRow(50.0, 18.42)]
    [DataRow(100.0, 100.0)]
    public void YFromLstar_ShouldConvertCorrectly(double lstar, double expectedY, double delta = 0.1)
    {
        Assert.AreEqual(expectedY, ColorUtils.YFromLstar(lstar), delta);
    }

    [TestMethod]
    [DataRow(0.0, 0.0)]
    [DataRow(18.42, 50.0, 1.0)]
    [DataRow(100.0, 100.0)]
    public void LstarFromY_ShouldConvertCorrectly(double y, double expectedLstar, double delta = 0.1)
    {
        Assert.AreEqual(expectedLstar, ColorUtils.LstarFromY(y), delta);
    }

    #endregion

    #region Linearization & Delinearization

    [TestMethod]
    public void Linearized_ColorArgb_ShouldConvertCorrectly()
    {
        var black = ColorUtils.Linearized(new ColorArgb(0, 0, 0));
        Assert.AreEqual(0.0, black.R, 0.001);
        Assert.AreEqual(0.0, black.G, 0.001);
        Assert.AreEqual(0.0, black.B, 0.001);

        var white = ColorUtils.Linearized(new ColorArgb(255, 255, 255));
        Assert.AreEqual(100.0, white.R, 0.001);
        Assert.AreEqual(100.0, white.G, 0.001);
        Assert.AreEqual(100.0, white.B, 0.001);
    }

    [TestMethod]
    [DataRow((byte)0, 0.0)]
    [DataRow((byte)128, 21.59, 0.1)]
    [DataRow((byte)255, 100.0)]
    public void Linearized_ByteValue_ShouldConvertCorrectly(byte value, double expected, double delta = 0.001)
    {
        Assert.AreEqual(expected, ColorUtils.Linearized(value), delta);
    }

    [TestMethod]
    public void Delinearized_ColorXyz_ShouldConvertCorrectly()
    {
        var black = ColorUtils.Delinearized(new LinearRgb(0.0, 0.0, 0.0));
        Assert.AreEqual(0, black.R);
        Assert.AreEqual(0, black.G);
        Assert.AreEqual(0, black.B);

        var white = ColorUtils.Delinearized(new LinearRgb(100.0, 100.0, 100.0));
        Assert.AreEqual(255, white.R);
        Assert.AreEqual(255, white.G);
        Assert.AreEqual(255, white.B);
    }

    [TestMethod]
    [DataRow(0.0, (byte)0)]
    [DataRow(21.59, (byte)128, 5)]
    [DataRow(100.0, (byte)255)]
    public void Delinearized_DoubleValue_ShouldConvertCorrectly(double value, byte expected, int delta = 0)
    {
        Assert.AreEqual(expected, ColorUtils.Delinearized(value), delta);
    }

    #endregion

    #region LabF / LabInvF

    /// <summary>
    /// Tests that <see cref="ColorUtils.LabF(double)"/> behaves correctly above and below the epsilon threshold.
    /// </summary>
    [TestMethod]
    [DataRow(0.0, (Lab.Kappa * 0.0 + Lab.FOffset) / Lab.FScale)]
    [DataRow(Lab.Epsilon / 2, (Lab.Kappa * (Lab.Epsilon / 2) + Lab.FOffset) / Lab.FScale)]
    public void LabF_Scalar_ShouldBehaveAsExpected(double input, double expected)
    {
        double result = ColorUtils.LabF(input);
        Assert.AreEqual(expected, result, 0.0001);
    }

    /// <summary>
    /// Tests that <see cref="ColorUtils.LabF(double)"/> behaves correctly above and below the epsilon threshold.
    /// </summary>
    [TestMethod]
    public void LabF_Scalar_ShouldBehaveAsExpected()
    {
        // Below epsilon (nonlinear region)
        double inputBelow = Lab.Epsilon / 2;
        double expectedBelow = (Lab.Kappa * inputBelow + Lab.FOffset) / Lab.FScale;
        Assert.AreEqual(expectedBelow, ColorUtils.LabF(inputBelow), 0.0001);

        // Above epsilon (linear/cubic root region)
        double inputAbove = Lab.Epsilon * 2;
        double expectedAbove = Math.Pow(inputAbove, 1.0 / 3.0);
        Assert.AreEqual(expectedAbove, ColorUtils.LabF(inputAbove), 0.0001);

        // At zero
        Assert.AreEqual((Lab.Kappa * 0 + Lab.FOffset) / Lab.FScale, ColorUtils.LabF(0.0), 0.0001);
    }

    /// <summary>
    /// Tests that <see cref="ColorUtils.LabF(ColorXyz)"/> applies the transformation component-wise.
    /// </summary>
    [TestMethod]
    public void LabF_Vector_ShouldApplyToEachComponent()
    {
        var input = new ColorXyz(0.0, 1.0, Lab.Epsilon / 2.0);
        var expected = new ColorXyz(
            ColorUtils.LabF(input.X),
            ColorUtils.LabF(input.Y),
            ColorUtils.LabF(input.Z)
        );

        var result = ColorUtils.LabF(input);

        Assert.AreEqual(expected.X, result.X, 0.0001);
        Assert.AreEqual(expected.Y, result.Y, 0.0001);
        Assert.AreEqual(expected.Z, result.Z, 0.0001);
    }

    /// <summary>
    /// Tests that <see cref="ColorUtils.LabInvF(double)"/> is the inverse of LabF for representative inputs.
    /// </summary>
    [TestMethod]
    [DataRow(0.0)]
    [DataRow(0.5)]
    [DataRow(1.0)]
    [DataRow(2.0)]
    [DataRow(5.0)]
    public void LabF_And_InvF_ShouldRoundtrip(double input)
    {
        double f = ColorUtils.LabF(input);
        double roundtrip = ColorUtils.LabInvF(f);

        Assert.AreEqual(input, roundtrip, 0.0001);
    }

    #endregion

    #region Round-trip

    /// <summary>
    /// Tests that converting colors to XYZ and Lab and back results in visually equivalent output.
    /// </summary>
    [TestMethod]
    public void Roundtrip_Argb_Xyz_Lab_ShouldRemainVisuallyAccurate()
    {
        uint[] testColors =
        {
            0xFFFF0000, 0xFF00FF00, 0xFF0000FF,
            0xFFFFFF00, 0xFF00FFFF, 0xFFFF00FF,
            0xFF000000, 0xFFFFFFFF, 0xFF808080
        };

        foreach (var color in testColors)
        {
            var argb = new ColorArgb(color);
            var xyz = ColorUtils.XyzFromArgb(argb);
            var roundArgb = ColorUtils.ArgbFromXyz(xyz);

            Assert.AreEqual(argb.R, roundArgb.R, 2);
            Assert.AreEqual(argb.G, roundArgb.G, 2);
            Assert.AreEqual(argb.B, roundArgb.B, 2);

            var lab = ColorUtils.LabFromArgb(color);
            var labBack = ColorUtils.ArgbFromLab(lab);

            Assert.AreEqual(argb.R, labBack.R, 2);
            Assert.AreEqual(argb.G, labBack.G, 2);
            Assert.AreEqual(argb.B, labBack.B, 2);
        }
    }

    #endregion
}
