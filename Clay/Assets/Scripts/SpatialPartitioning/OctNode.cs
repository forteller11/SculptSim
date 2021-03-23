using System;
using System.Collections.Generic;
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

        
        public OctNode(Vector3 center, float halfWidth)
        {
            Center = center;
            HalfWidth = halfWidth;
            
            NodeXYZ = -1;
            Node_YZ = -1;
            NodeX_Z = -1;
            NodeXY_ = -1;
            Node__Z = -1;
            NodeX__ = -1;
            Node_Y_ = -1;
            Node___ = -1;
            
            FirstElementIndex = -1;
        }

        public static OctNode Empty()
        {
            var octNode = new OctNode();
            
            octNode.Center = Vector3.negativeInfinity;
            octNode.HalfWidth = Single.NaN;
            
            octNode.NodeXYZ = -1;
            octNode.Node_YZ = -1;
            octNode.NodeX_Z = -1;
            octNode.NodeXY_ = -1;
            octNode.Node__Z = -1;
            octNode.NodeX__ = -1;
            octNode.Node_Y_ = -1;
            octNode.Node___ = -1;
            
            octNode.FirstElementIndex = -1;

            return octNode;
        }

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

        void SetChild(int octant, OctNode value, List<OctNode> nodes)
        {
            switch (octant)
            {
                case 0b_111:
                    NodeXYZ = nodes;
                    
            }
            
     
        }
        

        /// <returns> did octnode already exist</returns>
        bool SetOrCreateNodeAtIndex (ref int nodeIndex, OctNode value, List<OctNode> nodes)
        {
            if (nodeIndex > -1) //if a valid index
            {
                nodes[nodeIndex] = value;
                return true;
            }
            else
            {
                nodes.Add(value);
                nodeIndex = nodes.Count - 1;
                return false;
            }
        }

        bool GetNodeAtIndex(int nodeIndex, out OctNode value, List<OctNode> nodes)
        {
            if (nodeIndex > -1) //if a valid index
            {
                value = Empty();
                return false;
            }
            else
            {
                value = nodes[nodeIndex];
                return true;
            }
        }
        
        
        /// <returns> returns number between 0-7 which represents what octant
        /// the largest bit represents x, middle represents y, smallest bit z</returns>
        public static int AABBOctant(Vector3 point, Vector3 boxCenter)
        {
            int x = point.x > boxCenter.x ? 0b_100 : 0;
            int y = point.y > boxCenter.y ? 0b_010 : 0;
            int z = point.z > boxCenter.z ? 0b_001 : 0;

            return x | y | z;
        }
        
    }
}