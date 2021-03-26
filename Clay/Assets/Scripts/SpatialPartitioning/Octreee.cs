
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

        public int MaxDepth = 2;
        public int MaxValuesPerNode = 3; 

        public Octree()
        {
            Nodes  = new List<OctNode> (128);
            Values = new List<OctValue>(1024);
        }

        public void CleanAndPrepareForInsertion(AABB aabb)
        {
            Nodes.Clear();
            Values.Clear();
            Nodes.Add(new OctNode(this, 0, aabb));
        }

        public void Insert(Vector3 point)
        {
            var octValue = OctValue.CreateTail(Values.Count.ToString(), point);
            Values.Add(octValue);
            Nodes[0].InsertValueInSelfOrChildren(octValue);
        }

        public bool QueryNonAlloc(Sphere sphere, List<Vector3> results)
        {
            var root = Nodes[0];
            if (root.OverlapsSphere(sphere))
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
                    if (child.OverlapsSphere(sphere))
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