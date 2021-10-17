using UnityEngine;

namespace ClaySimulation.Utils
{
    public static class ColorExtensions
    {
        public static Color WithAlpha(this ref Color c, float alpha)
        {
            c.a = alpha;
            return c;
        }
    }
}