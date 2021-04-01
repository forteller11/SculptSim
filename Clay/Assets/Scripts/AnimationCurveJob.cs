using Unity.Collections;
using UnityEngine;

namespace DefaultNamespace
{
    public struct CurveNormalized
    {
        public readonly NativeArray<float> Samples;

        public CurveNormalized(AnimationCurve curve, int resolution=100)
        {
            if (curve.keys[0].time < 0 || curve.keys[curve.keys.Length-1].time > 1)
                Debug.LogError("Input Curve Keyframes times must be between 0-1");
            
            Samples = new NativeArray<float>(resolution, Allocator.Persistent);

            for (int i = 0; i < Samples.Length; i++)
            {
                float index = (float) i / Samples.Length;
                Samples[i] = curve.Evaluate(index);
            }
        }

        public float EvaluateDiscrete(float time)
        {
            int index = (int) (time * Samples.Length);
            return Samples[index];
        }

        // public float EvaluateSmoothLinear()
        // {
        //     
        // }
    }
}