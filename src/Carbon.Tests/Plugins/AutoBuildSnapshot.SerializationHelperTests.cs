using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oxide.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Carbon.Plugins.AutoBuildSnapshot.SerializationHelper;

namespace Carbon.Tests.Plugins;

[TestClass]
public partial class AutoBuildSnapshotTests
{
    [TestClass]
    public partial class SerializationHelperTests
    {
        #region Test Helpers

        /// <summary>
        /// Tests the serialization and deserialization of a type.
        /// </summary>
        /// <typeparam name="T">The type to test.</typeparam>
        /// <param name="original">The original object to serialize.</param>
        /// <param name="serializer">The serialization function.</param>
        /// <param name="deserializer">The deserialization function.</param>
        /// <param name="customComparer">Optional custom comparer for the deserialized object.</param>
        private static void TestSerializationRoundTrip<T>(
            T original,
            Action<BinaryWriter, T> serializer,
            Func<BinaryReader, T> deserializer,
            Action<T, T> customComparer = null)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            // Serialize
            serializer(writer, original);

            // Reset stream position
            stream.Position = 0;

            // Deserialize
            using var reader = new BinaryReader(stream);
            var deserialized = deserializer(reader);

            // Compare
            if (customComparer != null)
            {
                customComparer(original, deserialized);
            }
            else
            {
                Assert.AreEqual(original, deserialized, $"Deserialized value of type {typeof(T).Name} does not match original");
            }

            // Additionally check if we've read all data from the stream
            Assert.AreEqual(stream.Length, stream.Position, "Not all data was read from the stream");
        }

        #endregion

        #region Explicit Types

        #region System / Common

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, DateTime)"/> and <see cref="ReadDateTime(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_DateTime_Serialization_RoundTrip()
        {
            TestSerializationRoundTrip(DateTime.Now, Write, ReadDateTime);
            TestSerializationRoundTrip(DateTime.MinValue, Write, ReadDateTime);
            TestSerializationRoundTrip(DateTime.MaxValue, Write, ReadDateTime);
            TestSerializationRoundTrip(new DateTime(2000, 1, 1), Write, ReadDateTime);
        }

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, TimeSpan)"/> and <see cref="ReadTimeSpan(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_TimeSpan_Serialization_RoundTrip()
        {
            TestSerializationRoundTrip(TimeSpan.FromHours(1), Write, ReadTimeSpan);
            TestSerializationRoundTrip(TimeSpan.FromMinutes(30), Write, ReadTimeSpan);
            TestSerializationRoundTrip(TimeSpan.FromSeconds(15), Write, ReadTimeSpan);
            TestSerializationRoundTrip(TimeSpan.FromMilliseconds(500), Write, ReadTimeSpan);
        }

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, Type)"/> and <see cref="ReadType(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_Type_Serialization_RoundTrip()
        {
            TestSerializationRoundTrip(typeof(int), Write, ReadType,
                (original, deserialized) => Assert.AreEqual(original.FullName, deserialized.FullName));

            TestSerializationRoundTrip(typeof(string), Write, ReadType,
                (original, deserialized) => Assert.AreEqual(original.FullName, deserialized.FullName));

            TestSerializationRoundTrip(typeof(List<int>), Write, ReadType,
                (original, deserialized) => Assert.AreEqual(original.FullName, deserialized.FullName));
        }

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, Guid)"/> and <see cref="ReadGuid(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_Guid_Serialization_RoundTrip()
        {
            TestSerializationRoundTrip(Guid.NewGuid(), Write, ReadGuid);
            TestSerializationRoundTrip(Guid.Empty, Write, ReadGuid);
        }

        #endregion

        #region Carbon / Unity

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, VersionNumber)"/> and <see cref="ReadVersionNumber(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_VersionNumber_Serialization_RoundTrip()
        {
            TestSerializationRoundTrip(new VersionNumber(1, 2, 3), Write, ReadVersionNumber,
                (original, deserialized) =>
                {
                    Assert.AreEqual(original.Major, deserialized.Major);
                    Assert.AreEqual(original.Minor, deserialized.Minor);
                    Assert.AreEqual(original.Patch, deserialized.Patch);
                });
            TestSerializationRoundTrip(new VersionNumber(0, 0, 0), Write, ReadVersionNumber,
                (original, deserialized) =>
                {
                    Assert.AreEqual(original.Major, deserialized.Major);
                    Assert.AreEqual(original.Minor, deserialized.Minor);
                    Assert.AreEqual(original.Patch, deserialized.Patch);
                });
        }

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, Vector2)"/> and <see cref="ReadVector2(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_Vector2_Serialization_RoundTrip()
        {
            TestSerializationRoundTrip(new Vector2(1.0f, 2.0f), Write, ReadVector2,
                (original, deserialized) => {
                    Assert.AreEqual(original.x, deserialized.x);
                    Assert.AreEqual(original.y, deserialized.y);
                });

            TestSerializationRoundTrip(Vector2.zero, Write, ReadVector2,
                (original, deserialized) => {
                    Assert.AreEqual(original.x, deserialized.x);
                    Assert.AreEqual(original.y, deserialized.y);
                });

            TestSerializationRoundTrip(Vector2.one, Write, ReadVector2,
                (original, deserialized) => {
                    Assert.AreEqual(original.x, deserialized.x);
                    Assert.AreEqual(original.y, deserialized.y);
                });
        }

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, Vector3)"/> and <see cref="ReadVector3(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_Vector3_Serialization_RoundTrip()
        {
            TestSerializationRoundTrip(new Vector3(1.0f, 2.0f, 3.0f), Write, ReadVector3,
                (original, deserialized) => {
                    Assert.AreEqual(original.x, deserialized.x);
                    Assert.AreEqual(original.y, deserialized.y);
                    Assert.AreEqual(original.z, deserialized.z);
                });

            TestSerializationRoundTrip(Vector3.zero, Write, ReadVector3,
                (original, deserialized) => {
                    Assert.AreEqual(original.x, deserialized.x);
                    Assert.AreEqual(original.y, deserialized.y);
                    Assert.AreEqual(original.z, deserialized.z);
                });

            TestSerializationRoundTrip(Vector3.one, Write, ReadVector3,
                (original, deserialized) => {
                    Assert.AreEqual(original.x, deserialized.x);
                    Assert.AreEqual(original.y, deserialized.y);
                    Assert.AreEqual(original.z, deserialized.z);
                });
        }

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, Vector4)"/> and <see cref="ReadVector4(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_Vector4_Serialization_RoundTrip()
        {
            TestSerializationRoundTrip(new Vector4(1.0f, 2.0f, 3.0f, 4.0f), Write, ReadVector4,
                (original, deserialized) => {
                    Assert.AreEqual(original.x, deserialized.x);
                    Assert.AreEqual(original.y, deserialized.y);
                    Assert.AreEqual(original.z, deserialized.z);
                    Assert.AreEqual(original.w, deserialized.w);
                });

            TestSerializationRoundTrip(Vector4.zero, Write, ReadVector4,
                (original, deserialized) => {
                    Assert.AreEqual(original.x, deserialized.x);
                    Assert.AreEqual(original.y, deserialized.y);
                    Assert.AreEqual(original.z, deserialized.z);
                    Assert.AreEqual(original.w, deserialized.w);
                });

            TestSerializationRoundTrip(Vector4.one, Write, ReadVector4,
                (original, deserialized) => {
                    Assert.AreEqual(original.x, deserialized.x);
                    Assert.AreEqual(original.y, deserialized.y);
                    Assert.AreEqual(original.z, deserialized.z);
                    Assert.AreEqual(original.w, deserialized.w);
                });
        }

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, Quaternion)"/> and <see cref="ReadQuaternion(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_Quaternion_Serialization_RoundTrip()
        {
            TestSerializationRoundTrip(new Quaternion(1.0f, 2.0f, 3.0f, 4.0f), Write, ReadQuaternion,
                (original, deserialized) => {
                    Assert.AreEqual(original.x, deserialized.x);
                    Assert.AreEqual(original.y, deserialized.y);
                    Assert.AreEqual(original.z, deserialized.z);
                    Assert.AreEqual(original.w, deserialized.w);
                });

            TestSerializationRoundTrip(Quaternion.identity, Write, ReadQuaternion,
                (original, deserialized) => {
                    Assert.AreEqual(original.x, deserialized.x);
                    Assert.AreEqual(original.y, deserialized.y);
                    Assert.AreEqual(original.z, deserialized.z);
                    Assert.AreEqual(original.w, deserialized.w);
                });
        }

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, Color)"/> and <see cref="ReadColor(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_Color_Serialization_RoundTrip()
        {
            TestSerializationRoundTrip(new Color(1.0f, 0.5f, 0.25f, 0.75f), Write, ReadColor,
                (original, deserialized) => {
                    Assert.AreEqual(original.r, deserialized.r);
                    Assert.AreEqual(original.g, deserialized.g);
                    Assert.AreEqual(original.b, deserialized.b);
                    Assert.AreEqual(original.a, deserialized.a);
                });

            TestSerializationRoundTrip(Color.red, Write, ReadColor,
                (original, deserialized) => {
                    Assert.AreEqual(original.r, deserialized.r);
                    Assert.AreEqual(original.g, deserialized.g);
                    Assert.AreEqual(original.b, deserialized.b);
                    Assert.AreEqual(original.a, deserialized.a);
                });

            TestSerializationRoundTrip(Color.black, Write, ReadColor,
                (original, deserialized) => {
                    Assert.AreEqual(original.r, deserialized.r);
                    Assert.AreEqual(original.g, deserialized.g);
                    Assert.AreEqual(original.b, deserialized.b);
                    Assert.AreEqual(original.a, deserialized.a);
                });
        }

        #endregion

        #region Custom

        #endregion

        #endregion

        #region Generic Types

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, Array)"/> and <see cref="ReadArray{T}(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_Array_Serialization_RoundTrip()
        {
            // Test int array
            TestSerializationRoundTrip(new[] { 1, 2, 3, 4, 5 }, Write, ReadArray<int>,
                (original, deserialized) => {
                    Assert.AreEqual(original.Length, deserialized.Length);
                    for (int i = 0; i < original.Length; i++)
                    {
                        Assert.AreEqual(original[i], deserialized[i]);
                    }
                });

            // Test string array
            TestSerializationRoundTrip(new[] { "one", "two", "three" }, Write, ReadArray<string>,
                (original, deserialized) => {
                    Assert.AreEqual(original.Length, deserialized.Length);
                    for (int i = 0; i < original.Length; i++)
                    {
                        Assert.AreEqual(original[i], deserialized[i]);
                    }
                });

            // Test empty array
            TestSerializationRoundTrip(new string[0], Write, ReadArray<string>,
                (original, deserialized) => {
                    Assert.AreEqual(original.Length, deserialized.Length);
                });
        }

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, IList)"/> and <see cref="ReadList{T}(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_List_Serialization_RoundTrip()
        {
            // Test int list
            TestSerializationRoundTrip(new List<int> { 1, 2, 3, 4, 5 }, Write, ReadList<int>,
                (original, deserialized) => {
                    Assert.AreEqual(original.Count, deserialized.Count);
                    for (int i = 0; i < original.Count; i++)
                    {
                        Assert.AreEqual(original[i], deserialized[i]);
                    }
                });

            // Test string list
            TestSerializationRoundTrip(new List<string> { "one", "two", "three" }, Write, ReadList<string>,
                (original, deserialized) => {
                    Assert.AreEqual(original.Count, deserialized.Count);
                    for (int i = 0; i < original.Count; i++)
                    {
                        Assert.AreEqual(original[i], deserialized[i]);
                    }
                });

            // Test empty list
            TestSerializationRoundTrip(new List<string>(), Write, ReadList<string>,
                (original, deserialized) => {
                    Assert.AreEqual(original.Count, deserialized.Count);
                });
        }

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, IDictionary)"/> and <see cref="ReadDictionary{TKey, TValue}(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_Dictionary_Serialization_RoundTrip()
        {
            // Test string to int dictionary
            var stringIntDict = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };

            TestSerializationRoundTrip(stringIntDict, Write,
                reader => ReadDictionary(reader, new Dictionary<string, int>()),
                (original, deserialized) => {
                    Assert.AreEqual(original.Count, deserialized.Count);
                    foreach (var key in original.Keys)
                    {
                        Assert.IsTrue(deserialized.ContainsKey(key));
                        Assert.AreEqual(original[key], deserialized[key]);
                    }
                });

            // Test int to string dictionary
            var intStringDict = new Dictionary<int, string>
            {
                { 1, "one" },
                { 2, "two" },
                { 3, "three" }
            };

            TestSerializationRoundTrip(intStringDict, Write,
                reader => ReadDictionary(reader, new Dictionary<int, string>()),
                (original, deserialized) => {
                    Assert.AreEqual(original.Count, deserialized.Count);
                    foreach (var key in original.Keys)
                    {
                        Assert.IsTrue(deserialized.ContainsKey(key));
                        Assert.AreEqual(original[key], deserialized[key]);
                    }
                });

            // Test empty dictionary
            TestSerializationRoundTrip(new Dictionary<int, string>(), Write,
                reader => ReadDictionary(reader, new Dictionary<int, string>()),
                (original, deserialized) => {
                    Assert.AreEqual(original.Count, deserialized.Count);
                });
        }

        /// <summary>
        /// Tests for <see cref="Write(BinaryWriter, TypeMarker)"/> and <see cref="ReadTypeMarker(BinaryReader)"/>.
        /// </summary>
        [TestMethod]
        public void Test_TypeMarker_Serialization_RoundTrip()
        {
            foreach (TypeMarker marker in Enum.GetValues(typeof(TypeMarker)))
            {
                TestSerializationRoundTrip(marker, Write, ReadTypeMarker);
            }
        }
    }

    #endregion
}