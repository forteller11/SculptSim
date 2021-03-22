using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DefaultNamespace
{
    public static class Common
    {
        public static Color RandomColor(Random random, float alpha=1)
        {
            return new Color(random.NextFloat(), random.NextFloat(), random.NextFloat(), alpha);
        }
    }
}