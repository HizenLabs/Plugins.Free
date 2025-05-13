using HizenLabs.Extensions.UserPreference.Material.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

[TestClass]
public class Cam16PreAdaptRgbTests
{
    #region Construction

    [TestMethod]
    [DataRow(0.0, 0.0, 0.0)]
    [DataRow(0.25, 0.5, 0.75)]
    [DataRow(0.5, 0.5, 0.5)]
    [DataRow(0.75, 0.25, 0.5)]
    [DataRow(1.0, 1.0, 1.0)]
    public void Constructor_ShouldSetCorrectValues(double r, double g, double b)
    {
        // Arrange
        // Act
        var color = new Cam16PreAdaptRgb(r, g, b);

        // Assert
        Assert.AreEqual(r, color.R);
        Assert.AreEqual(g, color.G);
        Assert.AreEqual(b, color.B);
    }

    [TestMethod]
    [DataRow(0.0, 0.0, 0.0)]
    [DataRow(0.25, 0.5, 0.75)]
    [DataRow(0.5, 0.5, 0.5)]
    [DataRow(0.75, 0.25, 0.5)]
    [DataRow(1.0, 1.0, 1.0)]
    public void Constructor_Vector3d_ShouldSetCorrectValues(double x, double y, double z)
    {
        // Arrange
        var vector = new Vector3d(x, y, z);

        // Act
        var color = new Cam16PreAdaptRgb(vector);

        // Assert
        Assert.AreEqual(x, color.R);
        Assert.AreEqual(y, color.G);
        Assert.AreEqual(z, color.B);
    }

    #endregion

    #region Conversion

    [TestMethod]
    public void ToCam16Rgb_ShouldBeCorrect()
    {
        Assert.Fail("Test not implemented.");
    }

    [TestMethod]
    public void ToLinearRgb_ShouldBeCorrect()
    {
        Assert.Fail("Test not implemented.");
    }

    #endregion
}
