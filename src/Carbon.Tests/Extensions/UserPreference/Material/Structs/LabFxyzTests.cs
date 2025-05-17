using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

[TestClass]
public class LabFxyzTests
{
    #region Construction

    [TestMethod]
    [DataRow(0.123123, 124302d, -2390432.39029423)]
    [DataRow(0, 0, 0)]
    [DataRow(1.51554, 0.5151869, 8.8541989)]
    [DataRow(2.5198756, 3.18919, 0.1111011)]
    public void Constructor_ShouldSetCorrectValues(double fx, double fy, double fz)
    {
        // Arrange & Act
        LabFxyz labFxyz = new(fx, fy, fz);

        // Assert
        Assert.AreEqual(fx, labFxyz.Fx);
        Assert.AreEqual(fy, labFxyz.Fy);
        Assert.AreEqual(fz, labFxyz.Fz);
    }

    #endregion
}
