using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Utils;

/// <summary>
/// Unit tests for <see cref="ColorUtils"/> covering conversions between ARGB, XYZ, and Lab color spaces.
/// </summary>
[TestClass]
public class ColorUtilsTests
{
    #region ArgbFromRgb

    /// <summary>
    /// Tests that <see cref="ColorUtils.ArgbFromRgb"/> packs RGB into ARGB with full alpha.
    /// </summary>
    [TestMethod]
    [DataRow((byte)255, (byte)0, (byte)0, (uint)0xFFFF0000)]
    [DataRow((byte)0, (byte)255, (byte)0, (uint)0xFF00FF00)]
    [DataRow((byte)0, (byte)0, (byte)255, (uint)0xFF0000FF)]
    [DataRow((byte)255, (byte)255, (byte)255, (uint)0xFFFFFFFF)]
    [DataRow((byte)0, (byte)0, (byte)0, (uint)0xFF000000)]
    [DataRow((byte)128, (byte)128, (byte)128, (uint)0xFF808080)]
    public void ArgbFromRgb_ShouldReturnCorrectArgb(byte red, byte green, byte blue, uint expected)
    {
        uint result = ColorUtils.ArgbFromRgb(red, green, blue);
        Assert.AreEqual(expected, result);
    }

    #endregion

    #region ArgbFromLinearArgb

    /// <summary>
    /// Tests that <see cref="ColorUtils.ArgbFromLinearArgb"/> correctly converts linear RGB to sRGB ARGB.
    /// </summary>
    [TestMethod]
    public void ArgbFromLinearArgb_ShouldConvertCorrectly()
    {
        var linearArgb = new ColorXyz(100.0, 0.0, 0.0);
        Assert.AreEqual(0xFFFF0000, ColorUtils.ArgbFromLinearArgb(linearArgb));

        linearArgb = new ColorXyz(0.0, 100.0, 0.0);
        Assert.AreEqual(0xFF00FF00, ColorUtils.ArgbFromLinearArgb(linearArgb));
    }

    #endregion

    #region IsOpaque

    /// <summary>
    /// Tests that <see cref="ColorUtils.IsOpaque"/> correctly identifies fully opaque ARGB values.
    /// </summary>
    [TestMethod]
    [DataRow(0xFFFFFFFFu, true)]
    [DataRow(0x80FFFFFFu, false)]
    [DataRow(0x00FFFFFFu, false)]
    public void IsOpaque_ShouldReturnCorrectValue(uint argb, bool expected)
    {
        Assert.AreEqual(expected, ColorUtils.IsOpaque(argb));
    }

    #endregion

    #region ArgbFromXyz

    /// <summary>
    /// Tests that <see cref="ColorUtils.ArgbFromXyz"/> and vector variants convert from XYZ to ARGB correctly.
    /// </summary>
    [TestMethod]
    public void ArgbFromXyz_ShouldConvertCorrectly()
    {
        var whiteArgb = ColorUtils.ArgbFromXyz(WhitePoints.D65.X, WhitePoints.D65.Y, WhitePoints.D65.Z);
        Assert.AreEqual(255, whiteArgb.R);
        Assert.AreEqual(255, whiteArgb.G);
        Assert.AreEqual(255, whiteArgb.B);

        var redArgb = ColorUtils.ArgbFromXyz(41.24, 21.26, 1.93);
        Assert.AreEqual(255, redArgb.R);
        Assert.IsTrue(redArgb.G < 10);
        Assert.IsTrue(redArgb.B < 10);
    }

    [TestMethod]
    public void ArgbFromXyzVector_ShouldConvertCorrectly()
    {
        var xyz = new ColorXyz(41.24, 21.26, 1.93);
        var argb = ColorUtils.ArgbFromXyzVector(xyz);

        Assert.AreEqual(255, argb.R);
        Assert.IsTrue(argb.G < 10);
        Assert.IsTrue(argb.B < 10);

        var argb2 = ColorUtils.ArgbFromXyz(41.24, 21.26, 1.93);
        Assert.AreEqual(argb.Value, argb2.Value);
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
        var backToWhite = ColorUtils.ArgbFromXyzVector(whiteXyz);

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
        var red = ColorUtils.ArgbFromLab(53.24, 80.09, 67.20);
        Assert.AreEqual(255, red.R, 5);
        Assert.IsTrue(red.G < 20);
        Assert.IsTrue(red.B < 20);

        var white = ColorUtils.ArgbFromLab(100, 0, 0);
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
        Assert.AreEqual(0.0, black.X, 0.001);
        Assert.AreEqual(0.0, black.Y, 0.001);
        Assert.AreEqual(0.0, black.Z, 0.001);

        var white = ColorUtils.Linearized(new ColorArgb(255, 255, 255));
        Assert.AreEqual(100.0, white.X, 0.001);
        Assert.AreEqual(100.0, white.Y, 0.001);
        Assert.AreEqual(100.0, white.Z, 0.001);
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
        var black = ColorUtils.Delinearized(new ColorXyz(0.0, 0.0, 0.0));
        Assert.AreEqual(0, black.R);
        Assert.AreEqual(0, black.G);
        Assert.AreEqual(0, black.B);

        var white = ColorUtils.Delinearized(new ColorXyz(100.0, 100.0, 100.0));
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
            var roundArgb = ColorUtils.ArgbFromXyzVector(xyz);

            Assert.AreEqual(argb.R, roundArgb.R, 2);
            Assert.AreEqual(argb.G, roundArgb.G, 2);
            Assert.AreEqual(argb.B, roundArgb.B, 2);

            var lab = ColorUtils.LabFromArgb(color);
            var labBack = ColorUtils.ArgbFromLab(lab[0], lab[1], lab[2]);

            Assert.AreEqual(argb.R, labBack.R, 2);
            Assert.AreEqual(argb.G, labBack.G, 2);
            Assert.AreEqual(argb.B, labBack.B, 2);
        }
    }

    #endregion
}
