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
        
        public Vector3 GetExtentMin() => Center - new Vector3(HalfWidth,HalfWidth,HalfWidth);
        public Vector3 GetExtentMax() => Center + new Vector3(HalfWidth,HalfWidth,HalfWidth);
    }
}