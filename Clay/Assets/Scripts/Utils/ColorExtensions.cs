﻿using UnityEngine;

namespace ClaySimulation.Utils
{
    public static class ColorExtensions
    {
        public static Color WithAlpha(this Color c, float alpha)
        {
            c.a = alpha;
            return c;
        }
    }
}