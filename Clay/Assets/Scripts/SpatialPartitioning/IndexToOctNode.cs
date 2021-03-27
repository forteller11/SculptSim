using Unity.Collections;

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
        
        public static IndexToOctNode Empty() => new IndexToOctNode(-1);
        
        
        public bool HasValue() => _index >= 0;
        
        public OctNode GetElement(NativeList<OctNode> list) => list[_index];

        public void SetElement(NativeList<OctNode> list, OctNode value) => list[_index] = value;
        
        public void AddElement(NativeList<OctNode> list, OctNode value)
        {
            list.Add(value);
            _index = list.Length - 1;
        }
        
        public void SetElementOrAddIfEmpty(NativeList<OctNode> list, OctNode value)
        {
            if (HasValue())
                SetElement(list, value);
            else
                AddElement(list, value);
        }

    }
}