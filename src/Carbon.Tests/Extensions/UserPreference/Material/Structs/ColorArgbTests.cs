using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Tests for the <see cref="StandardRgb"/> struct.
/// </summary>
[TestClass]
public class ColorArgbTests
{
    #region IsOpaque

    /// <summary>
    /// Tests that <see cref="ColorUtils.IsOpaque"/> correctly identifies fully opaque ARGB values.
    /// </summary>
    [TestMethod]
    [DataRow(0xFFFFFFFFu, true)]
    [DataRow(0x80FFFFFFu, false)]
    [DataRow(0x00FFFFFFu, false)]
    public void IsOpaque_ShouldReturnCorrectValue(uint argb, bool expected)
    {
        StandardRgb color = new(argb);
        Assert.AreEqual(expected, color.IsOpaque);
    }

    #endregion
}
