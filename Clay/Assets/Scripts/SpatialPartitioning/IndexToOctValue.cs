using Unity.Collections;

namespace SpatialPartitioning
{
    
    /* --------------------------
     * just a int under the hood
     * a way to hold an index to a **particular type** of array
     * (more type safe than just holding an int)
     ---------------------------*/
    public struct IndexToOctValue
    {
        private int _index;
        public int Index => _index;

        public IndexToOctValue(int index) => _index = index;
        
        public static IndexToOctValue Empty() => new IndexToOctValue(-1);
        
        
        public bool HasValue() => _index >= 0;
        
        public OctValue GetElement(NativeList<OctValue> list) => list[_index];

        public void SetElement(NativeList<OctValue> list, OctValue value) => list[_index] = value;
        
        public void AddElement(NativeList<OctValue> list, OctValue value)
        {
            list.Add(value);
            _index = list.Length - 1;
        }
        
        public void SetElementOrAddIfEmpty(NativeList<OctValue> list, OctValue value)
        {
            if (HasValue())
                SetElement(list, value);
            else
                AddElement(list, value);
        }

    }
}