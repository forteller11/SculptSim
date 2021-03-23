using UnityEngine;

namespace SpatialPartitioning
{
    public static class CollisionHelpers
    {
        public static bool AABBPointOverlap(Vector3 point, Vector3 boxCenter, float boxHalfWidth)
        {
            if (Mathf.Abs(point.x - boxCenter.x) > boxHalfWidth ||
                Mathf.Abs(point.y - boxCenter.y) > boxHalfWidth ||
                Mathf.Abs(point.z - boxCenter.z) > boxHalfWidth)
            {
                return false;
            }

            return true;
        }


        /// <returns> returns vector3int between [-1,-1,-1] and [1,1,1]</returns>
        public static Vector3Int AABBOctant(Vector3 point, Vector3 boxCenter)
        {
            int x = point.x > boxCenter.x ? 1 : -1;
            int y = point.y > boxCenter.y ? 1 : -1;
            int z = point.z > boxCenter.z ? 1 : -1;
            return new Vector3Int(x, y, z);
        }
    }
}