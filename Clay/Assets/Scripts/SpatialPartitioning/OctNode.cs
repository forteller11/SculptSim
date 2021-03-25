using System;
using System.Collections.Generic;
using UnityEngine;


namespace SpatialPartitioning
{
    /* ---------------------
     * parent nodes will never contain elements,
     * leaf nodes will always contain elements
     --------------------*/
    public class OctNode
    {
        #region members

        public Octree Tree;

        //todo solution??
        // public int Index;
        public Vector3 Center;
        public float HalfWidth;
        
        public OctValue FirstValue;
        public int ValueCount;
        public bool IsLeaf;
        public int Depth;
        

        /* -------------
        where XYZ == plus in those axis, and _ means minus in those axis
          > so XYZ is right, upper, forward AABB
          > while ___ is left, down, backwards AABB
          >  X__ is right, dowm, backwards AAB
        --------------------------- */
        public OctNode ChildXYZ;
        public OctNode Child_YZ;
        public OctNode ChildX_Z;
        public OctNode ChildXY_;
        public OctNode Child__Z;
        public OctNode ChildX__;
        public OctNode Child_Y_;
        public OctNode Child___;
        #endregion

        #region constructors
        public OctNode(Octree tree, int depth, Vector3 center, float halfWidth)
        {
            Tree = tree;
            
            Center = center;
            HalfWidth = halfWidth;
            Depth = depth;
            
            ValueCount = 0;
            IsLeaf = true;
        }
        
        #endregion
        
        public void InsertValueInSelfOrChildren(OctValue value)
        {
            
            if (IsLeaf ||
                Depth > Tree.MaxDepth)
            {
                InsertValueInSelf(value);
                
                //if exceeded maxium allowed values, redistribute values into children
                //this node is no longer a leaf
                if (ValueCount > Tree.MaxValuesPerNode && 
                    Depth <= Tree.MaxDepth)
                {
                    IsLeaf = false;
                    
                    //copy linked-list into array
                    //this is because now the connections are implicit and can be traversed while manipulating the connections of the linked list
                    OctValue currentValue = FirstValue;
                    while (currentValue != null)
                    {
                        
                        var nextValue = currentValue.NextValue;
                        currentValue.NextValue = null;
                        
                        InsertValueInChildren(currentValue);
                        
                        currentValue = nextValue;
                    }
                    
                

                    FirstValue = null;
                }

            }
            //if not a leaf, find child and insert
            else
            {
                InsertValueInChildren(value);
            }
        }

        public OctValue GetLastValue()
        {
            OctValue currentValue = FirstValue;
            OctValue previousValue = null;
            
            while (currentValue != null)
            {
                previousValue = currentValue;
                currentValue = currentValue.NextValue;
            }

            return previousValue;
        }

        
        void InsertValueInSelf(OctValue value)
        {
            //if no values currently in node
            if (FirstValue == null)
            {
                FirstValue = value;
            }
            //otherwise find last element and link to new element
            else
            {
                var lastElement = GetLastValue();
                lastElement.NextValue = value;
            }

            ValueCount++;
        }
        
        /// <remarks> creates new children as necessary</remarks>
        void InsertValueInChildren(OctValue value)
        {
            var values = Tree.Values;
            var nodes = Tree.Nodes;
            
            var point = value.Position;
            int octant = OctantFromAABBPoint(point);
            var child = GetChildNodeFromOctant(octant);
            
            //if it doesn't exist, create new child and set to appropriate octNode child member
            if (child == null)
            {
                child = CreateChildNodeAtOctant(octant);
            }
            
            child.InsertValueInSelfOrChildren(value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="octant"></param>
        /// <returns>index of child in octnode array</returns>
        OctNode CreateChildNodeAtOctant(int octant)
        {
            var tree = Tree;
            var nodes = Tree.Nodes;
            //todo octantpos/2 maybe?
            var octantPosition = OctantToVector3Int(octant);
            var quarterWidth = HalfWidth / 2;
            var childOffset = (Vector3) octantPosition * quarterWidth;
            var childPos = Center + childOffset;

            var newOctNode = new OctNode(tree, Depth + 1, childPos, quarterWidth);
            
            nodes.Add(newOctNode);
            SetChildNodeFromOctant(octant, newOctNode);

            return newOctNode;
        }

        ///<returns>returns values between [-1,-1,-1] and [1,1,1]</returns>
        Vector3Int OctantToVector3Int(int octant)
        {
            int x = (octant & 0b_100) >> 2;
            int y = (octant & 0b_010) >> 1;
            int z = (octant & 0b_001) >> 0;

            var vec = (new Vector3Int(x, y, z));
            var scaledBetweenOneAndMinusOne = (vec * 2) - new Vector3Int(-1, -1, -1);
            //todo debug
            return scaledBetweenOneAndMinusOne;
        }
        
        
        
        //
        // public int GetChildNodeIndexContainingPoint(Vector3 point)
        // {
        //     int octant = OctantFromAABBPoint(point);
        //     int child = ChildNodeIndexFromOctant(octant);
        //     return child;
        // }
        
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
        OctNode GetChildNodeFromOctant(int octant)
        {
            switch (octant)
            {
                case 0b_111: return ChildXYZ;
                case 0b_011: return Child_YZ;
                case 0b_101: return ChildX_Z;
                case 0b_110: return ChildXY_;
                case 0b_001: return Child__Z;
                case 0b_100: return ChildX__;
                case 0b_010: return Child_Y_;
                case 0b_000: return Child___;
                default: throw new ArgumentException("octant must be between values 0 to 7!");
            }
        }
        
        void SetChildNodeFromOctant(int octant, OctNode value)
        {
            switch (octant)
            {
                case 0b_111: ChildXYZ = value; return;
                case 0b_011: Child_YZ = value; return;
                case 0b_101: ChildX_Z = value; return;
                case 0b_110: ChildXY_ = value; return;
                case 0b_001: Child__Z = value; return;
                case 0b_100: ChildX__ = value; return;
                case 0b_010: Child_Y_ = value; return;
                case 0b_000: Child___ = value; return;
                default: throw new ArgumentException("octant must be between values 0 to 7!");
            }
        }
        
    }
}