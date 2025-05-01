using Carbon.Tests.Test.Base;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using UnityEngine;

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

    /// <summary>
    /// Tests the <see cref="BinaryWriterExtensions.Write(BinaryWriter, Vector2)"/> method to ensure it correctly writes a <see cref="Vector2"/> value to a binary stream.
    /// </summary>
    [TestMethod]
    public void WriteVector2_ShouldWriteCorrectVector2()
    {
        Vector2[] testValues = new Vector2[]
        {
            Vector2.zero,
            new(1, 2),
            new(float.MaxValue, float.MinValue)
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            Vector2 actual = new(_reader.ReadSingle(), _reader.ReadSingle());

            Assert.AreEqual(testValues[i], actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryWriterExtensions.Write(BinaryWriter, Vector3)"/> method to ensure it correctly writes a <see cref="Vector3"/> value to a binary stream.
    /// </summary>
    [TestMethod]
    public void WriteVector3_ShouldWriteCorrectVector3()
    {
        Vector3[] testValues = new Vector3[]
        {
            Vector3.zero,
            new(1, 2, 3),
            new(float.MaxValue, float.MinValue, 0)
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            Vector3 actual = new(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());

            Assert.AreEqual(testValues[i], actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryWriterExtensions.Write(BinaryWriter, Vector4)"/> method to ensure it correctly writes a <see cref="Vector4"/> value to a binary stream.
    /// </summary>
    [TestMethod]
    public void WriteVector4_ShouldWriteCorrectVector4()
    {
        Vector4[] testValues = new Vector4[]
        {
            Vector4.zero,
            new(1, 2, 3, 4),
            new(float.MaxValue, float.MinValue, -2, 4),
            new(-51665651.1f, -1.51586168f, -2155.56651f, -213.151651f)
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            Vector4 actual = new(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());

            Assert.AreEqual(testValues[i], actual);
        }
    }
}
