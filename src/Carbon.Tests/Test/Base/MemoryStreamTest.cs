using Facepunch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Carbon.Tests.Test.Base;

/// <summary>
/// Base class for unit tests that require a <see cref="MemoryStream"/> instance.
/// </summary>
public abstract class MemoryStreamTest
{
    protected MemoryStream _memoryStream;

    /// <summary>
    /// Gets a new <see cref="MemoryStream"/> from the pool before each test.
    /// </summary>
    [TestInitialize]
    public void Initialize_MemoryStream()
    {
        _memoryStream = Pool.Get<MemoryStream>();
    }

    /// <summary>
    /// Cleans up the memory stream after each test.
    /// </summary>
    [TestCleanup]
    private void Cleanup_MemoryStream()
    {
        if (_memoryStream != null)
        {
            Pool.FreeUnmanaged(ref _memoryStream);
        }
    }

}
