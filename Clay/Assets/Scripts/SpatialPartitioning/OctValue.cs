using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public struct OctValue
    {
        public Vector3 Position;
        public IndexToOctValue PreviousValue;

        public static OctValue CreateTail(Vector3 value)
        {
            var octValue = new OctValue();
            octValue.Position = value;
            octValue.PreviousValue = IndexToOctValue.Empty();
            return octValue;
        }

    }
}