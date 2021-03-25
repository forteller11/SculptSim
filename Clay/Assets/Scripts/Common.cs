using System.Runtime.CompilerServices;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DefaultNamespace
{
    public static class Common
    {
        public static Color RandomColor(ref Random random, float alpha = 1)
        {
            return new Color(random.NextFloat(), random.NextFloat(), random.NextFloat(), alpha);
        }

        //from: https://developer.mozilla.org/en-US/docs/Games/Techniques/3D_collision_detection
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SphereAABBOverlap(Vector3 sPos, float sRadius, Vector3 bPos, float bHalfWidth)
        {

            Vector3 bMin = bPos - new Vector3(bHalfWidth, bHalfWidth, bHalfWidth);
            Vector3 bMax = bPos + new Vector3(bHalfWidth, bHalfWidth, bHalfWidth);
       
            // get box closest point to sphere center by clamping
            var x = Mathf.Max(bMin.x, Mathf.Min(sPos.x, bMax.x));
            var y = Mathf.Max(bMin.y, Mathf.Min(sPos.y, bMax.y));
            var z = Mathf.Max(bMin.z, Mathf.Min(sPos.z, bMax.z));

            var closestPointToSphere = new Vector3(x, y, z);

            var dist = Vector3.Distance(closestPointToSphere, sPos);

            return dist < sRadius;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutQuart(float x) => 1 - ((1 - x) * (1 - x) * (1 - x) * (1 - x));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInQuart(float x) => 1 - (x * x * x * x);
        }
}
