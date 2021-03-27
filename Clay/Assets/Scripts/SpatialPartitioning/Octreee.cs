
using System;
using System.Collections.Generic;
using ClaySimulation;
using Collision;
using SpatialPartitioning;
using Unity.Collections;
using UnityEngine;

namespace SpatialPartitioning
{
    public class Octree
    {
        public NativeList<OctNode> Nodes;
        public NativeList<OctValue> Values;
        public OctSettings Settings;

        public float MinHalfSize;
        public int MaxValuesPerNode = 16; 

        public Octree(OctSettings settings)
        {
            Nodes  = new List<OctNode> (128);
            Values = new List<OctValue>(1024);
            Settings = settings;
        }

        public void CleanAndPrepareForInsertion(AABB aabb, float maxHalfSize, int maxValuesPerNode)
        {
            MaxValuesPerNode = maxValuesPerNode;
            MinHalfSize = maxHalfSize;
            Nodes.Clear();
            Values.Clear();
            Nodes.Add(new OctNode(Nodes, Values, Settings, aabb));
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

        public bool QueryNonAlloc(Sphere sphere, NativeList<Vector3> results)
        {
            var root = Nodes[0];
            if (root.SphereOverlaps(sphere))
                GetOverlappingChildrenOrAddToResultsDepthFirst(sphere, root, results);
        
            return results.Length > 0;
        }

        void GetOverlappingChildrenOrAddToResultsDepthFirst(Sphere sphere, OctNode node, NativeList<Vector3> results)
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
                node.GetValues(out var values);

                for (int i = 0; i < values.Length; i++)
                {
                    results.Add(values[i].Position);
                }
            }
        }
        
    }
}