using Carbon.Tests.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Constants;

/// <summary>
/// We are just asserting that the generated conversion matrices are good.
/// </summary>
[TestClass]
public class ColorTransformsTests
{
    #region LinearRgb / CieXyz

    [TestMethod]
    public void LinearRgbToCieXyz_ShouldBeCorrect()
    {
        // Arrange
        ColorConversionMatrix expected = new
        (
            0.41233895, 0.35762064, 0.18051042,
            0.2126, 0.7152, 0.0722,
            0.01932141, 0.11916382, 0.95034478
        );

        // Act
        ColorConversionMatrix actual = ColorTransforms.LinearRgbToCieXyz;

        // Assert
        ColorConversionMatrixTests.AssertMatricesAreEqual(expected, actual, 1e-3);
    }

    [TestMethod]
    public void CieXyzToLinearRgb_ShouldBeCorrect()
    {
        // Arrange
        ColorConversionMatrix expected = new
        (
            3.2413774792388685, -1.5376652402851851, -0.49885366846268053,
            -0.9691452513005321, 1.8758853451067872, 0.04156585616912061,
            0.05562093689691305, -0.20395524564742123, 1.0571799111220335
        );

        // Act
        ColorConversionMatrix actual = ColorTransforms.CieXyzToLinearRgb;

        // Assert
        ColorConversionMatrixTests.AssertMatricesAreEqual(expected, actual, 1e-3);
    }

    #endregion

    #region CieXyz / Cam16PreAdaptRgb

    [TestMethod]
    public void CieXyzToCam16PreAdaptRgb_ShouldBeCorrect()
    {
        // Arrange
        ColorConversionMatrix expected = new
        (
            0.401288, 0.650173, -0.051461,
            -0.250268, 1.204414, 0.045854,
            -0.002079, 0.048952, 0.953127
        );

        // Act
        ColorConversionMatrix actual = ColorTransforms.CieXyzToCam16PreAdaptRgb;

        // Assert
        ColorConversionMatrixTests.AssertMatricesAreEqual(expected, actual, 1e-3);
    }

    [TestMethod]
    public void Cam16PreAdaptRgbToCieXyz_ShouldBeCorrect()
    {
        // Arrange
        ColorConversionMatrix expected = new
        (
            1.8620678, -1.0112547, 0.14918678,
            0.38752654, 0.62144744, -0.00897398,
            -0.01584150, -0.03412294, 1.0499644
        );

        // Act
        ColorConversionMatrix actual = ColorTransforms.Cam16PreAdaptRgbToCieXyz;

        // Assert
        ColorConversionMatrixTests.AssertMatricesAreEqual(expected, actual, 1e-3);
    }

    #endregion

    #region LinearRgb / Cam16ScaledDiscount

    [TestMethod]
    public void LinearRgbToCam16ScaledDiscount_ShouldBeCorrect()
    {
        // Arrange
        ColorConversionMatrix expected = new
        (
            0.001200833568784504, 0.002389694492170889, 0.0002795742885861124,
            0.0005891086651375999, 0.0029785502573438758, 0.0003270666104008398,
            0.00010146692491640572, 0.0005364214359186694, 0.0032979401770712076
        );

        // Act
        ColorConversionMatrix actual = ColorTransforms.LinearRgbToCam16ScaledDiscount;

        // Assert
        ColorConversionMatrixTests.AssertMatricesAreEqual(expected, actual, 1e-3);
    }

    [TestMethod]
    public void Cam16ScaledDiscountToLinearRgb_ShouldBeCorrect()
    {
        // Arrange
        ColorConversionMatrix expected = new
        (
            1373.2198709594231, -1100.4251190754821, -7.278681089101213,
            -271.815969077903, 559.6580465940733, -32.46047482791194,
            1.9622899599665666, -57.173814538844006, 308.7233197812385
        );

        // Act
        ColorConversionMatrix actual = ColorTransforms.Cam16ScaledDiscountToLinearRgb;

        // Assert
        ColorConversionMatrixTests.AssertMatricesAreEqual(expected, actual, 1e-3);
    }

    #endregion
}
