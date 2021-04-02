using Collision;
using SpatialPartitioning;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ClaySimulation
{

    [BurstCompile]
    public struct InsertOctreeJob : IJob
    {
        //input
        [ReadOnly] public NativeArray<Vector3> Positions;
        public AABB AABB;
        
        //output
        public NativeArray<Octree> Octree;
        
        public void Execute()
        {
            Octree[0].CleanAndPrepareForInsertion(AABB);
            
            for (int i = 0; i < Positions.Length; i++)
            {
                var p3 =  Positions[i];
                Octree[0].Insert(p3);
            }
        }
    }
}