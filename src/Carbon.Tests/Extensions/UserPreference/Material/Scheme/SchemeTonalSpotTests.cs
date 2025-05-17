using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Scheme;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Scheme;

[TestClass]
public class SchemeTonalSpotTests
{
    [TestMethod]
    public void Create_FromColor_ReturnsExpected(
        uint argb,
        bool isDark,
        double contrastLevel,
        uint primary,
        uint secondary,
        uint tertiary,
        uint neutral,
        uint neutralVariant,
        uint error)
    {
        // Arrange
        var color = argb;
        var hct = Hct.Create(color);

        // Act
        var scheme = SchemeTonalSpot.Create(hct, isDark, contrastLevel);

        Assert.IsNotNull(scheme);
    }
}
