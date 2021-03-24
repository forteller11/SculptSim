﻿using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public class OctValue
    {
        public Vector3 Position;
        public OctValue NextValue;

        public static OctValue CreateTail(Vector3 value)
        {
            var octValue = new OctValue();
            octValue.Position = value;
            octValue.NextValue = null;
            return octValue;
        }
        
        public static OctValue WithChild(Vector3 value, OctValue child)
        {
            var octValue = new OctValue();
            octValue.Position = value;
            octValue.NextValue = child;
            return octValue;
        }
        
        public OctValue GetLastElement()
        {
            if (NextValue == null)
            {
                return this;
            }
            else
            {
                return NextValue.GetLastElement();
            }
        }
        

    }
}