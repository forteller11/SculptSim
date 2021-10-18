using Unity.Mathematics;

namespace ClaySimulation.Utils
{
    public static class MathUtils
    {
        public static float MaxComponent(float2 f) => (f.x > f.y) ? f.x : f.y;
        
        public static float MinComponent(float2 f) => (f.x < f.y) ? f.x : f.y;
        
    }
}