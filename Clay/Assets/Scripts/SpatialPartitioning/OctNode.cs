using System;
using System.Collections.Generic;
using ClaySimulation;
using Collision;
using SpatialPartitioning;
using Unity.Collections;
using UnityEngine;


namespace SpatialPartitioning
{
    /* ---------------------
     * parent nodes will never contain elements,
     * leaf nodes will always contain elements
     --------------------*/
    public struct OctNode
    {
        #region members
        
        public OctSettings Settings;
        public NativeList<OctValue> Values;
        public NativeList<OctNode> Nodes;
        
        public AABB AABB;
        
        public IndexToValue<OctValue> FirstValue;
        public int ValueCount;
        public bool IsLeaf;

        public IndexToValue<OctNode> ChildXYZ;
        public IndexToValue<OctNode> Child_YZ;
        public IndexToValue<OctNode> ChildX_Z;
        public IndexToValue<OctNode> ChildXY_;
        public IndexToValue<OctNode> Child__Z;
        public IndexToValue<OctNode> ChildX__;
        public IndexToValue<OctNode> Child_Y_;
        public IndexToValue<OctNode> Child___;
        #endregion

        #region constructors
        public OctNode(Octree tree, AABB aabb)
        {
            AABB = aabb;

            ValueCount = 0;
            IsLeaf = true;
            
            ChildXYZ = IndexToValue<OctNode>.Empty();
            Child_YZ = IndexToValue<OctNode>.Empty();
            ChildX_Z = IndexToValue<OctNode>.Empty();
            ChildXY_ = IndexToValue<OctNode>.Empty();
            Child__Z = IndexToValue<OctNode>.Empty();
            ChildX__ = IndexToValue<OctNode>.Empty();
            Child_Y_ = IndexToValue<OctNode>.Empty();
            Child___ = IndexToValue<OctNode>.Empty();
        }
        
        #endregion
        
        public void InsertValueInSelfOrChildren(OctValue value)
        {
            
            if (IsLeaf)
            {
                InsertValueInSelf(value);

                float theoreticalChildHalfWidths = AABB.HalfWidth / 2f;
                //if exceeded maxium allowed values,
                //and child would not be less than min half width
                if (ValueCount > Settings.MaxValuesPerNode && 
                    theoreticalChildHalfWidths > Settings.MinHalfSize)
                {
                    //redistribute values into children
                    OctValue currentValue = FirstValue.GetElement();
                    while (currentValue.NextValue.HasValue())
                    {
                        var nextValue = currentValue.NextValue;
                        currentValue.NextValue = IndexToValue<OctValue>.Empty();
                        
                        InsertValueInChildren(currentValue);
                        
                        currentValue = nextValue;
                    }

                    //this node is no longer a leaf, revoke ownership of values
                    IsLeaf = false;
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
            IndexToValue<OctValue> currentValue = new IndexToValue<OctValue>();
            IndexToValue<OctValue> previousValue = IndexToValue<OctValue>.Empty();
            
            while (currentValue.HasValue())
            {
                previousValue = currentValue;
                currentValue = currentValue.GetElement(Values).NextValue;
            }

            if (!previousValue.HasValue())
                throw new Exception("Cant call last value if there isn't a first value!");
            
            return previousValue.GetElement(Values);
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
            if (Child___.HasValue() ) action.Invoke(Child___.GetElement(Nodes) );
            if (ChildX__.HasValue() ) action.Invoke(ChildX__.GetElement(Nodes) );
            if (Child_Y_.HasValue() ) action.Invoke(Child_Y_.GetElement(Nodes) );
            if (Child__Z.HasValue() ) action.Invoke(Child__Z.GetElement(Nodes) );
            if (ChildXY_.HasValue() ) action.Invoke(ChildXY_.GetElement(Nodes) );
            if (ChildX_Z.HasValue() ) action.Invoke(ChildX_Z.GetElement(Nodes) );
            if (Child_YZ.HasValue() ) action.Invoke(Child_YZ.GetElement(Nodes) );
            if (ChildXYZ.HasValue() ) action.Invoke(ChildXYZ.GetElement(Nodes) );
        }
        
        public OctNode GetChildFromPoint(Vector3 point)
        {
            var octant = OctantAtPosition(point);
            var child = GetChildNodeFromOctant(octant);
            return child;
        }
        
        void InsertValueInSelf(OctValue value)
        {
            //if no values currently in node
            if (!FirstValue.HasValue())
            {
                FirstValue.AddElement(Values, value);
            }
            //otherwise find last element and link to new element
            else
            {
                var lastElement = GetLastValue();
                lastElement.NextValue.AddElement(Values, value);
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
            
            var octantPosition = OctantToVector3Int(octant);
            var quarterWidth = AABB.HalfWidth / 2;
            var childOffset = (Vector3) octantPosition * quarterWidth;
            var childPos = AABB.Center + childOffset;

            var newOctNode = new OctNode(tree, new AABB(childPos, quarterWidth));
            
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

            Octant x = point.x > AABB.Center.x ? Octant.X__ : 0;
            Octant y = point.y > AABB.Center.y ? Octant._Y_ : 0;
            Octant z = point.z > AABB.Center.z ? Octant.__Z : 0;

            return x | y | z;
        }
        
        // public List<OctNode> ChildrenInsideSphere(Sphere sphere)
        // {
        //     //todo convert to struct then native list NativeList<OctNode>(Allocator.Temp);
        //     List<OctNode> results = new List<OctNode>();
        //     ForEachChild((child) =>
        //     {
        //         if (Common.SphereAABBOverlap(sphere, AABB))
        //         {
        //             results.Add(child);
        //         }
        //     });
        //     return results;
        // }

        public bool SphereOverlaps(Sphere sphere) => Intersection.SphereAABBOverlap(sphere, AABB);
        public bool PointOverlaps(Vector3 position) => Intersection.PointAABBOverlap(position, AABB);
        
        
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