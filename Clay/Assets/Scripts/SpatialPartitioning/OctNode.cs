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
     * octnode will always contain zero or 8 children, contigously distributed after FirstChildIndex
     --------------------*/
    public struct OctNode
    {
        #region members

        public AABB AABB;
        
        public IndexToOctValue LastValue;
        public int ValueCount; //a count of < 0 means the node is a leaf and is not to be added

        public int FirstChildIndex;
        
        public IndexToOctNode Child___ => new IndexToOctNode(FirstChildIndex + 0);
        public IndexToOctNode ChildX__ => new IndexToOctNode(FirstChildIndex + 1);
        public IndexToOctNode Child_Y_ => new IndexToOctNode(FirstChildIndex + 2);
        public IndexToOctNode Child__Z => new IndexToOctNode(FirstChildIndex + 3);
        public IndexToOctNode ChildXY_ => new IndexToOctNode(FirstChildIndex + 4);
        public IndexToOctNode ChildX_Z => new IndexToOctNode(FirstChildIndex + 5);
        public IndexToOctNode Child_YZ => new IndexToOctNode(FirstChildIndex + 6);
        public IndexToOctNode ChildXYZ => new IndexToOctNode(FirstChildIndex + 7);
        #endregion

        public OctNode(AABB aabb)
        {
            AABB = aabb;
            
            LastValue  = IndexToOctValue.Empty();

            ValueCount = 0;

            FirstChildIndex = int.MinValue;
        }
        
        public bool HasChildren() => FirstChildIndex >= 0;

        public readonly NativeSlice<OctNode> GetChildren(NativeArray<OctNode> nodes)
        {
            NativeSlice<OctNode> results = new NativeSlice<OctNode>(nodes, FirstChildIndex, 8);
            return results;
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

        public bool SphereOverlaps(Sphere sphere) => Intersection.SphereAABBOverlap(sphere, AABB);
        
        public bool PointOverlaps(Vector3 position) => Intersection.PointAABBOverlap(position, AABB);
        
    }
}