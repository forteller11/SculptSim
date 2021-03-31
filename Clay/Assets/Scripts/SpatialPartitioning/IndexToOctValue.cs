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
        
        public static IndexToOctValue Empty() => new IndexToOctValue(int.MinValue);
        public static IndexToOctValue NewElement(NativeArray<OctValue> list, ref int arrayLength, OctValue value)
        {
            var newIndex = new IndexToOctValue(arrayLength);
            list[arrayLength] = value;
            arrayLength++;
            return newIndex;
        }

        public bool HasValue() => _index >= 0;
        
        public OctValue GetElement(NativeArray<OctValue> list) => list[_index];

        public void SetElement(NativeArray<OctValue> list, OctValue value) => list[_index] = value;

        public override string ToString() => _index.ToString();
        
    }
}