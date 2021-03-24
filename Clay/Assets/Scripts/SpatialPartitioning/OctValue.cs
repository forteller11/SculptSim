using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public class OctValue
    {
        public Vector3 Position;
        public OctValue NextValue;
        public string Name;

        public static OctValue CreateTail(string name, Vector3 value)
        {
            var octValue = new OctValue();
            octValue.Name = name;
            octValue.Position = value;
            octValue.NextValue = null;
            return octValue;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}