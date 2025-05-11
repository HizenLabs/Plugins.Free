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
}
