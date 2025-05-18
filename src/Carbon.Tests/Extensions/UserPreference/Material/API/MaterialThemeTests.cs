using HizenLabs.Extensions.UserPreference.Material.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.API;

[TestClass]
public class MaterialThemeTests
{
    // Test data generated from: https://material-foundation.github.io/material-theme-builder/
    /// <summary>
    /// Test data created directly from the
    /// <a href="https://material-foundation.github.io/material-theme-builder">Material Theme Builder</a>
    /// </summary>
    [TestMethod]
    [DataRow("#63A002", false, MaterialContrast.Standard,
        "#4c662b", "#ffffff", "#cdeda3", "#354e16",
        "#586249", "#ffffff", "#dce7c8", "#404a33",
        "#386663", "#ffffff", "#bcece7", "#1f4e4b")]
    public void MaterialTheme_Create_ReturnsExpected(
        string seedHex,
        bool isDark,
        MaterialContrast contrast,

        string expectedPrimary,
        string expectedOnPrimary,
        string expectedPrimaryContainer,
        string expectedOnPrimaryContainer,

        string expectedSecondary,
        string expectedOnSecondary,
        string expectedSecondaryContainer,
        string expectedOnSecondaryContainer,

        string expectedTertiary,
        string expectedOnTertiary,
        string expectedTertiaryContainer,
        string expectedOnTertiaryContainer)
    {
        // Arrange
        var theme = MaterialTheme.Create(seedHex, isDark, contrast);

        // Act
        var primary = theme.Primary.ToRgbHex().ToLower();
        var onPrimary = theme.OnPrimary.ToRgbHex().ToLower();
        var primaryContainer = theme.PrimaryContainer.ToRgbHex().ToLower();
        var onPrimaryContainer = theme.OnPrimaryContainer.ToRgbHex().ToLower();

        var secondary = theme.Secondary.ToRgbHex().ToLower();
        var onSecondary = theme.OnSecondary.ToRgbHex().ToLower();
        var secondaryContainer = theme.SecondaryContainer.ToRgbHex().ToLower();
        var onSecondaryContainer = theme.OnSecondaryContainer.ToRgbHex().ToLower();

        var tertiary = theme.Tertiary.ToRgbHex().ToLower();
        var onTertiary = theme.OnTertiary.ToRgbHex().ToLower();
        var tertiaryContainer = theme.TertiaryContainer.ToRgbHex().ToLower();
        var onTertiaryContainer = theme.OnTertiaryContainer.ToRgbHex().ToLower();

        // Assert
        Assert.AreEqual(expectedPrimary, primary);
        Assert.AreEqual(expectedOnPrimary, onPrimary);
        Assert.AreEqual(expectedPrimaryContainer, primaryContainer);
        Assert.AreEqual(expectedOnPrimaryContainer, onPrimaryContainer);

        Assert.AreEqual(expectedSecondary, secondary);
        Assert.AreEqual(expectedOnSecondary, onSecondary);
        Assert.AreEqual(expectedSecondaryContainer, secondaryContainer);
        Assert.AreEqual(expectedOnSecondaryContainer, onSecondaryContainer);

        Assert.AreEqual(expectedTertiary, tertiary);
        Assert.AreEqual(expectedOnTertiary, onTertiary);
        Assert.AreEqual(expectedTertiaryContainer, tertiaryContainer);
        Assert.AreEqual(expectedOnTertiaryContainer, onTertiaryContainer);

    }
}
