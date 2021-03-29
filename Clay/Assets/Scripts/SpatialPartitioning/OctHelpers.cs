using UnityEngine;

namespace SpatialPartitioning
{
    public static class OctHelpers
    {
        ///<returns>returns values between [-1,-1,-1] and [1,1,1]</returns>
        public static Vector3Int OctantToVector3Int(Octant octant)
        {
            int x = (int) (octant & Octant.X__) >> 2;
            int y = (int) (octant & Octant._Y_) >> 1;
            int z = (int) (octant & Octant.__Z) >> 0;

            var vec = (new Vector3Int(x, y, z));
            var scaledBetweenOneAndMinusOne = (vec * 2) - new Vector3Int(1, 1, 1);
            
            return scaledBetweenOneAndMinusOne;
        }
    }
}