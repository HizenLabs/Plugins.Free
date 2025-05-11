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
    [DataRow(0xFFFFFFFFu, 95.0470, 100.0000, 108.8830)]
    [DataRow(0xFFFF0000u, 41.2456, 21.2673, 1.9334)]
    [DataRow(0xFF00FF00u, 35.7576, 71.5152, 11.9192)]
    [DataRow(0xFF0000FFu, 18.0437, 7.2175, 95.0304)]
    [DataRow(0xFF000000u, 0.0, 0.0, 0.0)]
    public void ToCieXyz_ShouldBeCorrect(uint color, double expectedX, double expectedY, double expectedZ)
    {
        // Arrange
        StandardRgb standardRgb = new(color);

        // Act
        CieXyz actualCieXyz = standardRgb.ToCieXyz();

        // Assert
        Assert.AreEqual(expectedX, actualCieXyz.X, 0.01);
        Assert.AreEqual(expectedY, actualCieXyz.Y, 0.01);
        Assert.AreEqual(expectedZ, actualCieXyz.Z, 0.01);
    }

    #endregion
}
