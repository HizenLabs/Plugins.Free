using HizenLabs.Extensions.UserPreference.Material.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Utils;

[TestClass]
public class StringUtilsTests
{
    [TestMethod]
    [DataRow(0x00000000u, "#000000")]
    [DataRow(0xFFFF0000u, "#FF0000")]
    [DataRow(0xFF00FF00u, "#00FF00")]
    [DataRow(0xFF0000FFu, "#0000FF")]
    [DataRow(0x80808080u, "#808080")]
    [DataRow(0x00FFFFFFu, "#FFFFFF")]
    [DataRow(0x518CC5D1u, "#8CC5D1")]
    [DataRow(0x518CC5D1u, "#8CC5D1")]
    [DataRow(0xFFFFFFFFu, "#FFFFFF")]
    public void HexFromArgb_ReturnsExpectedHex(uint argb, string expected)
    {
        string hex = StringUtils.HexFromArgb(argb);
        Assert.AreEqual(expected, hex);
    }
}
