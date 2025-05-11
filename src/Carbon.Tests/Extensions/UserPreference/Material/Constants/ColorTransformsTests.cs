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
}
