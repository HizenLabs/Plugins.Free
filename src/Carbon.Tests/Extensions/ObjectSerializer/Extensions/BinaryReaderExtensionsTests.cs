using Carbon.Tests.Test.Base;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Carbon.Tests.Extensions.ObjectSerializer.Extensions;

/// <summary>
/// Provides unit tests for the <see cref="BinaryReaderExtensions"/> class.
/// </summary>
[TestClass]
public class BinaryReaderExtensionsTests : BinaryReaderWriterTest
{
    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadDateTime(BinaryReader)"/> method to ensure it correctly reads a <see cref="DateTime"/> value from a binary stream.
    /// </summary>
    [TestMethod]
    public void ReadDateTime_ShouldReturnCorrectDateTime()
    {
        DateTime[] testValues = new DateTime[]
        {
            DateTime.MinValue,
            new(2000, 1, 1),
            DateTime.MaxValue
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i].ToBinary());

            _memoryStream.Position = 0;
            DateTime actual = _reader.ReadDateTime();

            Assert.AreEqual(testValues[i], actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadTimeSpan(BinaryReader)"/> method to ensure it correctly reads a <see cref="TimeSpan"/> value from a binary stream.
    /// </summary>
    [TestMethod]
    public void ReadTimeSpan_ShouldReturnCorrectTimeSpan()
    {
        TimeSpan[] testValues = new TimeSpan[]
        {
            TimeSpan.Zero,
            TimeSpan.FromHours(1),
            TimeSpan.FromDays(1)
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i].Ticks);

            _memoryStream.Position = 0;
            TimeSpan actual = _reader.ReadTimeSpan();

            Assert.AreEqual(testValues[i], actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadType(BinaryReader)"/> method to ensure it correctly reads a <see cref="Type"/> value from a binary stream.
    /// </summary>
    [TestMethod]
    public void ReadType_ShouldReturnCorrectType()
    {
        Type[] testValues = new Type[]
        {
            typeof(string),
            typeof(int),
            typeof(DateTime)
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i].AssemblyQualifiedName);

            _memoryStream.Position = 0;
            Type actual = _reader.ReadType();

            Assert.AreEqual(testValues[i], actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadGuid(BinaryReader)"/> method to ensure it correctly reads a <see cref="Guid"/> value from a binary stream.
    /// </summary>
    [TestMethod]
    public void ReadGuid_ShouldReturnCorrectGuid()
    {
        Guid[] testValues = new Guid[]
        {
            Guid.Empty,
            Guid.NewGuid(),
            new("12345678-1234-1234-1234-123456789012")
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i].ToByteArray());

            _memoryStream.Position = 0;
            Guid actual = _reader.ReadGuid();

            Assert.AreEqual(testValues[i], actual);
        }
    }
}
