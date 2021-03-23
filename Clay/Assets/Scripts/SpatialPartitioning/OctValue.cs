using System.Collections.Generic;
using UnityEngine;


namespace SpatialPartitioning
{
    public struct OctValue
    {
        public Vector3 Position;
        public int NextElementIndex;

        public static OctValue CreateTail(Vector3 value)
        {
            var octValue = new OctValue();
            octValue.Position = value;
            octValue.NextElementIndex = -1;
            return octValue;
        }
        
        public static OctValue WithChild(Vector3 value, int childIndex)
        {
            var octValue = new OctValue();
            octValue.Position = value;
            octValue.NextElementIndex = childIndex;
            return octValue;
        }
        
        public int GetLastElementIndex(int currentIndex, List<OctValue> values)
        {
            if (NextElementIndex > -1)
                return values[NextElementIndex].GetLastElementIndex(NextElementIndex, values);
            return currentIndex;
        }

        bool GetNext(List<OctValue> values, out OctValue next)
        {
            if (NextElementIndex > -1)
            {
                next = values[NextElementIndex];
                return true;
            }
            else
            {
                next = new OctValue();
                return false;
            }
        }

    }
}