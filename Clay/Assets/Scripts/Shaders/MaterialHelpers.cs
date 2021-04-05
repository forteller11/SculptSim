using SpatialPartitioning;
using Unity.Collections;
using UnityEngine;

namespace ClaySimulation.Shaders
{
    public class ClayMaterialSender
    {
        private Vector4 [] _positionsBuffer;
        private Matrix4x4 [] _octNodesBuffer;
        private Vector4 [] _octValuesBuffer;
        private Material _material;
        
        private static readonly int PARTICLES_LENGTH_UNIFORM = Shader.PropertyToID("_ParticlesLength");
        private static readonly int PARTICLES_UNIFORM = Shader.PropertyToID("_Particles");
        private static readonly int OCTNODES_UNIFORM = Shader.PropertyToID("_OctNodes");
        private static readonly int OCTVALUES_UNIFORM = Shader.PropertyToID("_OctValues");

        public ClayMaterialSender(Material material, int particlesCount, Octree octree)
        {
            _material = material;
            _positionsBuffer = new Vector4[particlesCount];
            _octNodesBuffer  = new Matrix4x4[octree.Nodes.Length];
            _octValuesBuffer = new Vector4[octree.Values.Length];
        }
        public  void SendClayMaterialData(Material mat, NativeArray<Vector4> positions, Octree octree)
        {
            //maybe have vector3 > 4 to save in jobs, and will be worth extra cost not being able to memcpy?
            positions.CopyTo(_positionsBuffer);

            CopyOctreeToBuffers(octree);
        }

        void CopyOctreeToBuffers(Octree octree)
        {
            
        }
    }
}