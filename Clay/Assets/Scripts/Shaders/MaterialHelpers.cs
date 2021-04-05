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
        public void SendClayMaterialData(NativeArray<Vector4> positions, Octree octree)
        {
            //maybe have vector3 > 4 to save in jobs, and will be worth extra cost not being able to memcpy?
            positions.CopyTo(_positionsBuffer);
            CopyOctreeToBuffers(octree);

            var mat = _material;
            mat.SetVectorArray(PARTICLES_UNIFORM, _positionsBuffer);
            mat.SetInt(PARTICLES_LENGTH_UNIFORM, _positionsBuffer.Length);
            mat.SetMatrixArray(OCTNODES_UNIFORM, _octNodesBuffer);
            mat.SetVectorArray(OCTVALUES_UNIFORM, _octValuesBuffer);
        }

        void CopyOctreeToBuffers(Octree octree)
        {
            //todo try to fit into vec4 > mat4x4
            for (int i = 0; i < octree.NodesCount.Value; i++)
            {
                //matrices are layed out row-wise (memory wise) in unity
                var n = octree.Nodes[i];
                var m = new Matrix4x4();

                //where first row of matrix is AABB:
                // --> Center.xyz == 00, 01, 02
                // --> HalfWidth == 03
                
                m[0, 0] = n.AABB.Center.x;
                m[0, 1] = n.AABB.Center.y;
                m[0, 2] = n.AABB.Center.z;
                m[0, 3] = n.AABB.HalfWidth;

                m[1, 0] = n.LastValue.Index;
                m[1, 1] = n.FirstChildIndex;

                _octNodesBuffer[i] = m;
            }
            
            for (int i = 0; i < octree.ValuesCount.Value; i++)
            {
                //matrices are layed out row-wise (memory wise) in unity
                var val = octree.Values[i];
                var vec = new Vector4();

                vec.x = val.Position.x;
                vec.y = val.Position.y;
                vec.z = val.Position.z;
                vec.w = val.PreviousValue.Index;
  
                _octValuesBuffer[i] = vec;
            }
        }
    }
}