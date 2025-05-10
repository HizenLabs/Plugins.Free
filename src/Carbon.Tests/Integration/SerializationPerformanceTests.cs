using HizenLabs.Extensions.ObjectSerializer.Extensions;
using HizenLabs.Extensions.ObjectSerializer.Internal;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Carbon.Tests.Extensions.ObjectSerializer.Performance;

[TestClass]
public class SerializationPerformanceTests
{
    // Test data structures
    private class TestData
    {
        public int IntValue { get; set; }
        public float FloatValue { get; set; }
        public string StringValue { get; set; }
        public Vector3 PositionValue { get; set; }
        public List<int> ListValue { get; set; }
        public Dictionary<string, int> DictValue { get; set; }
    }

    private List<TestData> _testDataSet;
    private const int SMALL_DATASET_SIZE = 10;
    private const int MEDIUM_DATASET_SIZE = 1000;
    private const int LARGE_DATASET_SIZE = 10000;
    private const int WARM_UP_ITERATIONS = 10;

    [TestInitialize]
    public void Setup()
    {
        // Initialize test data
        _testDataSet = GenerateTestData(LARGE_DATASET_SIZE);
    }

    private List<TestData> GenerateTestData(int count)
    {
        var data = new List<TestData>();
        var random = new System.Random(42); // Fixed seed for reproducibility

        for (int i = 0; i < count; i++)
        {
            var item = new TestData
            {
                IntValue = random.Next(),
                FloatValue = (float)random.NextDouble() * 1000f,
                StringValue = $"Item_{i}_{Guid.NewGuid()}",
                PositionValue = new Vector3(
                    (float)random.NextDouble() * 10f,
                    (float)random.NextDouble() * 10f,
                    (float)random.NextDouble() * 10f
                ),
                ListValue = Enumerable.Range(0, random.Next(5, 20))
                    .Select(_ => random.Next())
                    .ToList(),
                DictValue = Enumerable.Range(0, random.Next(3, 10))
                    .ToDictionary(
                        x => $"key_{x}",
                        x => random.Next()
                    )
            };
            data.Add(item);
        }

        return data;
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void PrimitiveType_SerializationPerformance()
    {
        const int iterations = 100000;

        // Test integers
        TestPrimitivePerformance("Int32", 42, iterations);
        TestPrimitivePerformance("Float", 3.14159f, iterations);
        TestPrimitivePerformance("String", "Hello, World!", iterations);
        TestPrimitivePerformance("Guid", Guid.NewGuid(), iterations);
        TestPrimitivePerformance("Vector3", new Vector3(1, 2, 3), iterations);
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void Complex_SmallDataset_RoundtripPerformance()
    {
        RunRoundtripPerformanceTest("Small Dataset", SMALL_DATASET_SIZE, 100);
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void Complex_MediumDataset_RoundtripPerformance()
    {
        RunRoundtripPerformanceTest("Medium Dataset", MEDIUM_DATASET_SIZE, 10);
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void Complex_LargeDataset_RoundtripPerformance()
    {
        RunRoundtripPerformanceTest("Large Dataset", LARGE_DATASET_SIZE, 1);
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void Serialization_MemoryPressure()
    {
        var dataset = _testDataSet.Take(1000).ToList();

        // Measure memory before
        var initialMemory = GC.GetTotalMemory(true);

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Serialize without measuring time
        foreach (var item in dataset)
        {
            writer.Write(item.IntValue);
            writer.Write(item.FloatValue);
            writer.Write(item.StringValue);
            writer.Write(item.PositionValue);
            writer.WriteList(item.ListValue);
            writer.WriteDictionary(item.DictValue);
        }

        // Measure memory after
        var finalMemory = GC.GetTotalMemory(true);
        var memoryUsed = finalMemory - initialMemory;

        Trace.WriteLine($"Memory used for 1000 objects: {memoryUsed / 1024.0:F2} KB");
        Trace.WriteLine($"Average memory per object: {memoryUsed / 1000.0:F2} bytes");
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void Detailed_TypeMarker_Performance()
    {
        const int iterations = 1000000;

        var types = new[]
        {
            typeof(int),
            typeof(string),
            typeof(Vector3),
            typeof(List<int>),
            typeof(Dictionary<string, int>),
            typeof(SerializableObject)
        };

        foreach (var type in types)
        {
            // Warm up
            for (int i = 0; i < 100; i++)
            {
                type.GetTypeMarker();
            }

            // Measure
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                type.GetTypeMarker();
            }
            sw.Stop();

            double avgNanoseconds = sw.Elapsed.TotalMilliseconds * 1_000_000 / iterations;
            Trace.WriteLine($"Type {type.Name}: {avgNanoseconds:F2} ns per call");
        }
    }

    private void TestPrimitivePerformance<T>(string typeName, T value, int iterations)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Warm up
        for (int i = 0; i < WARM_UP_ITERATIONS; i++)
        {
            ms.Position = 0;
            GenericWriter<T>.Write(writer, value);
        }

        // Measure serialization
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            ms.Position = 0;
            GenericWriter<T>.Write(writer, value);
        }
        sw.Stop();

        double avgNanosecondsSer = sw.Elapsed.TotalMilliseconds * 1_000_000 / iterations;

        // Prepare for deserialization
        ms.Position = 0;
        using var reader = new BinaryReader(ms);

        // Warm up
        for (int i = 0; i < WARM_UP_ITERATIONS; i++)
        {
            ms.Position = 0;
            GenericReader<T>.Read(reader);
        }

        // Measure deserialization
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            ms.Position = 0;
            GenericReader<T>.Read(reader);
        }
        sw.Stop();

        double avgNanosecondsDeser = sw.Elapsed.TotalMilliseconds * 1_000_000 / iterations;

        Trace.WriteLine($"{typeName} - Serialize: {avgNanosecondsSer:F2} ns/op, Deserialize: {avgNanosecondsDeser:F2} ns/op");
    }

    private void RunRoundtripPerformanceTest(string testName, int datasetSize, int iterations)
    {
        var dataset = _testDataSet.Take(datasetSize).ToList();
        var buffer = new byte[1024 * 1024 * 10]; // 10MB buffer

        // Warm up
        for (int i = 0; i < WARM_UP_ITERATIONS; i++)
        {
            using var ms = new MemoryStream(buffer);
            SerializeDataset(dataset, ms);
            ms.Position = 0;
            DeserializeDataset(ms, datasetSize);
        }

        // Measure serialization
        double totalSerializeMs = 0;
        double totalDeserializeMs = 0;
        long totalBytesWritten = 0;

        for (int i = 0; i < iterations; i++)
        {
            using var ms = new MemoryStream(buffer);

            // Serialize
            var sw = Stopwatch.StartNew();
            SerializeDataset(dataset, ms);
            sw.Stop();
            totalSerializeMs += sw.Elapsed.TotalMilliseconds;
            totalBytesWritten += ms.Position;

            // Deserialize
            ms.Position = 0;
            sw.Restart();
            DeserializeDataset(ms, datasetSize);
            sw.Stop();
            totalDeserializeMs += sw.Elapsed.TotalMilliseconds;
        }

        double avgSerializeMs = totalSerializeMs / iterations;
        double avgDeserializeMs = totalDeserializeMs / iterations;
        double avgBytesPerObject = totalBytesWritten / (iterations * datasetSize);

        Trace.WriteLine($"\n=== {testName} Performance ===");
        Trace.WriteLine($"Dataset size: {datasetSize} objects");
        Trace.WriteLine($"Iterations: {iterations}");
        Trace.WriteLine($"Average serialization time: {avgSerializeMs:F3} ms");
        Trace.WriteLine($"Average deserialization time: {avgDeserializeMs:F3} ms");
        Trace.WriteLine($"Objects per second (serialize): {datasetSize / (avgSerializeMs / 1000):F0}");
        Trace.WriteLine($"Objects per second (deserialize): {datasetSize / (avgDeserializeMs / 1000):F0}");
        Trace.WriteLine($"Average bytes per object: {avgBytesPerObject:F1}");
        Trace.WriteLine($"Throughput (serialize): {totalBytesWritten / (totalSerializeMs / 1000) / (1024 * 1024):F2} MB/s");
    }

    private void SerializeDataset(List<TestData> dataset, MemoryStream ms)
    {
        using var writer = new BinaryWriter(ms, Encoding.UTF8, true);

        foreach (var item in dataset)
        {
            writer.Write(item.IntValue);
            writer.Write(item.FloatValue);
            writer.Write(item.StringValue);
            writer.Write(item.PositionValue);
            writer.WriteList(item.ListValue);
            writer.WriteDictionary(item.DictValue);
        }
    }

    private void DeserializeDataset(MemoryStream ms, int count)
    {
        using var reader = new BinaryReader(ms, Encoding.UTF8, true);

        for (int i = 0; i < count; i++)
        {
            var intVal = reader.ReadInt32();
            var floatVal = reader.ReadSingle();
            var stringVal = reader.ReadString();
            var vec3Val = reader.ReadVector3();
            var listVal = reader.ReadList<int>();
            var dictVal = reader.ReadDictionary<string, int>();
        }
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void String_SerializationPatterns_Comparison()
    {
        const int iterations = 10000;
        var strings = new[]
        {
            "short",
            "medium length string here",
            "this is a much longer string that would be more typical in real applications with lots of text and characters",
            string.Concat(Enumerable.Repeat("very long ", 100))
        };

        foreach (var str in strings)
        {
            TestStringSerializationPerformance(str, iterations);
        }
    }

    private void TestStringSerializationPerformance(string value, int iterations)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Warm up
        for (int i = 0; i < 10; i++)
        {
            ms.Position = 0;
            writer.Write(value);
        }

        // Measure
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            ms.Position = 0;
            writer.Write(value);
        }
        sw.Stop();

        double avgMicroseconds = sw.Elapsed.TotalMilliseconds * 1000 / iterations;
        double bytesPerSecond = ms.Length * iterations / sw.Elapsed.TotalSeconds;

        Trace.WriteLine($"String length {value.Length}: {avgMicroseconds:F2} µs/op, {bytesPerSecond / (1024 * 1024):F2} MB/s");
    }
}