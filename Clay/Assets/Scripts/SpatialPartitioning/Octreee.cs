
using System;
using System.Collections.Generic;
using ClaySimulation;
using Collision;
using SpatialPartitioning;
using UnityEngine;

namespace SpatialPartitioning
{
    public class Octree
    {
        public List<OctNode> Nodes;
        public List<OctValue> Values;

        public float MinHalfSize;
        public int MaxValuesPerNode = 16; 

        public Octree()
        {
            Nodes  = new List<OctNode> (128);
            Values = new List<OctValue>(1024);
        }

        public void CleanAndPrepareForInsertion(AABB aabb, float maxHalfSize, int maxValuesPerNode)
        {
            MaxValuesPerNode = maxValuesPerNode;
            MinHalfSize = maxHalfSize;
            Nodes.Clear();
            Values.Clear();
            Nodes.Add(new OctNode(this, aabb));
        }

        public void Insert(Vector3 point)
        {
            var root = Nodes[0];
            if (root.PointOverlaps(point))
            {
                var octValue = OctValue.CreateTail(point);
                Values.Add(octValue);
                root.InsertValueInSelfOrChildren(octValue);
            }
            else
            {
                throw new ArgumentException("Cannot insert point outside of octree!");
            }
            
        }

        public bool QueryNonAlloc(Sphere sphere, List<Vector3> results)
        {
            var root = Nodes[0];
            if (root.SphereOverlaps(sphere))
                GetOverlappingChildrenOrAddToResultsDepthFirst(sphere, root, results);
        
            return results.Count > 0;
        }

        void GetOverlappingChildrenOrAddToResultsDepthFirst(Sphere sphere, OctNode node, List<Vector3> results)
        {
            if (!node.IsLeaf)
            {
                //todo remove closure allocation...
                node.ForEachChild((child) =>
                {
                    if (child.SphereOverlaps(sphere))
                        GetOverlappingChildrenOrAddToResultsDepthFirst(sphere, child, results);
                });
            }
            else
            {
                node.ForEachValue((value) =>
                {
                    results.Add(value.Position);
                });
            }
        }
        
    }
}