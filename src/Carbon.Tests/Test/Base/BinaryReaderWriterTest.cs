using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Carbon.Tests.Test.Base;

/// <summary>
/// Base class for unit tests that require a <see cref="BinaryReader"/> and <see cref="BinaryWriter"/>.
/// </summary>
public abstract class BinaryReaderWriterTest : MemoryStreamTest
{
    protected BinaryReader _reader;
    protected BinaryWriter _writer;

    /// <summary>
    /// Sets up a new <see cref="BinaryReader"/> and <see cref="BinaryWriter"/> before each test.
    /// </summary>
    [TestInitialize]
    public void Initialize_BinaryReaderWriters()
    {
        _reader = new(_memoryStream);
        _writer = new(_memoryStream);
    }

    /// <summary>
    /// Cleans up the <see cref="BinaryReader"/> and <see cref="BinaryWriter"/> after each test.
    /// </summary>
    [TestCleanup]
    public void Cleanup_BinaryReaderWriters()
    {
        _reader?.Dispose();
        _writer?.Dispose();
    }
}
