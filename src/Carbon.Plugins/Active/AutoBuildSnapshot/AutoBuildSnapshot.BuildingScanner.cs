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
        public static List<T> GetEntities<T>(BuildingPrivlidge tc, Vector4 zone, int layerMask)
            where T : BaseEntity
        {
            var found = Pool.Get<List<T>>();
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
        public static void GetZones(List<Vector4> results, BuildingPrivlidge tc, float zoneRadius, float maxRadius)
        {
            var building = tc.GetBuilding();
            var coordinates = Pool.Get<List<Vector3>>();

            foreach (var block in building.buildingBlocks)
            {
                coordinates.Add(block.ServerPosition);
            }

            if (coordinates.Count == 0)
            {
                Pool.FreeUnmanaged(ref coordinates);
            }

            var findZone = FindSplitZones(coordinates, zoneRadius, maxRadius);
            results.AddRange(findZone);

            Pool.FreeUnmanaged(ref coordinates);
            Pool.FreeUnmanaged(ref findZone);
        }

        /// <summary>
        /// Finds the zone(s) for the given coordinates using an iterative approach
        /// </summary>
        private static List<Vector4> FindSplitZones(List<Vector3> coordinates, float zoneRadius, float maxRadius)
        {
            // Create results list
            var result = Pool.Get<List<Vector4>>();

            // Create a queue for zones that need processing
            var zoneQueue = Pool.Get<Queue<List<Vector3>>>();
            zoneQueue.Enqueue(coordinates);

            // Process each zone in the queue
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
                    result.Add(new Vector4(center.x, center.y, center.z, radius));
                }

                // Free the current coordinates
                Pool.FreeUnmanaged(ref currentCoordinates);
            }

            // Free the queue
            Pool.FreeUnmanaged(ref zoneQueue);

            return result;
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
            var groups = SplitCoordinates(coordinates, center);

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

            Pool.FreeUnmanaged(ref groups);
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
        private static List<List<Vector3>> SplitCoordinates(List<Vector3> coordinates, Vector3 center)
        {
            // Splitting into octants (8 groups) based on position relative to center
            var groups = Pool.Get<List<List<Vector3>>>();
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
            var filteredGroups = Pool.Get<List<List<Vector3>>>();
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
            Pool.FreeUnmanaged(ref groups);
            return filteredGroups;
        }

        /// <summary>
        /// Filters out spheres that are completely contained within other spheres.
        /// </summary>
        /// <param name="spheres">The list of spheres to filter.</param>
        /// <returns>A list of spheres that were removed.</returns>
        public static List<Vector4> FilterContainedSpheres(List<Vector4> spheres)
        {
            if (spheres == null || spheres.Count <= 1)
                return Pool.Get<List<Vector4>>();

            var removedSpheres = Pool.Get<List<Vector4>>();

            // We'll use indices in reverse order to safely remove items during iteration
            for (int i = spheres.Count - 1; i >= 0; i--)
            {
                Vector4 targetSphere = spheres[i];
                if (IsCompletelyCovered(targetSphere, spheres, i))
                {
                    removedSpheres.Add(targetSphere);
                    spheres.RemoveAt(i);
                }
            }

            return removedSpheres;
        }

        /// <summary>
        /// Checks if a sphere is completely covered by other spheres.
        /// </summary>
        private static bool IsCompletelyCovered(Vector4 targetSphere, List<Vector4> allSpheres, int targetIndex)
        {
            Vector3 center = new(targetSphere.x, targetSphere.y, targetSphere.z);
            float radius = targetSphere.w;

            // Generate sample points
            var surfacePoints = GenerateSurfacePoints(center, radius);

            // Check coverage for each point
            bool isFullyCovered = CheckAllPointsCovered(surfacePoints, allSpheres, targetIndex);

            Pool.FreeUnmanaged(ref surfacePoints);
            return isFullyCovered;
        }

        /// <summary>
        /// Checks if all points are covered by at least one sphere in the collection
        /// </summary>
        private static bool CheckAllPointsCovered(List<Vector3> points, List<Vector4> spheres, int targetIndex)
        {
            foreach (var point in points)
            {
                if (!IsPointCoveredByAnySphere(point, spheres, targetIndex))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if a point is covered by any sphere except the target
        /// </summary>
        private static bool IsPointCoveredByAnySphere(Vector3 point, List<Vector4> spheres, int targetIndex)
        {
            for (int j = 0; j < spheres.Count; j++)
            {
                if (j == targetIndex)
                    continue;

                Vector4 sphere = spheres[j];
                Vector3 sphereCenter = new(sphere.x, sphere.y, sphere.z);
                float sphereRadius = sphere.w;

                if (Vector3.Distance(point, sphereCenter) <= sphereRadius)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Generates points on the surface of a sphere using the Fibonacci lattice method.
        /// </summary>
        /// <param name="center">The center of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <returns>A list of points on the surface of the sphere.</returns>
        private static List<Vector3> GenerateSurfacePoints(Vector3 center, float radius)
        {
            var points = Pool.Get<List<Vector3>>();
            int numPoints = 100; // Increase for better accuracy

            // Fibonacci lattice method for uniform distribution on a sphere
            float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;

            for (int i = 0; i < numPoints; i++)
            {
                float theta = 2 * Mathf.PI * i / goldenRatio;
                float phi = Mathf.Acos(1 - 2 * (i + 0.5f) / numPoints);

                float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
                float z = radius * Mathf.Cos(phi);

                points.Add(center + new Vector3(x, y, z));
            }

            return points;
        }
    }
}
