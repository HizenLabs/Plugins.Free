using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Tests.Extensions.UserPreference.Pooling;

[TestClass]
public class AllocationsTests
{
#if DEBUG
    [TestInitialize]
    public void TestInitialize()
    {
        // Preload the default instance before resetting the tracked count
        _ = ViewingConditions.Default;

        TrackedPool.FullReset();
    }

    [TestMethod]
    public void PooledObjects_ShouldReturn_ExceptDefaults()
    {
        int expectedRemaining = 0;

        _ = ViewingConditions.Default;

        // ViewingConditions.Default does not return to pool
        AssertAllocations(expectedRemaining, "ViewingConditions.Default");

        // scope for all using statements to dispose once done
        {
            StandardRgb color = 0xFFFFFFFF;
            using var hct = Hct.Create(color);

        }
        AssertAllocations(expectedRemaining, "Hct.Create");

        {
            StandardRgb color = 0xFFFFFFFF;
            var anotherHct = Hct.Create(color);
            using var scheme = SchemeTonalSpot.Create(anotherHct, false, 0.0);
        }
        AssertAllocations(expectedRemaining, "SchemeTonalSpot.Create");

        {
            StandardRgb color = 0xFFFFFFFF;
            var hct = Hct.Create(color);
            using var scheme = SchemeTonalSpot.Create(hct, false, 0.0);
            var spec = ColorSpecs.Get(scheme.SpecVersion);
        }
        AssertAllocations(expectedRemaining, "ColorSpecs.Get");

        {
            StandardRgb color = 0xFFFFFFFF;
            var hct = Hct.Create(color);
            using var scheme = SchemeTonalSpot.Create(hct, false, 0.0);
            var spec = ColorSpecs.Get(scheme.SpecVersion);
            var actualPrimary = spec.Primary.GetColor(scheme);
        }
        AssertAllocations(expectedRemaining, "Spec.Primary.GetColor");
    }

    private void AssertAllocations(int expeccted, string function)
    {
        if (expeccted != TrackedPool.AllocatedCount)
        {
            TrackedPoolDebug();

            Assert.Fail($"Allocation count mismatch in {function}. Expected: {expeccted}, Actual: {TrackedPool.AllocatedCount}");
        }
    }

    private void TrackedPoolDebug()
    {
        foreach (var pool in TrackedPool.TrackedPools)
        {
            Debug.WriteLine("=================================");
            Debug.WriteLine($"{pool.Key}: {pool.Value.Count}");

            if (pool.Key == typeof(ViewingConditions) && pool.Value.Count <= 1)
            {
                continue;
            }

            foreach (var trackingId in pool.Value.Tracking)
            {
                Debug.WriteLine($"TrackingId[{pool.Value.AllocationIndex[trackingId]}]: {trackingId}");
            }

            if (pool.Value.Count > 0)
            {
                var first = pool.Value.AllocationStacks.Last();
                Debug.WriteLine(first.Value);
            }
        }
    }
#endif
}
