using HizenLabs.Extensions.UserPreference.Material.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

[TestClass]
public class Cam16ScaledDiscountRgbTests
{
    #region Construction

    [TestMethod]
    [DataRow(0d, 0d, 0d)]
    [DataRow(0.25d, 0.5d, 0.75d)]
    [DataRow(0.5d, 0.5d, 0.5d)]
    [DataRow(0.75d, 0.25d, 0.5d)]
    public void Constructor_ShouldSetCorrectValues(double r, double g, double b)
    {
        // Arrange
        // Act
        var color = new Cam16ScaledDiscountRgb(r, g, b);

        // Assert
        Assert.AreEqual(r, color.R);
        Assert.AreEqual(g, color.G);
        Assert.AreEqual(b, color.B);
    }

    #endregion

    #region Conversion

    [TestMethod]
    [DataRow(0d, 0d, 0d, 0d, 0d, 0d)]
    public void ToLinearRgb(double r, double g, double b, double linR, double linG, double linB)
    {
        // Arrange
        var color = new Cam16ScaledDiscountRgb(r, g, b);
        var expected = new LinearRgb(linR, linG, linB);

        // Act
        var actual = color.ToLinearRgb();

        // Assert
        Assert.AreEqual(expected.R, actual.R);
        Assert.AreEqual(expected.G, actual.G);
        Assert.AreEqual(expected.B, actual.B);
    }

    #endregion
}
