using UnityEditor;
using UnityEngine;

namespace SpatialPartitioning
{
    [CreateAssetMenu(fileName = "Octree Settings", menuName = "Octree", order = 1)]
    public class OctSettings : ScriptableObject
    {
        public int MaxValuesPerNode;
        public float MinHalfSize;
    }
}