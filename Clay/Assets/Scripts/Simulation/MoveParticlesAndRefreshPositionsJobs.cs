// using System.Numerics;
// using Unity.Collections;
// using UnityEngine.Jobs;
//
// namespace ClaySimulation
// {
//     public struct MoveParticlesAndRefreshPositionsJobs : IJobParallelForTransform
//     {
//         public NativeArray<Vector3> ToMove;
//         public NativeArray<Vector3> Positions;
//         public void Execute(int index, TransformAccess transform)
//         {
//             
//             for (int i = 0; i < ToMove.Length; i++)
//             {
//                 var pos = Positions[i];
//                 var newPos = pos + ToMove[i];
//
//                 Positions[i] = newPos;
//                 transform.position = 
//                 rb.MovePosition(newPos);
//                 _particlePositions[i] = newPos;
//                 ToMove[i] = new Vector3(0,0,0);
//             }
//         }
//     }
// }