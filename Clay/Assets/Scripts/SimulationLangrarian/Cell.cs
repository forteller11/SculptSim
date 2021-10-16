using Unity.Mathematics;

namespace Fort.EulerSim
{
    public struct Cell
    {
        public float Density;
        public float3 CenterOfDensity; //normalized

        public void Reset()
        {
            Density = 0;
            CenterOfDensity = float3.zero;
        }
    }
}