// using System;
// using Unity.Collections;
// using UnityEngine;
//
// namespace DefaultNamespace
// {
//     public readonly struct CurveNormalized : IDisposable
//     {
//         public readonly NativeArray<float> Samples;
//         public readonly float MinTime;
//         public readonly float MaxTime;
//
//         public CurveNormalized(AnimationCurve curve, int resolution=100, float minTime=0, float maxTime=1)
//         {
//             MinTime = minTime;
//             MaxTime = maxTime;
//             
//             
//             
//             Samples = new NativeArray<float>(resolution, Allocator.Persistent);
//
//             for (int i = 0; i < Samples.Length; i++)
//             {
//                 float index = (float) i / Samples.Length;
//                 Samples[i] = curve.Evaluate(index);
//             }
//         }
//
//         public float EvaluateDiscrete(float time)
//         {
//             float range = MaxTime - MinTime;
//             Mathf.InverseLerp(MinTime, MaxTime, time);
//
//             var index = time
//             int index = (int) (time * Samples.Length);
//             return Samples[index];
//         }
//
//         // public float EvaluateSmoothLinear()
//         // {
//         //     
//         // }
//         public void Dispose()
//         {
//             Samples.Dispose();
//         }
//     }
// }