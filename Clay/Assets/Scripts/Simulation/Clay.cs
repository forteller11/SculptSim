using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

namespace ClaySimulation
{
    [RequireComponent(typeof(Rigidbody))]
    public class Clay : MonoBehaviour
    {
        private static readonly int PARTICLES_LENGTH_UNIFORM = Shader.PropertyToID("_ParticlesLength");
        private static readonly int PARTICLES_UNIFORM = Shader.PropertyToID("_Particles");
        
        [Required] public Rigidbody RigidBody;
        [Required] public Material Material;

        public Vector4 [] ParticlePositions;
        public int ParticleLength;

        public void SetMaterial()
        {
            Material.SetVectorArray(PARTICLES_UNIFORM, ParticlePositions);
            Material.SetInt(PARTICLES_LENGTH_UNIFORM, ParticleLength);
        }
        
        

    }
}
