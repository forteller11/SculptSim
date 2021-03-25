using UnityEngine;

namespace Collision
{
    public struct AABB
    {
        public Vector3 Center;
        public float HalfWidth;

        public AABB(Vector3 center, float halfWidth)
        {
            Center = center;
            HalfWidth = halfWidth;
        }
    }
}