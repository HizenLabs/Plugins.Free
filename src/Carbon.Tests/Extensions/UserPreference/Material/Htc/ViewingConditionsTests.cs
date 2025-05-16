using HizenLabs.Extensions.UserPreference.Material.Hct;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Hct;

[TestClass]
public class ViewingConditionsTests
{
    [TestMethod]
    public void Default_ViewingConditions_AreCorrect()
    {
        // Expected values derived from the official Java Material Color Utilities implementation
        var expectedN = 0.18418651851244416;
        var expectedAw = 29.980997194447333;
        var expectedNbb = 1.0169191804458755;
        var expectedNcb = 1.0169191804458755;
        var expectedC = 0.69;
        var expectedNc = 1.0;
        var expectedFl = 0.3884814537800353;
        var expectedFlRoot = 0.7894826179304937;
        var expectedZ = 1.909169568483652;
        var expectedRgbD = new[] { 1.02117770275752, 0.9863077294280124, 0.9339605082802299 };

        ViewingConditions vc = ViewingConditions.Default;

        const double precision = 1e-4;

        Assert.AreEqual(expectedN, vc.N, precision, $"N mismatch.      Expected: {expectedN},      Actual: {vc.N}");
        Assert.AreEqual(expectedAw, vc.Aw, precision, $"Aw mismatch.     Expected: {expectedAw},     Actual: {vc.Aw}");
        Assert.AreEqual(expectedNbb, vc.Nbb, precision, $"Nbb mismatch.    Expected: {expectedNbb},    Actual: {vc.Nbb}");
        Assert.AreEqual(expectedNcb, vc.Ncb, precision, $"Ncb mismatch.    Expected: {expectedNcb},    Actual: {vc.Ncb}");
        Assert.AreEqual(expectedC, vc.C, precision, $"C mismatch.      Expected: {expectedC},      Actual: {vc.C}");
        Assert.AreEqual(expectedNc, vc.Nc, precision, $"Nc mismatch.     Expected: {expectedNc},     Actual: {vc.Nc}");
        Assert.AreEqual(expectedFl, vc.Fl, precision, $"Fl mismatch.     Expected: {expectedFl},     Actual: {vc.Fl}");
        Assert.AreEqual(expectedFlRoot, vc.FlRoot, precision, $"FlRoot mismatch. Expected: {expectedFlRoot}, Actual: {vc.FlRoot}");
        Assert.AreEqual(expectedZ, vc.Z, precision, $"Z mismatch.      Expected: {expectedZ},      Actual: {vc.Z}");
        Assert.AreEqual(expectedRgbD[0], vc.RgbD.R, precision, $"RgbD.R mismatch. Expected: {expectedRgbD[0]}, Actual: {vc.RgbD.R}");
        Assert.AreEqual(expectedRgbD[1], vc.RgbD.G, precision, $"RgbD.G mismatch. Expected: {expectedRgbD[1]}, Actual: {vc.RgbD.G}");
        Assert.AreEqual(expectedRgbD[2], vc.RgbD.B, precision, $"RgbD.B mismatch. Expected: {expectedRgbD[2]}, Actual: {vc.RgbD.B}");
    }
}
