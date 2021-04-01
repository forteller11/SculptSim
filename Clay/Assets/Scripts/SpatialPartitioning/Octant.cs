using System;

namespace SpatialPartitioning
{
    /* -------------
     enum representing an octant
    where {char} == plus in the corresponding axis, and '_' means minus in those axis
      > so XYZ is right, upper, forward octant
      > while ___ is left, down, backwards octant
      >  X__ is right, down, backwards octant
    --------------------------- */
    [Flags]
    public enum Octant
    {
        ___ = 0b_000,
        X__ = 0b_100,
        _Y_ = 0b_010,
        __Z = 0b_001,
        XY_ = X__ | _Y_,
        X_Z = X__ | __Z,
        _YZ = _Y_ | __Z,
        XYZ = X__ | _Y_ | __Z,
        
    }
    
}