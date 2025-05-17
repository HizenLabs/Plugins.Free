using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.ColorSpaces;

[TestClass]
public class HtcTests
{
    [TestMethod]
    [DataRow(0xFF000000, 0.0, 0.0, 0.0)]
    [DataRow(0xFF0000FF, 282.79, 87.23, 32.30)]
    [DataRow(0xFF00CED1, 198.38, 51.75, 75.29)]
    [DataRow(0xFF00FF00, 142.14, 108.41, 87.74)]
    [DataRow(0xFF1E90FF, 259.32, 64.95, 59.38)]
    [DataRow(0xFF800080, 334.57, 70.27, 29.78)]
    [DataRow(0xFFB22222, 23.99, 77.17, 39.11)]
    [DataRow(0xFFFF0000, 27.41, 113.36, 53.23)]
    [DataRow(0xFFFFA500, 71.26, 60.53, 74.93)]

    public void Hct_Create_FromColor_ReturnsExpected(uint argb, double hue, double chroma, double tone)
    {
        // Arrange
        StandardRgb color = argb;

        // Act
        var hct = Hct.Create(color);

        // Assert
        const double precision = 1e-1;
        Assert.AreEqual(hue, hct.Hue, precision, $"Hue mismatch. Expected: {hue}, Actual: {hct.Hue}");
        Assert.AreEqual(chroma, hct.Chroma, precision, $"Chroma mismatch. Expected: {chroma}, Actual: {hct.Chroma}");
        Assert.AreEqual(tone, hct.Tone, precision, $"Tone mismatch. Expected: {tone}, Actual: {hct.Tone}");
    }
}
