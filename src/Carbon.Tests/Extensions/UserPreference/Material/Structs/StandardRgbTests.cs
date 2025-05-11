using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Tests for the <see cref="StandardRgb"/> struct.
/// </summary>
[TestClass]
public class StandardRgbTests
{
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
    [DataRow(0xFF000000u, 0d, 0d, 0d)]
    [DataRow(0x00FF0000u, 100d, 0d, 0d)]
    [DataRow(0xFFFF0000u, 100d, 0d, 0d)]
    [DataRow(0x0000FF00u, 0d, 100d, 0d)]
    [DataRow(0xFF00FF00u, 0d, 100d, 0d)]
    [DataRow(0x000000FFu, 0d, 0d, 100d)]
    [DataRow(0xFF0000FFu, 0d, 0d, 100d)]
    [DataRow(0x00FFFFFFu, 100d, 100d, 100d)]
    [DataRow(0xFFFFFFFFu, 100d, 100d, 100d)]
    [DataRow(0x00C7A123u, 57.112482, 35.560014, 1.680737)]
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
    [DataRow(0xFFFFFFFFu, 95.0470, 100.0000, 108.8830)]
    [DataRow(0xFFFF0000u, 41.2456, 21.2673, 1.9334)]
    [DataRow(0xFF00FF00u, 35.7576, 71.5152, 11.9192)]
    [DataRow(0xFF0000FFu, 18.0437, 7.2175, 95.0304)]
    [DataRow(0xFF000000u, 0.0, 0.0, 0.0)]
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
