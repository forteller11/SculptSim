using System;
using System.Collections.Generic;
using SpatialPartitioning;
using Unity.Collections;
using UnityEngine;


namespace SpatialPartitioning
{

    /* -------------
        where XYZ == plus in those axis, and _ means minus in those axis
          > so XYZ is right, upper, forward AABB
          > while ___ is left, down, backwards AABB
          >  X__ is right, dowm, backwards AAB
        --------------------------- */
    [Flags]
    public enum Octant
    {
        ___ = 0b_000,
        X__ = 0b_100,
        _Y_ = 0b_010,
        __Z = 0b_001,
        XY_ = X__ | _Y_,
        X_Z = X__ | __Z,
        _YZ = _Y_ | __Z,
        XYZ = X__ | _Y_ | __Z,
    };
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

        public void ForEachValue(Action<OctValue> action)
        {
            OctValue currentValue = FirstValue;
            while (currentValue != null)
            {
                action.Invoke(currentValue);
                currentValue = currentValue.NextValue;
            }
        }
        
        public void ForEachChild(Action<OctNode> action)
        {
            if (Child___ != null) action.Invoke(Child___);
            if (ChildX__ != null) action.Invoke(ChildX__);
            if (Child_Y_ != null) action.Invoke(Child_Y_);
            if (Child__Z != null) action.Invoke(Child__Z);
            if (ChildXY_ != null) action.Invoke(ChildXY_);
            if (ChildX_Z != null) action.Invoke(ChildX_Z);
            if (Child_YZ != null) action.Invoke(Child_YZ);
            if (ChildXYZ != null) action.Invoke(ChildXYZ);
        }

        //todo make getOctantS from AABB.
        //todo get children from octant
        public OctNode GetChildFromPoint(Vector3 point)
        {
            var octant = OctantAtPosition(point);
            var child = GetChildNodeFromOctant(octant);
            return child;
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
            var point = value.Position;
            var octant = OctantAtPosition(point);
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
        OctNode CreateChildNodeAtOctant(Octant octant)
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
        Vector3Int OctantToVector3Int(Octant octant)
        {
            int x = (int) (octant & Octant.X__) >> 2;
            int y = (int) (octant & Octant._Y_) >> 1;
            int z = (int) (octant & Octant.__Z) >> 0;

            var vec = (new Vector3Int(x, y, z));
            var scaledBetweenOneAndMinusOne = (vec * 2) - new Vector3Int(1, 1, 1);
            //todo debug
            return scaledBetweenOneAndMinusOne;
        }

        /// <returns> returns number between 0-7 which represents what octant
        /// the largest bit represents x, middle represents y, smallest bit z</returns>
        public Octant OctantAtPosition(Vector3 point)
        {
            var boxCenter = Center;
            
            Octant x = point.x > boxCenter.x ? Octant.X__ : 0;
            Octant y = point.y > boxCenter.y ? Octant._Y_ : 0;
            Octant z = point.z > boxCenter.z ? Octant.__Z : 0;

            return x | y | z;
        }
        
        // public NativeArray<int> OctantsAtPosition(Vector3 point)
        // {
        //     var boxCenter = Center;
        //     
        //     int x = point.x > boxCenter.x ? 0b_100 : 0;
        //     int y = point.y > boxCenter.y ? 0b_010 : 0;
        //     int z = point.z > boxCenter.z ? 0b_001 : 0;
        //
        //     return x | y | z;
        // }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="octant"></param>
        /// <returns> returns -1 if no child exists, otherwise the index into the nodes element</returns>
        OctNode GetChildNodeFromOctant(Octant octant)
        {
            switch (octant)
            {
                case Octant.XYZ: return ChildXYZ;
                case Octant._YZ: return Child_YZ;
                case Octant.X_Z: return ChildX_Z;
                case Octant.XY_: return ChildXY_;
                case Octant.__Z: return Child__Z;
                case Octant.X__: return ChildX__;
                case Octant._Y_: return Child_Y_;
                case Octant.___: return Child___;
                default: throw new ArgumentException("octant must be between values 0 to 7!");
            }
        }
        
        void SetChildNodeFromOctant(Octant octant, OctNode value)
        {
            switch (octant)
            {
                case Octant.XYZ: ChildXYZ = value; return;
                case Octant._YZ: Child_YZ = value; return;
                case Octant.X_Z: ChildX_Z = value; return;
                case Octant.XY_: ChildXY_ = value; return;
                case Octant.__Z: Child__Z = value; return;
                case Octant.X__: ChildX__ = value; return;
                case Octant._Y_: Child_Y_ = value; return;
                case Octant.___: Child___ = value; return;
                default: throw new ArgumentException("octant must be between values 0 to 7!");
            }
        }
        
    }
}