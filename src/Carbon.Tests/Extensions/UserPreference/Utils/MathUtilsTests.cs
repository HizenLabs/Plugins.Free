using HizenLabs.Extensions.UserPreference.Material.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Utils;

/// <summary>
/// Tests for <see cref="MathUtils"/>.
/// </summary>
[TestClass]
public class MathUtilsTests
{
    #region Lerp

    [TestMethod]
    [DataRow(10f, 20f, 0f, 10f)]
    [DataRow(10f, 20f, 1f, 20f)]
    [DataRow(10f, 20f, 0.5f, 15f)]
    [DataRow(10f, 20f, -0.5f, 5f)]
    [DataRow(10f, 20f, 1.5f, 25f)]
    [DataRow(0f, 1f, 0.333f, 0.333f)]
    public void Lerp_ReturnsExpectedResult(float start, float stop, float amount, float expected)
    {
        float result = MathUtils.Lerp(start, stop, amount);
        Assert.AreEqual(expected, result);
    }

    #endregion

    #region SanitizeDegrees

    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(1, 1)]
    [DataRow(180, 180)]
    [DataRow(359, 359)]
    [DataRow(360, 0)]
    [DataRow(361, 1)]
    [DataRow(720, 0)]
    [DataRow(450, 90)]
    [DataRow(-1, 359)]
    [DataRow(-90, 270)]
    [DataRow(-180, 180)]
    [DataRow(-360, 0)]
    [DataRow(-361, 359)]
    [DataRow(-540, 180)]
    [DataRow(int.MaxValue, (int.MaxValue % 360 + 360) % 360)]
    public void SanitizeDegrees_Int_ReturnsCorrectValue(int input, int expected)
    {
        int result = MathUtils.SanitizeDegrees(input);

        Assert.AreEqual(expected, result, $"Failed for input: {input}");
    }

    [TestMethod]
    [DataRow(0f, 0f)]
    [DataRow(1.5f, 1.5f)]
    [DataRow(180.25f, 180.25f)]
    [DataRow(359.999f, 359.999f)]
    [DataRow(360f, 0f)]
    [DataRow(361.5f, 1.5f)]
    [DataRow(720f, 0f)]
    [DataRow(450.75f, 90.75f)]
    [DataRow(-0.5f, 359.5f)]
    [DataRow(-89.75f, 270.25f)]
    [DataRow(-179.5f, 180.5f)]
    [DataRow(-360f, 0f)]
    [DataRow(-361.25f, 358.75f)]
    [DataRow(-539.5f, 180.5f)]
    [DataRow(float.MaxValue, float.NaN)]
    public void SanitizeDegrees_Float_ReturnsCorrectValue(float input, float expected)
    {
        float result = MathUtils.SanitizeDegrees(input);

        if (float.IsNaN(expected))
        {
            Assert.IsTrue(result >= 0 && result < 360,
                $"Result {result} for input {input} should be between 0 and 360");
        }
        else
        {
            Assert.AreEqual(expected, result, 0.0001f, $"Failed for input: {input}");
        }
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(10)]
    [DataRow(90)]
    [DataRow(180)]
    [DataRow(270)]
    [DataRow(359)]
    [DataRow(360)]
    [DataRow(400)]
    [DataRow(-10)]
    [DataRow(-90)]
    [DataRow(-180)]
    [DataRow(-359)]
    [DataRow(-360)]
    [DataRow(-400)]
    public void SanitizeDegrees_IntAndFloat_Consistency(int value)
    {
        float floatResult = MathUtils.SanitizeDegrees((float)value);
        int intResult = MathUtils.SanitizeDegrees(value);

        Assert.AreEqual(intResult, floatResult, 0.0001f,
            $"Mismatch at value {value}: Int returned {intResult}, Float returned {floatResult}");
    }

    #endregion

    #region RotationDirection

    [TestMethod]
    [DataRow(0f, 90f, 1f)]
    [DataRow(0f, 180f, 1f)]
    [DataRow(0f, 270f, -1f)]
    [DataRow(270f, 90f, 1f)]
    [DataRow(90f, 270f, 1f)]
    [DataRow(45f, 45f, 1f)]
    [DataRow(359f, 1f, 1f)]
    [DataRow(1f, 359f, -1f)]
    public void RotationDirection_ReturnsExpected(float from, float to, float expected)
    {
        float result = MathUtils.RotationDirection(from, to);
        Assert.AreEqual(expected, result);
    }

    #endregion

    #region DifferenceDegrees

    [TestMethod]
    [DataRow(0f, 0f, 0f)]
    [DataRow(0f, 90f, 90f)]
    [DataRow(0f, 180f, 180f)]
    [DataRow(0f, 360f, 0f)]
    [DataRow(10f, 350f, 20f)]
    [DataRow(45f, 405f, 0f)]
    [DataRow(90f, 270f, 180f)]
    [DataRow(180f, 0f, 180f)]
    [DataRow(270f, 90f, 180f)]
    [DataRow(350f, 10f, 20f)]
    public void DifferenceDegrees_ReturnsExpected(float from, float to, float expected)
    {
        float result = MathUtils.DifferenceDegrees(from, to);
        Assert.AreEqual(expected, result);
    }

    #endregion
}
