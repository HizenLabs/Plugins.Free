using Carbon.Tests.Test.Base;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace Carbon.Tests.Extensions.ObjectSerializer.Integration;

/// <summary>
/// Tests for round-trip serialization tasks.
/// </summary>
[TestClass]
public class RoundTripSerializationTests : BinaryReaderWriterTest
{
    /// <summary>
    /// Tests the round-trip serialization of a list of strings.
    /// </summary>
    [TestMethod]
    [TestCategory("Collections")]
    public void ReadWrite_List()
    {
        var list = new List<string> { "Hello", "World", null, "foo", "bar!" };

        _writer.WriteList(list);

        _memoryStream.Position = 0;

        List<string> test = new();
        var deserializedList = _reader.ReadList(test);

        Assert.IsNotNull(deserializedList);
        Assert.AreEqual(list.Count, deserializedList.Count);
        Assert.IsTrue(ReferenceEquals(test, deserializedList));

        CollectionAssert.AreEqual(list, deserializedList);
        CollectionAssert.AreEqual(test, deserializedList);

        Assert.ThrowsException<EndOfStreamException>(() => _reader.ReadByte());

        _memoryStream.Position = 0;

        var nextList = _reader.ReadList<int>();
        Assert.IsNotNull(nextList);

        Assert.IsFalse(ReferenceEquals(test, nextList));
    }

    /// <summary>
    /// Tests the round-trip serialization of a <see cref="Dictionary{TKey, TValue}"/> where the key is a <see cref="string"/> and the value is a <see cref="List{T}"/> of type <see cref="object"/>.
    /// </summary>
    [TestMethod]
    [TestCategory("Collections")]
    public void ReadWrite_DictionaryListObject()
    {
        var dict = new Dictionary<string, List<object>>
        {
            { "key1", new List<object> { "value1", "value2", null, 1 } },
            { "key2", new List<object> { "value3", "value4" } },
            { "", new List<object> { null, 0.4f, 0.1423092831d, 32.2390482093849082m, (byte)12 } },
        };

        _writer.WriteDictionary(dict);
        _memoryStream.Position = 0;

        var actual = _reader.ReadDictionary<string, List<object>>();

        Assert.IsNotNull(actual);
        Assert.AreEqual(dict.Count, actual.Count);

        foreach (var kvp in dict)
        {
            CollectionAssert.AreEqual(kvp.Value, actual[kvp.Key], $"Key: {kvp.Key}");
        }

        Assert.ThrowsException<EndOfStreamException>(() => _reader.ReadByte());
    }
}
