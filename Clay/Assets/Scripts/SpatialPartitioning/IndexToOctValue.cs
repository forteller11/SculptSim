using System;
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
        
        public static IndexToOctValue Empty() => new IndexToOctValue(-int.MaxValue);
        public static IndexToOctValue NewElement(NativeList<OctValue> list, OctValue value)
        {
            var index = new IndexToOctValue(-int.MaxValue);
            index.AddElement(list, value);
            return index;
        }
        
        
        public bool HasValue() => _index >= 0;
        
        public OctValue GetElement(NativeList<OctValue> list) => list[_index];

        public void SetElement(NativeList<OctValue> list, OctValue value) => list[_index] = value;
        
        void AddElement(NativeList<OctValue> list, OctValue value)
        {
            _index = list.Length;
            list.Add(value);
        }

        public override string ToString() => _index.ToString();
        
    }
}