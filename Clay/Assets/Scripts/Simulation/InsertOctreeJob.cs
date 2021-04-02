// using Collision;
// using SpatialPartitioning;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Jobs;
// using UnityEngine;
//
// namespace ClaySimulation
// {
//
//     [BurstCompile]
//     public struct InsertOctreeJob : IJob
//     {
//         //input
//         [ReadOnly] public NativeArray<Vector3> Positions;
//         public AABB AABB;
//         
//         //output
//         public Octree Octree;
//         
//         public void Execute()
//         {
//             Octree.CleanAndPrepareForInsertion(AABB);
//             
//             for (int i = 0; i < Positions.Length; i++)
//             {
//                 var p3 =  Positions[i];
//                 Octree.Insert(p3);
//             }
//         }
//     }
// }