using Carbon.Tests.Test.Base;
using Epic.OnlineServices;
using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Carbon.Tests.Extensions.ObjectSerializer.Extensions;

/// <summary>
/// Provides unit tests for the <see cref="BinaryReaderExtensions"/> class.
/// </summary>
[TestClass]
public class BinaryReaderExtensionsTests : BinaryReaderWriterTest
{
    #region System

    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadEnum{TEnum}(BinaryReader)"/> method to ensure it correctly reads an <see cref="Enum"/> value from a binary stream.
    /// </summary>
    [TestMethod]
    public void ReadEnum_ShouldReturnCorrectEnum()
    {
        _memoryStream.SetLength(0);
        _writer.Write(3);

        _memoryStream.Position = 0;
        var actualDayOfWeek = _reader.ReadEnum<DayOfWeek>();

        Assert.AreEqual(DayOfWeek.Wednesday, actualDayOfWeek);

        _memoryStream.Position = 0;
        var actualOperatingSystemFamily = _reader.ReadEnum<OperatingSystemFamily>();

        Assert.AreEqual(OperatingSystemFamily.Linux, actualOperatingSystemFamily);

        _memoryStream.Position = 0;
        var actualPlatformID = _reader.ReadEnum<PlatformID>();

        Assert.AreEqual(PlatformID.WinCE, actualPlatformID);
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

    #endregion

    #region UnityEngine

    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadVector2(BinaryReader)"/> method to ensure it correctly reads a <see cref="Vector2"/> value from a binary stream.
    /// </summary>
    [TestMethod]
    public void ReadVector2_ShouldReturnCorrectVector2()
    {
        Vector2[] testValues = new Vector2[]
        {
            Vector2.zero,
            new(1, 1),
            new(float.MaxValue, float.MinValue)
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            Vector2 actual = _reader.ReadVector2();

            Assert.AreEqual(testValues[i], actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadVector3(BinaryReader)"/> method to ensure it correctly reads a <see cref="Vector3"/> value from a binary stream.
    /// </summary>
    [TestMethod]
    public void ReadVector3_ShouldReturnCorrectVector3()
    {
        Vector3[] testValues = new Vector3[]
        {
            Vector3.zero,
            new(1, 1, 1),
            new(float.MaxValue, float.MinValue, 0)
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            Vector3 actual = _reader.ReadVector3();

            Assert.AreEqual(testValues[i], actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadVector4(BinaryReader)"/> method to ensure it correctly reads a <see cref="Vector4"/> value from a binary stream.
    /// </summary>
    [TestMethod]
    public void ReadVector4_ShouldReturnCorrectVector4()
    {
        Vector4[] testValues = new Vector4[]
        {
            Vector4.zero,
            new(1, 1, 1, 1),
            new(float.MaxValue, float.MinValue, -2, 4),
            new(-51665651.1f, -1.51586168f, -2155.56651f, -213.151651f)
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            Vector4 actual = _reader.ReadVector4();

            Assert.AreEqual(testValues[i], actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadQuaternion(BinaryReader)"/> method to ensure it correctly reads a <see cref="Quaternion"/> value from a binary stream.
    /// </summary>
    [TestMethod]
    public void ReadQuaternion_ShouldReturnCorrectQuaternion()
    {
        Quaternion[] testValues = new Quaternion[]
        {
            Quaternion.identity,
            new(1, 1, 1, 1),
            new(float.MaxValue, float.MinValue, -2, 4),
            new(-51665651.1f, -1.51586168f, -2155.56651f, -213.151651f)
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            Quaternion actual = _reader.ReadQuaternion();

            Assert.AreEqual(testValues[i], actual);
        }
    }

    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadColor(BinaryReader)"/> method to ensure it correctly reads a <see cref="Color"/> value from a binary stream.
    /// </summary>
    [TestMethod]
    public void ReadColor_ShouldReturnCorrectColor()
    {
        Color[] testValues = new Color[]
        {
            Color.clear,
            new(1, 1, 1, 1),
            new(float.MaxValue, float.MinValue, -2, 4),
            new(-51665651.1f, -1.51586168f, -2155.56651f, -213.151651f)
        };

        for (int i = 0; i < testValues.Length; i++)
        {
            _memoryStream.SetLength(0);
            _writer.Write(testValues[i]);

            _memoryStream.Position = 0;
            Color actual = _reader.ReadColor();

            Assert.AreEqual(testValues[i], actual);
        }
    }

    #endregion

    #region Collections

    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadArray{T}(BinaryReader, T[], int, int)"/> method 
    /// to ensure it correctly reads an array of type <typeparamref name="T"/> from a binary stream.
    /// </summary>
    [TestMethod]
    public void ReadArray_ShouldReturnCorrectArray()
    {
        int[] testValues = new int[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };

        _writer.Write(testValues.Length);
        for (int i = 0; i < testValues.Length; i++)
        {
            _writer.Write(testValues[i]);
        }

        int[] actual = new int[testValues.Length];
        _memoryStream.Position = 0;
        _reader.ReadArray(actual);

        CollectionAssert.AreEqual(testValues, actual);

        Assert.ThrowsException<EndOfStreamException>(() => _reader.ReadByte());

        _memoryStream.Position = 0;
        Assert.ThrowsException<ArgumentException>(() => _reader.ReadArray(actual, 2, 5));
    }

    /// <summary>
    /// Tests the <see cref="BinaryReaderExtensions.ReadList{T}(BinaryReader, List{T})"/> method
    /// </summary>
    [TestMethod]
    public void ReadList_ShouldReturnCorrectList()
    {
        var testValues = new List<int> { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };

        _writer.Write(testValues.Count);
        foreach (var item in testValues)
        {
            _writer.Write(item);
        }

        List<int> actual = new();
        _memoryStream.Position = 0;
        var result = _reader.ReadList(actual);

        Assert.ReferenceEquals(result, actual);
        CollectionAssert.AreEqual(testValues, actual);
        Assert.ThrowsException<EndOfStreamException>(() => _reader.ReadByte());

        _memoryStream.Position = 0;
        var newResult = _reader.ReadList<int>();

        Assert.IsFalse(result == newResult);
        CollectionAssert.AreEqual(result, newResult);
    }

    #endregion
}
