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
        
        public AABB AABB;
        
        public IndexToOctValue FirstValue;
        public int ValueCount;
        public int IsLeaf; //used as a bool, but is an int so it is blittable and can be stored in a nativeList<T

        public IndexToOctNode SelfIndex;
        
        public IndexToOctNode ChildXYZ;
        public IndexToOctNode Child_YZ;
        public IndexToOctNode ChildX_Z;
        public IndexToOctNode ChildXY_;
        public IndexToOctNode Child__Z;
        public IndexToOctNode ChildX__;
        public IndexToOctNode Child_Y_;
        public IndexToOctNode Child___;
        #endregion
        
        public OctNode(IndexToOctNode index, AABB aabb, OctSettings settings)
        {
            SelfIndex = index;
            AABB = aabb;
            Settings = settings;

            FirstValue = IndexToOctValue.Empty();
            ValueCount = 0;
            IsLeaf = 1;

            ChildXYZ = IndexToOctNode.Empty();
            Child_YZ = IndexToOctNode.Empty();
            ChildX_Z = IndexToOctNode.Empty();
            ChildXY_ = IndexToOctNode.Empty();
            Child__Z = IndexToOctNode.Empty();
            ChildX__ = IndexToOctNode.Empty();
            Child_Y_ = IndexToOctNode.Empty();
            Child___ = IndexToOctNode.Empty();
        }

        public void InsertValueInSelfOrChildren(NativeList<OctNode> nodes, NativeList<OctValue> values, OctValue value)
        {
            if (IsLeaf != 0)
            {
                InsertValueInSelf(values, value);

                float theoreticalChildHalfWidth = AABB.HalfWidth / 2f;
                //if exceeded maxium allowed values,
                //and child would not be less than min half width
                if (ValueCount > Settings.MaxValuesPerNode && 
                    theoreticalChildHalfWidth > Settings.MinHalfSize)
                {
                    //redistribute values into children
                    IndexToOctValue currentValueIndex = FirstValue;
                    while (currentValueIndex.HasValue())
                    {
                        var currentValue = currentValueIndex.GetElement(values);
                        var nextValueIndexCache = currentValue.NextValue;
                        
                        //break up linked list (child will reconstruct it appropriately)
                        currentValue.NextValue = IndexToOctValue.Empty();
                        currentValueIndex.SetElement(values, currentValue);
                        
                        InsertValueInChildren(nodes, values, currentValue);
                        
                        currentValueIndex = nextValueIndexCache;
                    }

                    //this node is no longer a leaf, revoke ownership of values
                    FirstValue = IndexToOctValue.Empty();
                    IsLeaf = 0; //todo: remove use of IsLeaf by just using if (valueCount > SpecialReallyBigNumber)
                    ValueCount = 0;
                }

            }
            //if not a leaf, find child and insert
            else
            {
                InsertValueInChildren(nodes, values, value);
            }
            
            //persist values set via this method
            SelfIndex.SetElement(nodes, this);
        }

        public OctValue GetLastValue(NativeList<OctValue> values)
        {
            IndexToOctValue currentValue = new IndexToOctValue();
            IndexToOctValue previousValue = IndexToOctValue.Empty();
            
            while (currentValue.HasValue())
            {
                previousValue = currentValue;
                currentValue  = currentValue.GetElement(values).NextValue;
            }

            if (!previousValue.HasValue())
                throw new Exception("Cant call last value if there isn't a first value!");
            
            return previousValue.GetElement(values);
        }
        
        public int GetValues(NativeList<OctValue> values, out NativeList<OctValue> results, Allocator allocator = Allocator.Temp)
        {
            results = new NativeList<OctValue>(Settings.MaxValuesPerNode, allocator);

            IndexToOctValue currentValueIndex = FirstValue;
            while (currentValueIndex.HasValue())
            {
                var currentElement = currentValueIndex.GetElement(values); 
                currentValueIndex = currentElement.NextValue;
                results.Add(currentElement);
            }

            return results.Length;
        }
        
        //todo turn into array/native... block allocate children all 8 at once, store other children implicitely
        public void ForEachChild(NativeList<OctNode> nodes, Action<OctNode> action)
        {
            if (Child___.HasValue() ) action.Invoke(Child___.GetElement(nodes) );
            if (ChildX__.HasValue() ) action.Invoke(ChildX__.GetElement(nodes) );
            if (Child_Y_.HasValue() ) action.Invoke(Child_Y_.GetElement(nodes) );
            if (Child__Z.HasValue() ) action.Invoke(Child__Z.GetElement(nodes) );
            if (ChildXY_.HasValue() ) action.Invoke(ChildXY_.GetElement(nodes) );
            if (ChildX_Z.HasValue() ) action.Invoke(ChildX_Z.GetElement(nodes) );
            if (Child_YZ.HasValue() ) action.Invoke(Child_YZ.GetElement(nodes) );
            if (ChildXYZ.HasValue() ) action.Invoke(ChildXYZ.GetElement(nodes) );
        }
        
        public OctNode GetChildFromPoint(NativeList<OctNode> nodes, Vector3 point)
        {
            var octant = OctantAtPosition(point);
            var child = GetChildNodeFromOctant(octant);
            return child.GetElement(nodes);
        }
        
        void InsertValueInSelf(NativeList<OctValue> values, OctValue value)
        {
            //if no values currently in node
            if (!FirstValue.HasValue())
            {
                FirstValue.AddElement(values, value);
            }
            //otherwise find last element and link to new element
            else
            {
                var lastElement = GetLastValue(values);
                lastElement.NextValue.AddElement(values, value);
            }

            ValueCount++;
        }
        
        /// <remarks> creates new children as necessary</remarks>
        void InsertValueInChildren(NativeList<OctNode> nodes, NativeList<OctValue> values, OctValue value)
        {
            var point = value.Position;
            var octant = OctantAtPosition(point);
            var childIndex = GetChildNodeFromOctant(octant);
            
            //if it doesn't exist, create new child and set to appropriate octNode child member
            if (!childIndex.HasValue())
            {
                childIndex = CreateChildNodeAtOctant(nodes, octant);
            }
            
            childIndex.GetElement(nodes).InsertValueInSelfOrChildren(nodes, values, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="octant"></param>
        /// <returns>index of child in octnode array</returns>
        IndexToOctNode CreateChildNodeAtOctant(NativeList<OctNode> nodes, Octant octant)
        {
            var octantPosition = OctantToVector3Int(octant);
            var quarterWidth = AABB.HalfWidth / 2;
            var childOffset = (Vector3) octantPosition * quarterWidth;
            var childPos = AABB.Center + childOffset;

            var childNode = new OctNode(new IndexToOctNode(nodes.Length), new AABB(childPos, quarterWidth), Settings);
            var childNodeIndex = IndexToOctNode.Empty();
            childNodeIndex.AddElement(nodes, childNode);
            
            SetChildNodeFromOctant(nodes, octant, childNode);
            SelfIndex.SetElement(nodes, this);

            return childNodeIndex;
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="octant"></param>
        /// <returns> returns -1 if no child exists, otherwise the index into the nodes element</returns>
        IndexToOctNode GetChildNodeFromOctant(Octant octant)
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
        
        void SetChildNodeFromOctant(NativeList<OctNode> nodes, Octant octant, OctNode value)
        {
            switch (octant)
            {
                case Octant.XYZ: ChildXYZ.SetElement(nodes, value); return;
                case Octant._YZ: Child_YZ.SetElement(nodes, value); return;
                case Octant.X_Z: ChildX_Z.SetElement(nodes, value); return;
                case Octant.XY_: ChildXY_.SetElement(nodes, value); return;
                case Octant.__Z: Child__Z.SetElement(nodes, value); return;
                case Octant.X__: ChildX__.SetElement(nodes, value); return;
                case Octant._Y_: Child_Y_.SetElement(nodes, value); return;
                case Octant.___: Child___.SetElement(nodes, value); return;
                default: throw new ArgumentException("octant must be between values 0 to 7!");
            }
        }
        
        public bool SphereOverlaps(Sphere sphere) => Intersection.SphereAABBOverlap(sphere, AABB);
        public bool PointOverlaps(Vector3 position) => Intersection.PointAABBOverlap(position, AABB);
        
    }
}