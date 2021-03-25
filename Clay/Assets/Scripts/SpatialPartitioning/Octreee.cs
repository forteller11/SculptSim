
using System;
using System.Collections.Generic;
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

        public bool GetNeighors(Sphere sphere, List<Vector3> results)
        {
            OctNode currentQuad = Nodes[0];
            while (!currentQuad.IsLeaf)
            {
                var children = currentQuad.ChildrenInsideSphere(sphere);

                for (int i = 0; i < children; i++)
                {
                    //if leaf... add to results, otherwise, recursively go init...
                    children.ChildrenInsideSphere();
                    
                    //todo make this part recursive
                }
            }
            
            currentQuad.ForEachValue(value =>
            {
                results.Add(value.Position);
            });
            
            //todo get radius
        
            return results.Count > 0;
        }
        
    }
}