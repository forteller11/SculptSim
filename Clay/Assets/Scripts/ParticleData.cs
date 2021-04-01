using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ParticleData : IComponentData
{
    public float Radius;
    public Vector3 ToMove;
}
