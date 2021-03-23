﻿using System;
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

        public Octree Tree;
        
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
        public OctNode(Octree tree, Vector3 center, float halfWidth)
        {
            Tree = tree;
            
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
        
        #endregion
        
        public void InsertValueInSelfOrChildren(int valueIndex)
        {
            //todo 
            if (IsLeaf)
            {
                InsertValueInSelf(valueIndex);
                //todo, if over count... place in children
            }
            //if not a leaf, find child and insert
            else
            {
                InsertValueInChildren(valueIndex);
            }
            //1) if is a leaf, just add....
            
            //2) once surpasses that...
            //create children
            //give up ownership of elements to children
            
            //3) if already gave up elements, then just call .AddElement to appropriate child.
        }

        void InsertValueInSelf(int indexOfValue)
        {
            var values = Tree.Values;

            //if no elements currently in tree
            if (FirstValueIndex < 0)
            {
                FirstValueIndex = indexOfValue;
            }
            //otherwise find last element and link to new element
            else
            {
                var lastElementIndex = values[FirstValueIndex].GetLastElementIndex(FirstValueIndex, values);
                var lastElement = values[lastElementIndex];
                values[lastElementIndex] = OctValue.WithChild(lastElement.Position, indexOfValue);
            }
        }

        void InsertValueInChildren(int indexOfValue)
        {
            var values = Tree.Values;
            var nodes = Tree.Nodes;
            
            var point = values[indexOfValue].Position;
            int octant = OctantFromAABBPoint(point);
            int childIndex = ChildNodeIndexFromOctant(octant);
            
            //if it doesn't exist, create new child
            if (childIndex < 0)
            {
                childIndex = CreateChildFromOctant(octant);
            }
            
            nodes[childIndex].InsertValueInSelfOrChildren(indexOfValue);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="octant"></param>
        /// <returns>index of child in octnode array</returns>
        int CreateChildFromOctant(int octant)
        {
            //todo 
            //create right octant with right center and halfwidth/2...
        }
        
        

        
        
        /// <returns> did octnode already exist</returns>
        bool SetOrCreateNodeAtIndex (ref int nodeIndex, OctNode value)
        {
            if (nodeIndex > -1) //if a valid index
            {
                Tree.Nodes[nodeIndex] = value;
                return true;
            }
            else
            {
                Tree.Nodes.Add(value);
                nodeIndex = Tree.Nodes.Count - 1;
                return false;
            }
        }

       
        bool GetNodeAtIndex(int nodeIndex, out OctNode value, List<OctNode> nodes)
        {
            if (nodeIndex > -1) //if a valid index
            {
                value = new OctNode(null, Vector3.negativeInfinity, Single.NaN);
                return false;
            }
            else
            {
                value = nodes[nodeIndex];
                return true;
            }
        }

        public int GetChildNodeIndexContainingPoint(Vector3 point)
        {
            int octant = OctantFromAABBPoint(point);
            int child = ChildNodeIndexFromOctant(octant);
            return child;
        }
        
        /// <returns> returns number between 0-7 which represents what octant
        /// the largest bit represents x, middle represents y, smallest bit z</returns>
        public int OctantFromAABBPoint(Vector3 point)
        {
            var boxCenter = Center;
            
            int x = point.x > boxCenter.x ? 0b_100 : 0;
            int y = point.y > boxCenter.y ? 0b_010 : 0;
            int z = point.z > boxCenter.z ? 0b_001 : 0;

            return x | y | z;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="octant"></param>
        /// <returns> returns -1 if no child exists, otherwise the index into the nodes element</returns>
        int ChildNodeIndexFromOctant(int octant)
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
        
    }
}