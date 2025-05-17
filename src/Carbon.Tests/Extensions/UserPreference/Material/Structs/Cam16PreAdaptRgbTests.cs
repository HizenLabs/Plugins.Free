using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

[TestClass]
public class Cam16PreAdaptRgbTests
{
    #region Construction

    [TestMethod]
    [DataRow(0.0, 0.0, 0.0)]
    [DataRow(0.25, 0.5, 0.75)]
    [DataRow(0.5, 0.5, 0.5)]
    [DataRow(0.75, 0.25, 0.5)]
    [DataRow(1.0, 1.0, 1.0)]
    public void Constructor_ShouldSetCorrectValues(double r, double g, double b)
    {
        // Arrange
        // Act
        var color = new Cam16PreAdaptRgb(r, g, b);

        // Assert
        Assert.AreEqual(r, color.R);
        Assert.AreEqual(g, color.G);
        Assert.AreEqual(b, color.B);
    }

    [TestMethod]
    [DataRow(0.0, 0.0, 0.0)]
    [DataRow(0.25, 0.5, 0.75)]
    [DataRow(0.5, 0.5, 0.5)]
    [DataRow(0.75, 0.25, 0.5)]
    [DataRow(1.0, 1.0, 1.0)]
    public void Constructor_Vector3d_ShouldSetCorrectValues(double x, double y, double z)
    {
        // Arrange
        var vector = new Vector3d(x, y, z);

        // Act
        var color = new Cam16PreAdaptRgb(vector);

        // Assert
        Assert.AreEqual(x, color.R);
        Assert.AreEqual(y, color.G);
        Assert.AreEqual(z, color.B);
    }

    #endregion

    #region Conversion

    [TestMethod]
    public void ToCam16Rgb_ShouldBeCorrect()
    {
        // Arrange
        var vc = ViewingConditions.Default;
        var rgbW = WhitePoints.D65.ToCam16PreAdaptRgb();

        // Act
        Cam16Rgb result = rgbW.ToCam16Rgb(vc);

        // Assert (matches Java's output)
        Assert.AreEqual(9.656952240716015, result.R, 1e-4, "R channel mismatch.");
        Assert.AreEqual(9.682086762132320, result.G, 1e-4, "G channel mismatch.");
        Assert.AreEqual(9.723831595138450, result.B, 1e-4, "B channel mismatch.");
    }

    [TestMethod]
    public void ToLinearRgb_ShouldBeCorrect()
    {
        // Arrange: known sRGB white point
        var argb = new StandardRgb(255, 255, 255);

        // Act
        var linear = argb.ToLinearRgb(); // or ToCieXyz, depending on your structure

        // Assert: verify conversion is accurate within expected tolerance
        Assert.AreEqual(100.0, linear.R, 1e-3, "R linear mismatch.");
        Assert.AreEqual(100.0, linear.G, 1e-3, "G linear mismatch.");
        Assert.AreEqual(100.0, linear.B, 1e-3, "B linear mismatch.");
    }

    #endregion
}
