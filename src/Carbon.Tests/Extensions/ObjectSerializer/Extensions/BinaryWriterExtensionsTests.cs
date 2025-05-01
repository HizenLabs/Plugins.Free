using Carbon.Tests.Test.Base;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Carbon.Tests.Extensions.ObjectSerializer.Extensions;

/// <summary>
/// Provides unit tests for the <see cref="BinaryWriterExtensions"/> class.
/// </summary>
[TestClass]
public class BinaryWriterExtensionsTests : BinaryReaderWriterTest
{
    /// <summary>
    /// Tests the <see cref="BinaryWriterExtensions.Write(BinaryWriter, DateTime)"/> method to ensure it correctly writes a <see cref="DateTime"/> value to a binary stream.
    /// </summary>
    [TestMethod]
    public void WriteDateTime_ShouldWriteCorrectDateTime()
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
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            long actual = _reader.ReadInt64();

            Assert.AreEqual(testValues[i].ToBinary(), actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryWriterExtensions.Write(BinaryWriter, TimeSpan)"/> method to ensure it correctly writes a <see cref="TimeSpan"/> value to a binary stream.
    /// </summary>
    [TestMethod]
    public void WriteTimeSpan_ShouldWriteCorrectTimeSpan()
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
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            long actual = _reader.ReadInt64();

            Assert.AreEqual(testValues[i].Ticks, actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryWriterExtensions.Write(BinaryWriter, Type)"/> method to ensure it correctly writes a <see cref="Type"/> value to a binary stream.
    /// </summary>
    [TestMethod]
    public void WriteType_ShouldWriteCorrectType()
    {
        Type[] testValues = new Type[]
        {
            typeof(int),
            typeof(string),
            typeof(DateTime)
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            string actual = _reader.ReadString();

            Assert.AreEqual(testValues[i].AssemblyQualifiedName, actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryWriterExtensions.Write(BinaryWriter, Guid)"/> method to ensure it correctly writes a <see cref="Guid"/> value to a binary stream.
    /// </summary>
    [TestMethod]
    public void WriteGuid_ShouldWriteCorrectGuid()
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
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            byte[] binary = _reader.ReadBytes(16);
            Guid actual = new(binary);

            Assert.AreEqual(testValues[i], actual);
        }
    }
}
