﻿using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Tests for the <see cref="StandardRgb"/> struct.
/// </summary>
[TestClass]
public class StandardRgbTests
{
    #region Construction

    [TestMethod]
    [DataRow(0x00000000u, 0, 0, 0, 0)]
    [DataRow(0x00FF1234u, 0, 255, 18, 52)]
    [DataRow(0x00FFFFFFu, 0, 255, 255, 255)]
    [DataRow(0x80FF1234u, 128, 255, 18, 52)]
    [DataRow(0xFF000000u, 255, 0, 0, 0)]
    [DataRow(0xFF0000FFu, 255, 0, 0, 255)]
    [DataRow(0xFF00FF00u, 255, 0, 255, 0)]
    [DataRow(0xFF123456u, 255, 18, 52, 86)]
    [DataRow(0xFFFF0000u, 255, 255, 0, 0)]
    [DataRow(0xFFFFFFFFu, 255, 255, 255, 255)]
    public void Constructor_ShouldSetCorrectValues(uint argb, int expectedA, int expectedR, int expectedG, int expectedB)
    {
        // Arrange

        // Act
        StandardRgb color = argb;

        // Assert
        Assert.AreEqual(argb, color.Value);
        Assert.AreEqual(expectedA, color.A);
        Assert.AreEqual(expectedR, color.R);
        Assert.AreEqual(expectedG, color.G);
        Assert.AreEqual(expectedB, color.B);
    }

    [TestMethod]
    [DataRow(0, 0, 0)]
    [DataRow(0, 0, 255)]
    [DataRow(0, 255, 0)]
    [DataRow(0, 255, 255)]
    [DataRow(125, 252, 35)]
    [DataRow(255, 0, 0)]
    [DataRow(255, 0, 255)]
    [DataRow(255, 255, 0)]
    [DataRow(255, 255, 255)]
    public void Constructor_Rgb_ShouldSetCorrectValues(int r, int g, int b)
    {
        // Arrange
        byte byteR = (byte)r;
        byte byteG = (byte)g;
        byte byteB = (byte)b;

        // Act
        StandardRgb color = new(byteR, byteG, byteB);

        // Assert
        Assert.AreEqual(255, color.A);
        Assert.AreEqual(r, color.R);
        Assert.AreEqual(g, color.G);
        Assert.AreEqual(b, color.B);
    }

    [TestMethod]
    [DataRow(0, 0, 0, 0)]
    [DataRow(0, 125, 252, 35)]
    [DataRow(255, 0, 0, 0)]
    [DataRow(255, 0, 0, 255)]
    [DataRow(255, 0, 255, 0)]
    [DataRow(255, 0, 255, 255)]
    [DataRow(255, 255, 0, 0)]
    [DataRow(255, 255, 0, 255)]
    [DataRow(255, 255, 255, 0)]
    [DataRow(255, 255, 255, 255)]
    public void Constructor_Argb_ShouldSetCorrectValues(int a, int r, int g, int b)
    {
        // Arrange
        byte byteA = (byte)a;
        byte byteR = (byte)r;
        byte byteG = (byte)g;
        byte byteB = (byte)b;

        // Act
        StandardRgb color = new(byteA, byteR, byteG, byteB);

        // Assert
        Assert.AreEqual(a, color.A);
        Assert.AreEqual(r, color.R);
        Assert.AreEqual(g, color.G);
        Assert.AreEqual(b, color.B);
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
        StandardRgb color = new(argb);
        Assert.AreEqual(expected, color.IsOpaque);
    }

    #endregion

    #region Conversions

    [TestMethod]
    [DataRow(0x00000000u, 0d, 0d, 0d)]
    [DataRow(0x000000FFu, 0d, 0d, 100d)]
    [DataRow(0x0000FF00u, 0d, 100d, 0d)]
    [DataRow(0x00C7A123u, 57.112482, 35.560014, 1.680737)]
    [DataRow(0x00FF0000u, 100d, 0d, 0d)]
    [DataRow(0x00FFFFFFu, 100d, 100d, 100d)]
    [DataRow(0xFF000000u, 0d, 0d, 0d)]
    [DataRow(0xFF0000FFu, 0d, 0d, 100d)]
    [DataRow(0xFF00FF00u, 0d, 100d, 0d)]
    [DataRow(0xFFFF0000u, 100d, 0d, 0d)]
    [DataRow(0xFFFFFFFFu, 100d, 100d, 100d)]
    public void ToLinearRgb_ShouldBeCorrect(uint color, double expectedR, double expectedG, double expectedB)
    {
        // Arrange
        StandardRgb standardRgb = color;

        // Act
        LinearRgb actualLinearRgb = standardRgb.ToLinearRgb();

        // Assert
        Assert.AreEqual(expectedR, actualLinearRgb.R, 0.1);
        Assert.AreEqual(expectedG, actualLinearRgb.G, 0.1);
        Assert.AreEqual(expectedB, actualLinearRgb.B, 0.1);
    }

    [TestMethod]
    [DataRow(0xFF000000u, 0.0, 0.0, 0.0)]
    [DataRow(0xFF0000FFu, 18.0437, 7.2175, 95.0304)]
    [DataRow(0xFF00FF00u, 35.7576, 71.5152, 11.9192)]
    [DataRow(0xFFFF0000u, 41.2456, 21.2673, 1.9334)]
    [DataRow(0xFFFFFFFFu, 95.0470, 100.0000, 108.8830)]
    public void ToCieXyz_ShouldBeCorrect(uint color, double expectedX, double expectedY, double expectedZ)
    {
        // Arrange
        StandardRgb standardRgb = color;

        // Act
        CieXyz actualCieXyz = standardRgb.ToCieXyz();

        // Assert
        Assert.AreEqual(expectedX, actualCieXyz.X, 0.1);
        Assert.AreEqual(expectedY, actualCieXyz.Y, 0.1);
        Assert.AreEqual(expectedZ, actualCieXyz.Z, 0.1);
    }

    #endregion
}
