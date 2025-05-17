using HizenLabs.Extensions.UserPreference.Material.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

[TestClass]
public class LabTests
{
    #region Construction

    [TestMethod]
    [DataRow(0d, 0d, 0d)]
    [DataRow(0.25d, 0.5d, 0.75d)]
    [DataRow(0.5d, 0.5d, 0.5d)]
    [DataRow(0.75d, 0.25d, 0.5d)]
    public void Constructor_ShouldSetCorrectValues(double l, double a, double b)
    {
        // Arrange
        // Act
        var color = new Lab(l, a, b);

        // Assert
        Assert.AreEqual(l, color.L);
        Assert.AreEqual(a, color.A);
        Assert.AreEqual(b, color.B);
    }

    #endregion
}
