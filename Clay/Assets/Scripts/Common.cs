using System.Runtime.CompilerServices;
using Collision;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ClaySimulation
{
    public static class Common
    {
        public static Color RandomColor(ref Random random, float alpha = 1)
        {
            return new Color(random.NextFloat(), random.NextFloat(), random.NextFloat(), alpha);
        }
        
        
        //from: https://developer.mozilla.org/en-US/docs/Games/Techniques/3D_collision_detection
        public static bool SphereAABBOverlap(Sphere sphere, AABB aabb)
        {
            Vector3 bMin = aabb.Center - new Vector3(aabb.HalfWidth, aabb.HalfWidth, aabb.HalfWidth);
            Vector3 bMax = aabb.Center + new Vector3(aabb.HalfWidth, aabb.HalfWidth, aabb.HalfWidth);
       
            // get box closest point to sphere center by clamping
            var x = Mathf.Max(bMin.x, Mathf.Min(sphere.Position.x, bMax.x));
            var y = Mathf.Max(bMin.y, Mathf.Min(sphere.Position.y, bMax.y));
            var z = Mathf.Max(bMin.z, Mathf.Min(sphere.Position.z, bMax.z));

            var closestPointToSphere = new Vector3(x, y, z);
            var dist = Vector3.Distance(closestPointToSphere, sphere.Position);

            return dist < sphere.Radius;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutQuart(float x) => 1 - ((1 - x) * (1 - x) * (1 - x) * (1 - x));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInQuart(float x) => 1 - (x * x * x * x);
    }
}
