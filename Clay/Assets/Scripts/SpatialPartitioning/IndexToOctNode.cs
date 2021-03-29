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
        public static IndexToOctNode NewElement(NativeList<OctNode> list, OctNode value)
        {
            var index = new IndexToOctNode(-1);
            index.AddElement(list, value);
            return index;
        }


        public bool HasValue() => _index >= 0;
        
        public OctNode GetElement(NativeList<OctNode> list) => list[_index];

        public void SetElement(NativeList<OctNode> list, OctNode value) => list[_index] = value;
        
        void AddElement(NativeList<OctNode> list, OctNode value)
        {
            _index = list.Length;
            list.Add(value);
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