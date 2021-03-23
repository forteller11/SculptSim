
using System;
using System.Collections.Generic;
using SpatialPartitioning;
using UnityEngine;

namespace SpatialPartitioning
{
    [Serializable]
    public class Octree
    {
        public List<OctNode> Nodes;
        public List<OctValue> Values;

        public Octree()
        {
            Nodes  = new List<OctNode> (128);
            Values = new List<OctValue>(1024);
        }
        
        public void ConstructTree(Vector3 worldPosition, float halfWidth, List<Vector3> particles)
        {
            Nodes.Add(new OctNode(worldPosition, halfWidth));

            for (int i = 0; i < UPPER; i++)
            {
                
            }
            //todo construct recursively
        }

    }
}