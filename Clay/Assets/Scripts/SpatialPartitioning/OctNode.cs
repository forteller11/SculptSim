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
        
        public OctNode(NativeList<OctNode> nodes, NativeList<OctValue> values, OctSettings settings, AABB aabb)
        {
            Nodes = nodes;
            Values = values;
            Settings = settings;
            AABB = aabb;

            FirstValue = IndexToValue<OctValue>.Empty();
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

        public void InsertValueInSelfOrChildren(OctValue value)
        {
            if (IsLeaf)
            {
                InsertValueInSelf(value);

                float theoreticalChildHalfWidth = AABB.HalfWidth / 2f;
                //if exceeded maxium allowed values,
                //and child would not be less than min half width
                if (ValueCount > Settings.MaxValuesPerNode && 
                    theoreticalChildHalfWidth > Settings.MinHalfSize)
                {
                    //redistribute values into children
                    IndexToValue<OctValue> currentValueIndex = FirstValue;
                    while (currentValueIndex.HasValue())
                    {
                        var currentValue = currentValueIndex.GetElement(Values);
                        var nextValueIndexCache = currentValue.NextValue;
                        
                        //break up linked list (child will reconstruct it appropriately)
                        currentValue.NextValue = IndexToValue<OctValue>.Empty();
                        currentValueIndex.SetElement(Values, currentValue);
                        
                        InsertValueInChildren(currentValue);
                        
                        currentValueIndex = nextValueIndexCache;
                    }

                    //this node is no longer a leaf, revoke ownership of values
                    FirstValue = IndexToValue<OctValue>.Empty();
                    IsLeaf = false; //todo: remove use of IsLeaf by just using if (valueCount > SpecialReallyBigNumber)
                    ValueCount = 0;
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
        
        public int GetValues(out NativeList<OctValue> results, Allocator allocator = Allocator.Temp)
        {
            results = new NativeList<OctValue>(allocator);
            results.Capacity = Settings.MaxValuesPerNode;
            
            IndexToValue<OctValue> currentValueIndex = FirstValue;
            while (currentValueIndex.HasValue())
            {
                var currentElement = currentValueIndex.GetElement(Values); 
                currentValueIndex = currentElement.NextValue;
                results.Add(currentElement);
            }

            return results.Length;
        }
        
        //todo turn into array/native... block allocate children all 8 at once, store other children implicitely
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
            return child.GetElement(Nodes);
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
            var childIndex = GetChildNodeFromOctant(octant);
            
            //if it doesn't exist, create new child and set to appropriate octNode child member
            if (childIndex.HasValue())
            {
                childIndex = CreateChildNodeAtOctant(octant);
            }
            
            childIndex.GetElement(Nodes).InsertValueInSelfOrChildren(value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="octant"></param>
        /// <returns>index of child in octnode array</returns>
        IndexToValue<OctNode> CreateChildNodeAtOctant(Octant octant)
        {
            var octantPosition = OctantToVector3Int(octant);
            var quarterWidth = AABB.HalfWidth / 2;
            var childOffset = (Vector3) octantPosition * quarterWidth;
            var childPos = AABB.Center + childOffset;

            var newOctNode = new OctNode(Nodes, Values, Settings, new AABB(childPos, quarterWidth));

            var childIndex = IndexToValue<OctNode>.Empty();
            childIndex.AddElement(Nodes, newOctNode);
            SetChildNodeFromOctant(octant, newOctNode);

            return childIndex;
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
        IndexToValue<OctNode> GetChildNodeFromOctant(Octant octant)
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
                case Octant.XYZ: ChildXYZ.SetElement(Nodes, value); return;
                case Octant._YZ: Child_YZ.SetElement(Nodes, value); return;
                case Octant.X_Z: ChildX_Z.SetElement(Nodes, value); return;
                case Octant.XY_: ChildXY_.SetElement(Nodes, value); return;
                case Octant.__Z: Child__Z.SetElement(Nodes, value); return;
                case Octant.X__: ChildX__.SetElement(Nodes, value); return;
                case Octant._Y_: Child_Y_.SetElement(Nodes, value); return;
                case Octant.___: Child___.SetElement(Nodes, value); return;
                default: throw new ArgumentException("octant must be between values 0 to 7!");
            }
        }
        
        public bool SphereOverlaps(Sphere sphere) => Intersection.SphereAABBOverlap(sphere, AABB);
        public bool PointOverlaps(Vector3 position) => Intersection.PointAABBOverlap(position, AABB);
        
    }
}