using Carbon.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Carbon.Tests.Plugins;

[TestClass]
public class ChatMarkdownTests
{
    [TestMethod]
    [DataRow("\"\"", "\\\"\\\"")]
    [DataRow("\"Quote\"", "\\\"Quote\\\"")]
    [DataRow("\"Test", "\\\"Test")]
    [DataRow("", "")]
    [DataRow("Hello, World!", "Hello, World!")]
    [DataRow("Say \"Hello\"!", "Say \\\"Hello\\\"!")]
    [DataRow("Test \"", "Test \\\"")]
    public void Escape_ShouldReturnEscapedString(string message, string expected)
    {
        // Arrange
        var buffer = new char[message.Length * 2];
        var span = buffer.AsSpan(0, message.Length);
        message.AsSpan().CopyTo(span);
        
        // Act
        var result = ChatMarkdown.Escape(span, buffer);

        // Assert
        Assert.AreEqual(expected, result.ToString());
    }

    [TestMethod]
    [DataRow("\\\"\\\"", "\"\"")]
    [DataRow("\\\"Quote\\\"", "\"Quote\"")]
    [DataRow("\\\"Test", "\"Test")]
    [DataRow("", "")]
    [DataRow("Hello, World!", "Hello, World!")]
    [DataRow("Say \\\"Hello\\\"!", "Say \"Hello\"!")]
    [DataRow("Test \\\"", "Test \"")]
    public void Unescape_ShouldReturnUnescapedString(string message, string expected)
    {
        // Arrange
        var buffer = new char[message.Length];
        var span = buffer.AsSpan(0, message.Length);
        message.AsSpan().CopyTo(span);

        // Act
        var result = ChatMarkdown.Unescape(span);

        // Assert
        Assert.AreEqual(expected, result.ToString());
    }
}
