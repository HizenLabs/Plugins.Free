using HizenLabs.Extensions.UserPreference.Material.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Carbon.Tests.Extensions.UserPreference.Material.Utils;

/// <summary>
/// Unit tests for <see cref="MathUtils"/> static utility methods.
/// </summary>
[TestClass]
public class MathUtilsTests
{
    #region Lerp Tests

    /// <summary>
    /// Tests that <see cref="MathUtils.Lerp"/> correctly interpolates between two values.
    /// </summary>
    [TestMethod]
    [DataRow(5.0, 0, 1.0, 0.0)]
    [DataRow(0.0, 10.0, 0.5, 5.0)]
    [DataRow(-10.0, 10.0, 0.75, 5.0)]
    [DataRow(0.0, 1.0, 0.25, 0.25)]
    [DataRow(0.0, 1.0, 0.0, 0.0)]
    [DataRow(0.0, 1.0, 1.0, 1.0)]
    public void Lerp_ReturnsCorrectInterpolation(double start, double stop, double amount, double expected)
    {
        double result = MathUtils.Lerp(start, stop, amount);
        Assert.AreEqual(expected, result, 0.000001);
    }

    #endregion

    #region Clamp Tests

    /// <summary>
    /// Tests that <see cref="MathUtils.Clamp(int, int, int)"/> correctly clamps integer values.
    /// </summary>
    [TestMethod]
    [DataRow(0, 10, -5, 0)]
    [DataRow(0, 10, 5, 5)]
    [DataRow(0, 10, 15, 10)]
    [DataRow(-10, 0, -15, -10)]
    [DataRow(-10, 0, -5, -5)]
    [DataRow(-10, 0, 5, 0)]
    public void Clamp_Int_ReturnsCorrectValue(int min, int max, int input, int expected)
    {
        int result = MathUtils.Clamp(min, max, input);
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="MathUtils.Clamp(double, double, double)"/> correctly clamps double values.
    /// </summary>
    [TestMethod]
    [DataRow(0.0, 10.0, -5.0, 0.0)]
    [DataRow(0.0, 10.0, 5.0, 5.0)]
    [DataRow(0.0, 10.0, 15.0, 10.0)]
    [DataRow(-10.0, 0.0, -15.0, -10.0)]
    [DataRow(-10.0, 0.0, -5.0, -5.0)]
    [DataRow(-10.0, 0.0, 5.0, 0.0)]
    [DataRow(0.5, 1.5, 1.0, 1.0)]
    public void Clamp_Double_ReturnsCorrectValue(double min, double max, double input, double expected)
    {
        double result = MathUtils.Clamp(min, max, input);
        Assert.AreEqual(expected, result, 0.000001);
    }

    #endregion

    #region Sanitize Tests

    /// <summary>
    /// Tests that <see cref="MathUtils.SanitizeDegrees(int)"/> wraps angles correctly to the range [0, 360).
    /// </summary>
    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(90, 90)]
    [DataRow(359, 359)]
    [DataRow(360, 0)]
    [DataRow(361, 1)]
    [DataRow(720, 0)]
    [DataRow(-1, 359)]
    [DataRow(-90, 270)]
    [DataRow(-360, 0)]
    [DataRow(-361, 359)]
    public void SanitizeDegrees_Int_ReturnsCorrectValue(int degrees, int expected)
    {
        int result = MathUtils.SanitizeDegrees(degrees);
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="MathUtils.SanitizeDegrees(double)"/> wraps angles correctly to the range [0, 360).
    /// </summary>
    [TestMethod]
    [DataRow(0.0, 0.0)]
    [DataRow(90.0, 90.0)]
    [DataRow(359.5, 359.5)]
    [DataRow(360.0, 0.0)]
    [DataRow(361.5, 1.5)]
    [DataRow(720.0, 0.0)]
    [DataRow(-1.5, 358.5)]
    [DataRow(-90.0, 270.0)]
    [DataRow(-360.0, 0.0)]
    [DataRow(-361.5, 358.5)]
    public void SanitizeDegrees_Double_ReturnsCorrectValue(double degrees, double expected)
    {
        double result = MathUtils.SanitizeDegrees(degrees);
        Assert.AreEqual(expected, result, 0.000001);
    }

    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(Math.PI, Math.PI)]
    [DataRow(2 * Math.PI, 0)]
    [DataRow(-Math.PI, Math.PI)]
    [DataRow(-2 * Math.PI, 0)]
    [DataRow(3 * Math.PI, Math.PI)]
    [DataRow(-3 * Math.PI, Math.PI)]
    [DataRow(7 * Math.PI, Math.PI)]
    [DataRow(-7 * Math.PI, Math.PI)]
    public void SanitizeRadians_ReturnsValueWithin_0_To_2PI(double input, double expected)
    {
        // Act
        double result = MathUtils.SanitizeRadians(input);

        // Assert
        Assert.AreEqual(expected, result, 1e-10, $"Input: {input}, Expected: {expected}, Got: {result}");
    }

    #endregion

    #region RotationDirection Tests

    /// <summary>
    /// Tests that <see cref="MathUtils.RotationDirection"/> returns 1 or -1 depending on shortest angular path.
    /// </summary>
    [TestMethod]
    [DataRow(0.0, 90.0, 1.0)]
    [DataRow(0.0, 180.0, 1.0)]
    [DataRow(0.0, 181.0, -1.0)]
    [DataRow(0.0, 270.0, -1.0)]
    [DataRow(90.0, 0.0, -1.0)]
    [DataRow(270.0, 90.0, 1.0)]
    [DataRow(359.0, 1.0, 1.0)]
    [DataRow(1.0, 359.0, -1.0)]
    public void RotationDirection_ReturnsCorrectDirection(double from, double to, double expected)
    {
        double result = MathUtils.RotationDirection(from, to);
        Assert.AreEqual(expected, result, 0.000001);
    }

    #endregion

    #region DifferenceDegrees Tests

    /// <summary>
    /// Tests that <see cref="MathUtils.DifferenceDegrees"/> returns the smallest angle difference between two angles.
    /// </summary>
    [TestMethod]
    [DataRow(0.0, 0.0, 0.0)]
    [DataRow(0.0, 90.0, 90.0)]
    [DataRow(0.0, 180.0, 180.0)]
    [DataRow(0.0, 270.0, 90.0)]
    [DataRow(0.0, 360.0, 0.0)]
    [DataRow(90.0, 270.0, 180.0)]
    [DataRow(270.0, 90.0, 180.0)]
    [DataRow(359.0, 1.0, 2.0)]
    public void DifferenceDegrees_ReturnsCorrectDifference(double a, double b, double expected)
    {
        double result = MathUtils.DifferenceDegrees(a, b);
        Assert.AreEqual(expected, result, 0.000001);
    }

    #endregion

    #region Conversion Tests

    /// <summary>
    /// Tests that <see cref="MathUtils.ToDegrees"/> correctly converts radians to degrees.
    /// </summary>
    [TestMethod]
    [DataRow(0.0, 0.0)]
    [DataRow(Math.PI, 180.0)]
    [DataRow(Math.PI / 2, 90.0)]
    [DataRow(Math.PI / 4, 45.0)]
    [DataRow(-Math.PI, -180.0)]
    [DataRow(2 * Math.PI, 360.0)]
    public void ToDegrees_ReturnsCorrectValue(double radians, double expected)
    {
        double result = MathUtils.ToDegrees(radians);
        Assert.AreEqual(expected, result, 0.000001);
    }

    /// <summary>
    /// Tests that <see cref="MathUtils.ToRadians"/> correctly converts degrees to radians.
    /// </summary>
    [TestMethod]
    [DataRow(0.0, 0.0)]
    [DataRow(180.0, Math.PI)]
    [DataRow(90.0, Math.PI / 2)]
    [DataRow(45.0, Math.PI / 4)]
    [DataRow(-180.0, -Math.PI)]
    [DataRow(360.0, 2 * Math.PI)]
    public void ToRadians_ReturnsCorrectValue(double degrees, double expected)
    {
        double result = MathUtils.ToRadians(degrees);
        Assert.AreEqual(expected, result, 0.000001);
    }

    #endregion

    #region Miscellaneous Tests

    /// <summary>
    /// Tests that <see cref="MathUtils.Hypot"/> returns the correct length for the hypotenuse.
    /// </summary>
    [TestMethod]
    [DataRow(3.0, 4.0, 5.0)]
    [DataRow(5.0, 12.0, 13.0)]
    [DataRow(1.0, 1.0, 1.4142135623730951)]
    [DataRow(0.0, 5.0, 5.0)]
    [DataRow(7.0, 0.0, 7.0)]
    [DataRow(0.0, 0.0, 0.0)]
    public void Hypotenuse_ReturnsCorrectValue(double x, double y, double expected)
    {
        double result = MathUtils.Hypot(x, y);
        Assert.AreEqual(expected, result, 0.000001);
    }

    [TestMethod]
    [DataRow(0.0, 0.0)]
    [DataRow(1.0, 1.0)]
    [DataRow(8.0, 2.0)]
    [DataRow(27.0, 3.0)]
    [DataRow(-1.0, -1.0)]
    [DataRow(-8.0, -2.0)]
    [DataRow(-27.0, -3.0)]
    [DataRow(58.6, 3.88417872675)]
    public void Cbrt_ReturnsCorrectValue(double input, double expected)
    {
        double result = MathUtils.Cbrt(input);
        Assert.AreEqual(expected, result, 1e-10);
    }

    [TestMethod]
    [DataRow(0.0, 0.0)]
    [DataRow(1e-10, 9.999999999500001e-11)]
    [DataRow(1e-5, 9.999950000333332e-6)]
    [DataRow(0.01, 0.009950330853168092)]
    [DataRow(0.1, 0.0953101798043249)]
    [DataRow(1.0, 0.6931471805599453)]
    [DataRow(Math.E - 1, 1.0)]
    [DataRow(9.0, 2.302585092994046)]
    [DataRow(99.0, 4.605170185988092)]
    [DataRow(-0.5, -0.6931471805599453)]
    [DataRow(-0.9999999999, -23.02585084720009)]
    public void Log1p_ReturnsCorrectValue(double input, double expected)
    {
        double result = MathUtils.Log1p(input);
        Assert.AreEqual(expected, result, 1e-10);
    }


    #endregion
}
