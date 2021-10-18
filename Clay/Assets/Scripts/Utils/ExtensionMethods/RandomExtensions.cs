using Unity.Mathematics;
using UnityEngine;

namespace ClaySimulation.Utils.ExtensionMethods
{
    public static class RandomExtensions
    {
        public static Color NextColor(this ref Unity.Mathematics.Random random)
        {
            var value = random.NextFloat3(new float3(0), new float3(1));
            return new Color(value.x, value.y, value.z);
        }
    }
}