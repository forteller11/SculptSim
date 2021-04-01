using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

[SerializeField]
public struct ClaySimSettings 
{
    [Sirenix.OdinInspector.ReadOnly] public NativeArray<Vector3> Particles;
    [Range(0, 1)] public float DesiredPercentBetweenMinMax;
    [MinMaxSlider(0, 3)] public Vector2 MinMaxRadius;
    public int MaxParticlesToSimulate;
    [Range(0, 1)] public float ConstMult;
}
