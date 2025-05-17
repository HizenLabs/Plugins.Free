using HizenLabs.Extensions.UserPreference.Material.Hct;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Htc;

[TestClass]
public class Cam16Tests
{
    /// <summary>
    /// Tests the conversion from ARGB to CAM16 color appearance model.
    /// Test data is pulled from the java implementation directly using
    /// a debug class and calculating the values.
    /// </summary>
    [DataTestMethod]
    [DataRow(0xFF000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000)]
    [DataRow(0xFF0000FF, 25.465629, 87.230694, 282.788180, 78.481410, 68.867116, 93.674751, 36.741968, 9.164285, -40.375305)]
    [DataRow(0xFF00FF00, 79.331577, 108.410061, 142.139894, 138.520049, 85.587858, 78.604926, 86.711153, -37.476894, 29.133081)]
    [DataRow(0xFF800080, 23.891540, 70.272616, 334.568034, 76.017164, 55.479008, 85.429627, 34.796264, 32.382326, -15.398404)]
    [DataRow(0xFFFF0000, 46.445185, 113.357887, 27.408225, 105.988792, 89.494082, 91.889775, 59.584819, 43.297655, 22.451259)]
    [DataRow(0xFFFFA500, 68.374123, 60.526207, 71.263591, 128.598374, 47.784388, 60.957237, 78.611204, 10.381962, 30.608125)]
    [DataRow(0xFFFFFFFF, 100.000000, 2.869035, 209.491959, 155.521198, 2.265053, 12.068254, 100.000000, -1.922337, -1.087250)]
    public void Cam16_FromColor_ReturnsExpected(
        uint argb,
        double expectedJ, double expectedC, double expectedH,
        double expectedQ, double expectedM, double expectedS,
        double expectedJstar, double expectedAstar, double expectedBstar)
    {
        var cam = Cam16.FromColor(argb);

        // It's not perfect, but that's okay.
        var precision = 1e-1;

        Assert.AreEqual(expectedJ, cam.Lightness, precision);
        Assert.AreEqual(expectedC, cam.Chroma, precision);
        Assert.AreEqual(expectedH, cam.Hue, precision);
        Assert.AreEqual(expectedQ, cam.Brightness, precision);
        Assert.AreEqual(expectedM, cam.Colorfulness, precision);
        Assert.AreEqual(expectedS, cam.Saturation, precision);
        Assert.AreEqual(expectedJstar, cam.JStar, precision);
        Assert.AreEqual(expectedAstar, cam.AStar, precision);
        Assert.AreEqual(expectedBstar, cam.BStar, precision);
    }

}
