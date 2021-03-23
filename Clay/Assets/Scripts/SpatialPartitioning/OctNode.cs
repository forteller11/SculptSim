using System;
using UnityEngine;

namespace SpatialPartitioning
{
    /* ---------------------
     * parent nodes will never contain elements,
     * leaf nodes will always contain elements
     --------------------*/
    [Serializable]
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
        
        //change to point to lkist
        public int NodeXYZ;
        public int Node_YZ;
        public int NodeX_Z;
        public int NodeXY_;
        public int Node__Z;
        public int NodeX__;
        public int Node_Y_;
        public int Node___;
        
        /// <summary>
        /// index in octree el array, -1 means no elements (not a leaf)
        /// </summary>
        public int FirstElementIndex;
        
    }
}