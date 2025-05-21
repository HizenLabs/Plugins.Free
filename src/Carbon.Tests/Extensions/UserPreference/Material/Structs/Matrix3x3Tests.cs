using HizenLabs.Extensions.UserPreference.Material.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

[TestClass]
public class Matrix3x3Tests
{
    #region Indexer

    [TestMethod]
    public void Indexer_ShouldReturnCorrectValue()
    {
        // Arrange
        Matrix3x3 matrix = new
        (
            1, 2, 3,
            4, 5, 6,
            7, 8, 9
        );

        // Act & Assert
        Assert.AreEqual(1, matrix[0]);
        Assert.AreEqual(1, matrix[0, 0]);

        Assert.AreEqual(2, matrix[1]);
        Assert.AreEqual(2, matrix[0, 1]);

        Assert.AreEqual(3, matrix[2]);
        Assert.AreEqual(3, matrix[0, 2]);

        Assert.AreEqual(4, matrix[3]);
        Assert.AreEqual(4, matrix[1, 0]);

        Assert.AreEqual(5, matrix[4]);
        Assert.AreEqual(5, matrix[1, 1]);

        Assert.AreEqual(6, matrix[5]);
        Assert.AreEqual(6, matrix[1, 2]);

        Assert.AreEqual(7, matrix[6]);
        Assert.AreEqual(7, matrix[2, 0]);

        Assert.AreEqual(8, matrix[7]);
        Assert.AreEqual(8, matrix[2, 1]);

        Assert.AreEqual(9, matrix[8]);
        Assert.AreEqual(9, matrix[2, 2]);
    }

    [TestMethod]
    public void Indexer_InvalidIndex_ShouldThrowException()
    {
        // Arrange
        Matrix3x3 matrix = new
        (
            1, 2, 3,
            4, 5, 6,
            7, 8, 9
        );
        // Act & Assert
        Assert.ThrowsException<System.IndexOutOfRangeException>(() => _ = matrix[9]);
    }

    #endregion

    #region Inversion

    [TestMethod]
    public void Inversion_ShouldReturnCorrectMatrix()
    {
        // Arrange
        Matrix3x3 matrix = new
        (
            2, 1, 3,
            0, 2, 4,
            1, 1, 2
        );

        Matrix3x3 expected = new
        (
            0, -0.5, 1,
            -2, -0.5, 4,
            1, 0.5, -2
        );

        // Act
        Matrix3x3 actual = matrix.ToInverted();

        // Assert
        AssertMatricesAreEqual(expected, actual);
    }

    #endregion

    #region Test Helpers

    internal static void AssertMatricesAreEqual(Matrix3x3 expected, Matrix3x3 actual, double delta = 1e-3)
    {
        for (int i = 0; i < 9; i++)
        {
            Assert.AreEqual(expected[i], actual[i], delta, $"Matrix elements at index {i} do not match.");
        }
    }

    #endregion
}
