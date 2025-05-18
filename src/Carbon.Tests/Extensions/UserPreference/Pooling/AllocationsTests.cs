using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.DynamicColors;
using HizenLabs.Extensions.UserPreference.Material.Scheme;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Pooling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Carbon.Plugins.AutoBuildSnapshot.ColorPalettes;

namespace Carbon.Tests.Extensions.UserPreference.Pooling;

[TestClass]
public class AllocationsTests
{
    [TestMethod]
    public void PooledObjects_ShouldReturn_ExceptDefaults()
    {
        int expectedRemaining = 0;

        _ = ViewingConditions.Default;

        // ViewingConditions.Default does not return to pool
        Assert.AreEqual(++expectedRemaining, TrackedPool.AllocatedCount, "Calling ViewingConditions.Default caused unexpected allocations");

        // scope for all using statements to dispose once done
        {
            StandardRgb color = 0xFFFFFFFF;
            using var hct = Hct.Create(color);

            // Increment by one for the active Hct hold
            Assert.AreEqual(++expectedRemaining, TrackedPool.AllocatedCount, "Creating Hct caused unexpected allocations");

            using var scheme = SchemeTonalSpot.Create(hct, false, 0.0);

            // Increment by one for the active SchemeTonalSpot hold
            Assert.AreEqual(++expectedRemaining, TrackedPool.AllocatedCount, "Creating SchemeTonalSpot caused unexpected allocations");

            var spec = ColorSpecs.Get(scheme.SpecVersion);

            Assert.AreEqual(expectedRemaining, TrackedPool.AllocatedCount, "Getting ColorSpecs caused unexpected allocations");

            var actualPrimary = spec.Primary.GetColor(scheme);

            Assert.AreEqual(expectedRemaining, TrackedPool.AllocatedCount, "Getting Primary color caused unexpected allocations");
        }

        // All of the above allocated resources should be freed except the ViewingConditions.Default
        Assert.AreEqual(1, TrackedPool.AllocatedCount, "Exiting the scope, but unexpected allocations remain");
    }
}
