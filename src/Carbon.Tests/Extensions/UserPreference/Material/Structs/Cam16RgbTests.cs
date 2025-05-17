using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Hct;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

[TestClass]
public class Cam16RgbTests
{
    #region Construction

    [TestMethod]
    public void Constructor_ShouldSetCorrectValues()
    {
        // Arrange
        double r = 10.0;
        double g = 20.0;
        double b = 30.0;

        // Act
        var cam16 = new Cam16Rgb(r, g, b);

        // Assert
        Assert.AreEqual(r, cam16.R, 1e-10);
        Assert.AreEqual(g, cam16.G, 1e-10);
        Assert.AreEqual(b, cam16.B, 1e-10);
    }

    #endregion
}
