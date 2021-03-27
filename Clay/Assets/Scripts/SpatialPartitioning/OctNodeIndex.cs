using Unity.Collections;

namespace SpatialPartitioning
{
    
    /* --------------------------
     * just a int under the hood
     * a way to hold an index to a **particular type** of array
     * (more type safe than just holding an int)
     ---------------------------*/
    public struct IndexToValue<T> where T : struct
    {
        private int _index;

        public IndexToValue(int index) => _index = index;
        
        public static IndexToValue<T> Empty() => new IndexToValue<T>(-1);
        
        
        public bool HasValue() => _index >= 0;
        
        public T GetElement(NativeList<T> list) => list[_index];

        public void SetElement(NativeList<T> list, T value) => list[_index] = value;
        
        public void AddElement(NativeList<T> list, T value)
        {
            list.Add(value);
            _index = list.Length - 1;
        }
        
        public void SetElementOrAddIfEmpty(NativeList<T> list, T value)
        {
            if (HasValue())
                SetElement(list, value);
            else
                AddElement(list, value);
        }

    }
}