using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public struct OctValue
    {
        public Vector3 Position;
        public IndexToValue<OctValue> NextValue;

        public static OctValue CreateTail(Vector3 value)
        {
            var octValue = new OctValue();
            octValue.Position = value;
            octValue.NextValue = IndexToValue<OctValue>.Empty();
            return octValue;
        }

    }
}