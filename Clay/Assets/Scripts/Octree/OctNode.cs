using UnityEngine;

namespace Octree
{
    /* ---------------------
     * parent nodes will never contain elements,
     * leaf nodes will always contain elements
     --------------------*/
    public struct OctNode
    {
        /* -------------
        where XYZ == plus in those axis, and _ means minus in those axis
          > so XYZ is right, upper, forward AABB
          > while ___ is left, down, backwards AABB
          >  X__ is right, dowm, backwards AAB
        --------------------------- */ 
        
        public Vector3 Center;
        public float HalfWidth;
        
        public OctNode? NodeXYZ;
        public OctNode? Node_YZ;
        public OctNode? NodeX_Z;
        public OctNode? NodeXY_;
        public OctNode? Node__Z;
        public OctNode? NodeX__;
        public OctNode? Node_Y_;
        public OctNode? Node___;
        
        /// <summary>
        /// index in octree el array, -1 means no elements (not a leaf)
        /// </summary>
        public int ElementIndex;

        public bool IsOverlapping(Vector3 point)
        {
            
        }
        
        // public bool GetOverlappingSubtree(Vector3 point)
        // {
        //     
        // }
    }
}