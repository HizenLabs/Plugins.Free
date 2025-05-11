using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

[TestClass]
public class WhitePointTests
{
    [TestMethod]
    [DataRow(WhitePoints.D65X, WhitePoints.D65Y, 95.047, 100d, 108.883)]
    [DataRow(0.31272, 0.32903, 95.047, 100d, 108.883)] // this should consolidate with D65 and is only here for assertion
    public void FromChromaticity_ShouldReturnCorrectWhitePoint(double x, double y, double expectedX, double expectedY, double expectedZ)
    {
        // Act
        WhitePoint whitePoint = WhitePoint.FromChromaticity(x, y);

        // Assert
        Assert.AreEqual(expectedX, whitePoint.X, 0.01);
        Assert.AreEqual(expectedY, whitePoint.Y, 0.01);
        Assert.AreEqual(expectedZ, whitePoint.Z, 0.01);

        // From what I can tell, these are just hard-coded from the wiki in the material-color-utilities implementation
        // As such, their values (which I use for assertion) are not actually derived from the x and y and end up being
        // off by a bit. So tolerance is 0.01 for that reason
    }
}
