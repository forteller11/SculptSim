
using System;
using System.Collections.Generic;
using SpatialPartitioning;
using UnityEngine;

namespace SpatialPartitioning
{
    public class Octree
    {
        public List<OctNode> Nodes;
        public List<OctValue> Values;

        public int MaxDepth = 2;
        public int MaxValuesPerNode = 8; 

        public Octree()
        {
            Nodes  = new List<OctNode> (128);
            Values = new List<OctValue>(1024);
        }

        public void CleanAndPrepareForInsertion(Vector3 worldPosition, float halfWidth)
        {
            Nodes.Clear();
            Values.Clear();
            Nodes.Add(new OctNode(this, 0, worldPosition, halfWidth));
        }

        public void Insert(Vector3 point)
        {
            var octValue = OctValue.CreateTail(point);
            Values.Add(octValue);
            int indexOfAdded = Values.Count - 1;
            Nodes[0].InsertValueInSelfOrChildren(indexOfAdded);
            //todo methods change temporary value for some reason... make sure they all effect final array
            //make methods even less oop and more functional.... entities??
        }
        
    }
}