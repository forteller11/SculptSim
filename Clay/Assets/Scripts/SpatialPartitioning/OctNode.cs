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
        #region members
        public Vector3 Center;
        public float HalfWidth;
        
        public int FirstValueIndex;
        public int ValueCount;
        public bool IsLeaf;
        
        /* -------------
        where XYZ == plus in those axis, and _ means minus in those axis
          > so XYZ is right, upper, forward AABB
          > while ___ is left, down, backwards AABB
          >  X__ is right, dowm, backwards AAB
        --------------------------- */
        public int NodeXYZ;
        public int Node_YZ;
        public int NodeX_Z;
        public int NodeXY_;
        public int Node__Z;
        public int NodeX__;
        public int Node_Y_;
        public int Node___;
        #endregion

        #region constructors
        public OctNode(Vector3 center, float halfWidth)
        {
            Center = center;
            HalfWidth = halfWidth;

            FirstValueIndex = -1;
            ValueCount = 0;
            IsLeaf = true;
            
            NodeXYZ = -1;
            Node_YZ = -1;
            NodeX_Z = -1;
            NodeXY_ = -1;
            Node__Z = -1;
            NodeX__ = -1;
            Node_Y_ = -1;
            Node___ = -1;
       
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
        #endregion
        
        void AddElement(Vector3 point, List<OctValue> values)
        {
            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="octant"></param>
        /// <returns> returns -1 if no child exists, otherwise the index into the nodes element</returns>
        int IndexFromOctant(int octant)
        {
            switch (octant)
            {
                case 0b_111: return NodeXYZ;
                case 0b_011: return Node_YZ;
                case 0b_101: return NodeX_Z;
                case 0b_110: return NodeXY_;
                case 0b_001: return Node__Z;
                case 0b_100: return NodeX__;
                case 0b_010: return Node_Y_;
                case 0b_000: return Node___;
                default: throw new ArgumentException("octant must be between values 0 to 7!");
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