
using System;
using SpatialPartitioning;
using UnityEngine;

namespace SpatialPartitioning
{
    [Serializable]
    public class Octree
    {
        public Vector3 WorldPosition;
        public float Scale;

        public OctNode Root;
        
    }
}