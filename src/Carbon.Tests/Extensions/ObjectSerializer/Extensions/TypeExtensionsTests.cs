
using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    [DataRow(typeof(System.Guid), TypeMarker.Guid)]
    [DataRow(typeof(System.DateTime), TypeMarker.DateTime)]
    [DataRow(typeof(System.TimeSpan), TypeMarker.TimeSpan)]
    [DataRow(typeof(System.Type), TypeMarker.Type)]
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
    public void GetTypeMarker_ReturnsCorrectType(System.Type type, TypeMarker expected)
    {
        var actual = type.GetTypeMarker();
        Assert.AreEqual(expected, actual);
    }
}
