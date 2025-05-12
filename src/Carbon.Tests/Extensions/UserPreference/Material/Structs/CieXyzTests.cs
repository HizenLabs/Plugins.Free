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

    #endregion
}
