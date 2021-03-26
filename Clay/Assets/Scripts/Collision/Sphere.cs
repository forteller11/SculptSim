using System;
using UnityEngine;

namespace Collision
{
    [Serializable]
    public struct Sphere
    {
        public Vector3 Position;
        public float Radius;

        public Sphere(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }
}