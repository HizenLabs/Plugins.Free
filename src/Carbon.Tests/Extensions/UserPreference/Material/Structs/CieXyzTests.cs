using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

[TestClass]
public class CieXyzTests
{
    #region Chromaticity

    [TestMethod]
    [DataRow(WhitePoints.D65X, WhitePoints.D65Y, 1d, .95047, 1d, 1.08883)]
    [DataRow(0.31272, 0.32903, 1d, .95047, 1d, 1.08883)] // this should consolidate with D65 and is only here for assertion
    [DataRow(WhitePoints.D65X, WhitePoints.D65Y, 100d, 95.047, 100d, 108.883)]
    public void FromChromaticity_ShouldReturnCorrectWhitePoint(double x, double y, double luminance, double expectedX, double expectedY, double expectedZ)
    {
        // Act
        CieXyz whitePoint = CieXyz.FromChromaticity(x, y, luminance);

        // Assert
        var delta = 0.0001 * luminance;
        Assert.AreEqual(expectedX, whitePoint.X, delta);
        Assert.AreEqual(expectedY, whitePoint.Y, delta);
        Assert.AreEqual(expectedZ, whitePoint.Z, delta);
    }

    #endregion

    #region Conversions

    [TestMethod]
    [DataRow(0d, 0d, 0d, 0d, 0d, 0d)]
    [DataRow(0.3575761, 0.7151522, 0.119192, 0.6023701672, 0.7773668528, 0.1478797584)]
    [DataRow(0.95047, 1d, 1.08883, 0.97555292473, 1.01646898486, 1.0847692442)]
    public void ToCam16PreAdaptRgb_ShouldBeCorrect(double x, double y, double z, double cam16R, double cam16G, double cam16B)
    {
        // Arrange
        CieXyz cieXyz = new(x, y, z);
        Cam16PreAdaptRgb expected = new(cam16R, cam16G, cam16B);
        
        // Act
        Cam16PreAdaptRgb actualCam16 = cieXyz.ToCam16PreAdaptRgb();

        // Assert
        Assert.AreEqual(expected.R, actualCam16.R, 0.0001);
        Assert.AreEqual(expected.G, actualCam16.G, 0.0001);
        Assert.AreEqual(expected.B, actualCam16.B, 0.0001);
    }

    [TestMethod]
    [DataRow(0d, 0d, 0d, 0d, 0d, 0d)]
    [DataRow(.180437, .072175, .950304, 0d, 0d, 1d)] //~0000FF D65
    [DataRow(.357576, .715152, .119192, 0d, 1d, 0d)] //~00FF00 D65
    [DataRow(.412456, .212673, .019334, 1d, 0d, 0d)] //~FF0000 D65
    [DataRow(.950470, 1d, 1.088830, 1d, 1d, 1d)] // FFFFFF D65
    public void ToLinearRgb_ShouldBeCorrect(double x, double y, double z, double linR, double linG, double linB)
    {
        // Arrange
        CieXyz cieXyz = new(x, y, z);
        LinearRgb expected = new(linR, linG, linB);

        // Act
        LinearRgb actualLin = cieXyz.ToLinearRgb();

        // Assert
        Assert.AreEqual(expected.R, actualLin.R, 0.001);
        Assert.AreEqual(expected.G, actualLin.G, 0.001);
        Assert.AreEqual(expected.B, actualLin.B, 0.001);
    }

    [TestMethod]
    [DataRow(0d, 0d, 0d, 0xFF000000u)]
    [DataRow(18.04375, 7.21750, 95.03041, 0xFF0000FFu)]
    [DataRow(35.75761, 71.51522, 11.91920, 0xFF00FF00u)]
    [DataRow(41.24564, 21.26729, 1.93339, 0xFFFF0000u)]
    [DataRow(95.047, 100d, 108.883, 0xFFFFFFFFu)]
    public void ToStandardRgb_ShouldBeCorrect(double x, double y, double z, uint expectedArgb)
    {
        // Arrange
        CieXyz cieXyz = new(x, y, z);
        StandardRgb expected = new(expectedArgb);

        // Act
        StandardRgb actual = cieXyz.ToStandardRgb();

        // Assert
        Assert.AreEqual(expected.R, actual.R);
        Assert.AreEqual(expected.G, actual.G);
        Assert.AreEqual(expected.B, actual.B);
        Assert.AreEqual(expected.A, actual.A);
        Assert.AreEqual(expected.Value, actual.Value);
    }

    #endregion
}
