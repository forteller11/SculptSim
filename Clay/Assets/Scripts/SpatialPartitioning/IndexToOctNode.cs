using Unity.Collections;
using UnityEditor.Build;

namespace SpatialPartitioning
{
    
    /* --------------------------
     * just a int under the hood
     * a way to hold an index to a **particular type** of array
     * (more type safe than just holding an int)
     ---------------------------*/
    public struct IndexToOctNode
    {
        private int _index;
        public int Index => _index;

        public IndexToOctNode(int index) => _index = index;

        public readonly OctNode GetElement(NativeArray<OctNode> list) => list[_index];

        public void SetElement(NativeArray<OctNode> list, OctNode value)
        {
            list[_index] = value;
        }

        public override string ToString() => _index.ToString();

    }
}