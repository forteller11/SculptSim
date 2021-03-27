using System;
using UnityEditor;
using UnityEngine;

namespace SpatialPartitioning
{
    [Serializable]
    public struct OctSettings
    {
        public int MaxValuesPerNode;
        public float MinHalfSize;
    }
}