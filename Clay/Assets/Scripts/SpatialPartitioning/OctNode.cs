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

        public AABB AABB;
        
        public IndexToOctValue FirstValue;
        public int ValueCount;
        public int IsLeaf; //used as a bool, but is an int so it is blittable and can be stored in a nativeList<T

        public IndexToOctNode ChildXYZ;
        public IndexToOctNode Child_YZ;
        public IndexToOctNode ChildX_Z;
        public IndexToOctNode ChildXY_;
        public IndexToOctNode Child__Z;
        public IndexToOctNode ChildX__;
        public IndexToOctNode Child_Y_;
        public IndexToOctNode Child___;
        #endregion
        
        public OctNode(AABB aabb)
        {
            AABB = aabb;

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

        public readonly NativeList<OctNode> GetChildren(NativeList<OctNode> nodes)
        {
            NativeList<OctNode> results = new NativeList<OctNode>(8, Allocator.Temp);
            
            if (Child___.HasValue() ) results.Add(Child___.GetElement(nodes) );
            if (ChildX__.HasValue() ) results.Add(ChildX__.GetElement(nodes) );
            if (Child_Y_.HasValue() ) results.Add(Child_Y_.GetElement(nodes) );
            if (Child__Z.HasValue() ) results.Add(Child__Z.GetElement(nodes) );
            if (ChildXY_.HasValue() ) results.Add(ChildXY_.GetElement(nodes) );
            if (ChildX_Z.HasValue() ) results.Add(ChildX_Z.GetElement(nodes) );
            if (Child_YZ.HasValue() ) results.Add(Child_YZ.GetElement(nodes) );
            if (ChildXYZ.HasValue() ) results.Add(ChildXYZ.GetElement(nodes) );
            
            return results;
        }
        
        public OctNode GetChildFromPoint(NativeList<OctNode> nodes, Vector3 point)
        {
            var octant = OctantAtPosition(point);
            var child = GetChildNodeFromOctant(octant);
            return child.GetElement(nodes);
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
        public IndexToOctNode GetChildNodeFromOctant(Octant octant)
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
        
        ///<summary> Takes an octant and sets the corresponding child with the value</summary>
        /// <Remarks>
        /// Not persistant to array
        /// </Remarks>
        public void SetChildNodeIndexFromOctant(Octant octant, IndexToOctNode value)
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
        
        public bool SphereOverlaps(Sphere sphere) => Intersection.SphereAABBOverlap(sphere, AABB);
        public bool PointOverlaps(Vector3 position) => Intersection.PointAABBOverlap(position, AABB);
        
    }
}