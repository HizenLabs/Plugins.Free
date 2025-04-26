using Facepunch;
using System.Collections.Generic;
using UnityEngine;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Math for finding and scanning <see cref="BuildingPrivlidge"/> zones and gathering entities.
    /// </summary>
    private static class BuildingScanner
    {
        /// <summary>
        /// Gets all entities in the specified zone.
        /// </summary>
        /// <typeparam name="T">The type of entity to get.</typeparam>
        /// <param name="tc">The building privilege to get the entities from.</param>
        /// <param name="zone">The zone to search in.</param>
        /// <param name="layerMask">The layer mask to use for the search.</param>
        /// <returns>A list of entities found in the zone.</returns>
        public static PooledList<T> GetEntities<T>(BuildingPrivlidge tc, Vector4 zone, int layerMask)
            where T : BaseEntity
        {
            var found = Pool.Get<PooledList<T>>();
            Vis.Entities(new(zone.x, zone.y, zone.z), zone.w, found, layerMask);
            return found;
        }

        /// <summary>
        /// Gets the zone(s) for the given building privilege.
        /// </summary>
        /// <param name="results">The list to store the results in.</param>
        /// <param name="tc">The building privilege to get the entities from.</param>
        /// <param name="zoneRadius">The radius to use for the zone (from a 1x1 foundation).</param>
        /// <param name="maxRadius">The maximum radius split up the zones into.</param>
        public static PooledList<Vector4> GetZones(BuildingPrivlidge tc, float zoneRadius, float maxRadius)
        {
            using var coordinates = Pool.Get<PooledList<Vector3>>();

            var building = tc.GetBuilding();
            foreach (var block in building.buildingBlocks)
            {
                coordinates.Add(block.ServerPosition);
            }

            var results = Pool.Get<PooledList<Vector4>>();
            if (coordinates.Count == 0)
            {
                return results;
            }

            using var findZone = FindSplitZones(coordinates, zoneRadius, maxRadius);
            results.AddRange(findZone);

            return results;
        }

        /// <summary>
        /// Finds the zone(s) for the given coordinates using an iterative approach
        /// </summary>
        private static PooledList<Vector4> FindSplitZones(List<Vector3> coordinates, float zoneRadius, float maxRadius)
        {
            var zoneQueue = Pool.Get<Queue<List<Vector3>>>();
            zoneQueue.Enqueue(coordinates);

            var results = Pool.Get<PooledList<Vector4>>();
            while (zoneQueue.Count > 0)
            {
                var currentCoordinates = zoneQueue.Dequeue();

                // Skip empty coordinate sets
                if (currentCoordinates.Count == 0)
                {
                    Pool.FreeUnmanaged(ref currentCoordinates);
                    continue;
                }

                // Calculate zone parameters
                Vector3 center = CalculateCenter(currentCoordinates);
                float maxDistance = CalculateMaxDistance(currentCoordinates, center);
                float radius = maxDistance * 2 + zoneRadius;

                // Check if we need to split the zone
                if (radius > maxRadius)
                {
                    // Split into smaller zones and add to queue
                    SplitAndEnqueueZones(currentCoordinates, center, zoneQueue);
                }
                else
                {
                    // Add this zone to results
                    results.Add(new Vector4(center.x, center.y, center.z, radius));
                }

                // Free the current coordinates
                Pool.FreeUnmanaged(ref currentCoordinates);
            }

            // Free the queue
            Pool.FreeUnmanaged(ref zoneQueue);

            return results;
        }

        /// <summary>
        /// Calculates the maximum distance from the center to any coordinate
        /// </summary>
        private static float CalculateMaxDistance(List<Vector3> coordinates, Vector3 center)
        {
            float maxDistance = 0f;
            foreach (var coord in coordinates)
            {
                float distance = Vector3.Distance(center, coord);
                maxDistance = Mathf.Max(maxDistance, distance);
            }
            return maxDistance;
        }

        /// <summary>
        /// Splits coordinates and adds them to the queue
        /// </summary>
        private static void SplitAndEnqueueZones(List<Vector3> coordinates, Vector3 center, Queue<List<Vector3>> zoneQueue)
        {
            using var groups = SplitCoordinates(coordinates, center);
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                if (group.Count > 0)
                {
                    zoneQueue.Enqueue(group);
                }
                else
                {
                    Pool.FreeUnmanaged(ref group);
                }
            }
        }

        /// <summary>
        /// Calculates the center point from a list of coordinates.
        /// </summary>
        /// <param name="coordinates">The list of coordinates to calculate the center from.</param>
        /// <returns>The center point.</returns>
        private static Vector3 CalculateCenter(List<Vector3> coordinates)
        {
            Vector3 sum = Vector3.zero;
            foreach (var coord in coordinates)
            {
                sum += coord;
            }
            return sum / coordinates.Count;
        }

        /// <summary>
        /// Splits the coordinates into groups based on their position relative to the center point.
        /// </summary>
        /// <param name="coordinates">The list of coordinates to split.</param>
        /// <param name="center">The center point to use for splitting.</param>
        /// <returns>A list of groups of coordinates.</returns>
        private static PooledList<List<Vector3>> SplitCoordinates(List<Vector3> coordinates, Vector3 center)
        {
            // Splitting into octants (8 groups) based on position relative to center
            using var groups = Pool.Get<PooledList<List<Vector3>>>();
            for (int i = 0; i < 8; i++)
            {
                groups.Add(Pool.Get<List<Vector3>>());
            }

            foreach (var coord in coordinates)
            {
                int groupIndex = 0;

                // Determine which octant the point belongs to
                if (coord.x >= center.x) groupIndex |= 1;
                if (coord.y >= center.y) groupIndex |= 2;
                if (coord.z >= center.z) groupIndex |= 4;

                groups[groupIndex].Add(coord);
            }

            // Filter out empty groups
            var filteredGroups = Pool.Get<PooledList<List<Vector3>>>();
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                if (group.Count > 0)
                {
                    filteredGroups.Add(group);
                }
                else
                {
                    Pool.FreeUnmanaged(ref group);
                    groups[i] = null; // Mark as freed
                }
            }
            return filteredGroups;
        }
    }
}
