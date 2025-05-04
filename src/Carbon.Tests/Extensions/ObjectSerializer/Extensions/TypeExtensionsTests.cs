
using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carbon.Tests.Extensions.ObjectSerializer.Extensions;

/// <summary>
/// Tests the <see cref="TypeExtensions"/> class.
/// </summary>
[TestClass]
public class TypeExtensionsTests
{
    /// <summary>
    /// Tests the <see cref="TypeExtensions.GetTypeMarker(System.Type)"/> method.
    /// </summary>
    /// <param name="type">The type to test.</param>
    /// <param name="expected">The expected <see cref="TypeMarker"/>.</param>
    [TestMethod]
    [TestCategory("System")]
    [DataRow(null, TypeMarker.Null)]
    [DataRow(typeof(bool), TypeMarker.Boolean)]
    [DataRow(typeof(sbyte), TypeMarker.SByte)]
    [DataRow(typeof(byte), TypeMarker.Byte)]
    [DataRow(typeof(short), TypeMarker.Int16)]
    [DataRow(typeof(ushort), TypeMarker.UInt16)]
    [DataRow(typeof(int), TypeMarker.Int32)]
    [DataRow(typeof(uint), TypeMarker.UInt32)]
    [DataRow(typeof(long), TypeMarker.Int64)]
    [DataRow(typeof(ulong), TypeMarker.UInt64)]
    [DataRow(typeof(float), TypeMarker.Single)]
    [DataRow(typeof(double), TypeMarker.Double)]
    [DataRow(typeof(decimal), TypeMarker.Decimal)]
    [DataRow(typeof(char), TypeMarker.Char)]
    [DataRow(typeof(string), TypeMarker.String)]
    [DataRow(typeof(byte[]), TypeMarker.ByteArray)]
    [DataRow(typeof(Guid), TypeMarker.Guid)]
    [DataRow(typeof(DateTime), TypeMarker.DateTime)]
    [DataRow(typeof(TimeSpan), TypeMarker.TimeSpan)]
    [DataRow(typeof(Type), TypeMarker.Type)]
    [DataRow(typeof(Vector2), TypeMarker.Vector2)]
    [DataRow(typeof(Vector3), TypeMarker.Vector3)]
    [DataRow(typeof(Vector4), TypeMarker.Vector4)]
    [DataRow(typeof(Quaternion), TypeMarker.Quaternion)]
    [DataRow(typeof(Color), TypeMarker.Color)]
    [DataRow(typeof(int[]), TypeMarker.Array)]
    [DataRow(typeof(string[]), TypeMarker.Array)]
    [DataRow(typeof(List<int>), TypeMarker.List)]
    [DataRow(typeof(List<string>), TypeMarker.List)]
    [DataRow(typeof(Dictionary<int, string>), TypeMarker.Dictionary)]
    [DataRow(typeof(Dictionary<string, object>), TypeMarker.Dictionary)]
    public void GetTypeMarker_ReturnsCorrectType(Type type, TypeMarker expected)
    {
        var actual = type.GetTypeMarker();
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [TestCategory("System")]
    [DataRow(typeof(bool), false)]
    [DataRow(typeof(sbyte), false)]
    [DataRow(typeof(byte), false)]
    [DataRow(typeof(short), false)]
    [DataRow(typeof(ushort), false)]
    [DataRow(typeof(int), false)]
    [DataRow(typeof(uint), false)]
    [DataRow(typeof(long), false)]
    [DataRow(typeof(ulong), false)]
    [DataRow(typeof(float), false)]
    [DataRow(typeof(double), false)]
    [DataRow(typeof(decimal), false)]
    [DataRow(typeof(char), false)]
    [DataRow(typeof(string), true)]
    [DataRow(typeof(byte[]), true)]
    [DataRow(typeof(Guid), false)]
    [DataRow(typeof(DateTime), false)]
    [DataRow(typeof(TimeSpan), false)]
    [DataRow(typeof(Type), true)]
    [DataRow(typeof(Vector2), false)]
    [DataRow(typeof(Vector3), false)]
    [DataRow(typeof(Vector4), false)]
    [DataRow(typeof(Quaternion), false)]
    [DataRow(typeof(Color), false)]
    [DataRow(typeof(int[]), true)]
    [DataRow(typeof(string[]), true)]
    [DataRow(typeof(List<int>), true)]
    [DataRow(typeof(List<string>), true)]
    [DataRow(typeof(Dictionary<int, string>), true)]
    [DataRow(typeof(Dictionary<string, object>), true)]
    [DataRow(typeof(object), true)]
    [DataRow(typeof(bool?), true)]
    [DataRow(typeof(sbyte?), true)]
    [DataRow(typeof(byte?), true)]
    [DataRow(typeof(short?), true)]
    [DataRow(typeof(int?), true)]
    [DataRow(typeof(Nullable<>), false)]
    [DataRow(typeof(void), false)]
    [DataRow(typeof(ValueType), true)]
    [DataRow(typeof(Enum), true)]
    [DataRow(typeof(Delegate), true)]
    [DataRow(typeof(DayOfWeek), false)]
    [DataRow(typeof(TypeMarker), false)]
    [DataRow(typeof(IDisposable), true)]
    public void CanBeNull_ReturnsExpectedValue(Type type, bool expected)
    {
        var actual = type.CanBeNull();
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [TestCategory("System")]
    public void CanBeNull_GenericReturnsExpectedValue()
    {
        Assert.IsFalse(GenericCanBeNull<bool>());
        Assert.IsFalse(GenericCanBeNull<byte>());
        Assert.IsFalse(GenericCanBeNull<short>());
        Assert.IsFalse(GenericCanBeNull<int>());
        Assert.IsFalse(GenericCanBeNull<uint>());
        Assert.IsFalse(GenericCanBeNull<long>());
        Assert.IsFalse(GenericCanBeNull<double>());
        Assert.IsFalse(GenericCanBeNull<decimal>());
        Assert.IsFalse(GenericCanBeNull<float>());
        Assert.IsFalse(GenericCanBeNull<char>());
        Assert.IsFalse(GenericCanBeNull<Guid>());
        Assert.IsFalse(GenericCanBeNull<DateTime>());
        Assert.IsFalse(GenericCanBeNull<TimeSpan>());
        Assert.IsFalse(GenericCanBeNull<Vector2>());
        Assert.IsFalse(GenericCanBeNull<Vector3>());
        Assert.IsFalse(GenericCanBeNull<Vector4>());
        Assert.IsFalse(GenericCanBeNull<Quaternion>());
        Assert.IsFalse(GenericCanBeNull<Color>());
        Assert.IsTrue(GenericCanBeNull<Type>());
        Assert.IsTrue(GenericCanBeNull<string>());
        Assert.IsTrue(GenericCanBeNull<string>());
        Assert.IsTrue(GenericCanBeNull<int?>());
        Assert.IsTrue(GenericCanBeNull<object>());
        Assert.IsTrue(GenericCanBeNull<int?>());
    }

    private bool GenericCanBeNull<T>()
    {
        return typeof(T).CanBeNull();
    }
}
