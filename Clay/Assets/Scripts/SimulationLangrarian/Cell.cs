using Unity.Mathematics;
using UnityEngine;

namespace Fort.EulerSim
{
    public struct Cell
    {
        public float Density;
        public float2 CenterOfDensity; //normalized
        public int ParticlesInCell;
        public Color Color;

        public void Reset()
        {
            Density = 0;
            CenterOfDensity = float2.zero;
            ParticlesInCell = 0;
        }

        public void AddParticle(Particle particle)
        {
            ParticlesInCell++;

            float weight = (float) 1 / ParticlesInCell;
            
            Color = Color.LerpUnclamped(Color, particle.Color, weight);
            Density = math.lerp(Density, particle.Mass, weight);
            CenterOfDensity = math.lerp(CenterOfDensity, particle.Position, weight);
        }

    }
}