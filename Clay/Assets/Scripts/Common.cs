using System.Runtime.CompilerServices;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ClaySimulation
{
    public static class Common
    {
        public static Color RandomColor(ref Random random, float alpha = 1)
        {
            return new Color(random.NextFloat(), random.NextFloat(), random.NextFloat(), alpha);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutQuart(float x) => 1 - ((1 - x) * (1 - x) * (1 - x) * (1 - x));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInQuart(float x) => 1 - (x * x * x * x);
    }
}
