
using System;
using System.Collections.Generic;
using Collision;
using Unity.Collections;
using UnityEngine;

namespace SpatialPartitioning
{
    public class Octree : IDisposable
    {
        public NativeList<OctNode> Nodes;
        public NativeList<OctValue> Values;
        public OctSettings Settings;

        public Octree(OctSettings settings)
        {
            Nodes  = new NativeList<OctNode> (128,  Allocator.Persistent);
            Values = new NativeList<OctValue>(1024, Allocator.Persistent);
            Settings = settings;
        }

        public void CleanAndPrepareForInsertion(AABB aabb)
        {
            Nodes.Clear();
            Values.Clear();
            Nodes.Add(new OctNode(new IndexToOctNode(Nodes.Length), aabb,Settings));
        }

        public void Insert(Vector3 point)
        {
            var root = Nodes[0];
            if (root.PointOverlaps(point))
            {
                var octValue = OctValue.CreateTail(point);
                Values.Add(octValue);
                root.InsertValueInSelfOrChildren(Nodes, Values, octValue);
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
            if (node.IsLeaf != 0)
            {
                //todo remove closure allocation...
                node.ForEachChild(Nodes, (child) =>
                {
                    if (child.SphereOverlaps(sphere))
                        GetOverlappingChildrenOrAddToResultsDepthFirst(sphere, child, results);
                });
            }
            else
            {
                node.GetValues(Values, out var values);
                for (int i = 0; i < values.Length; i++)
                    results.Add(values[i].Position);
                values.Dispose();
            }
        }

        public void Dispose()
        {
            Nodes.Dispose();
            Values.Dispose();
        }
    }
}