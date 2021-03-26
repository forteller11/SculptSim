using UnityEngine;

namespace Collision
{
    public static class Intersection
    {
        //from: https://developer.mozilla.org/en-US/docs/Games/Techniques/3D_collision_detection
        public static bool SphereAABBOverlap(Sphere sphere, AABB aabb)
        {
            var extentMin = aabb.GetExtentMin();
            var extentMax = aabb.GetExtentMax();
            
            // get box closest point to sphere center by clamping
            var x = Mathf.Max(extentMin.x, Mathf.Min(sphere.Position.x, extentMax.x));
            var y = Mathf.Max(extentMin.y, Mathf.Min(sphere.Position.y, extentMax.y));
            var z = Mathf.Max(extentMin.z, Mathf.Min(sphere.Position.z, extentMax.z));

            var closestPointToSphere = new Vector3(x, y, z);
            var dist = Vector3.Distance(closestPointToSphere, sphere.Position);

            return dist < sphere.Radius;
        }

        public static bool PointAABBOverlap(Vector3 point, AABB aabb)
        {
            var extentMin = aabb.GetExtentMin();
            var extentMax = aabb.GetExtentMax();
            
            bool withinX = point.x >= extentMin.x && point.x <= extentMax.x;
            bool withinY = point.y >= extentMin.y && point.y <= extentMax.y;
            bool withinZ = point.z >= extentMin.z && point.z <= extentMax.z;
            
            return withinX && withinY && withinZ;
        }
    }
}