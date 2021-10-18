using System;
using System.Collections.Generic;
using ClaySimulation.Utils;
using Shapes;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Fort.EulerSim
{
    public class Simulation : ImmediateModeShapeDrawer
    {
        public Grid Grid;
        [SerializeField] private float _viscocity = 0.98f;
        [SerializeField] private float _minDistFromCenterOfDensity = 0.001f;

        [SerializeField] private float2 _minMaxValidDensity = new float2(0.4f, .6f);

        [NonSerialized] [ShowInInspector] public List<Particle> Particles = new List<Particle>(80);

        public void AddParticle(Particle particle)
        {
            Particles.Add(particle);
        }

        private void Start()
        {
            Grid.Init();
        }

        public void Update()
        {
            Grid.Reset();
            
            #region add particles to grid
            for (int i = 0; i < Particles.Count; i++)
            {
                Grid.AddParticleToSim(Particles[i]);
            }
            #endregion

            #region simulate particles from grid
            for (int i = 0; i < Particles.Count; i++)
            {
                var particle = Particles[i];
                float2 localPos = Grid.WorldPositionToLocalPosition(particle.Position);
                int2 cellIndex = Grid.CellIndexFromWorldPosition(particle.Position);
                Cell cell = Grid._cells[cellIndex.x, cellIndex.y];

                float2 awayFromDensity = localPos - cell.CenterOfDensity;
                particle.Velocity += awayFromDensity * 1/particle.Mass;
                
                
                // float distFromCenterOfDenssity = math.distance(float2.zero, awayFromDensity);
                //
                // if (distFromCenterOfDesnsity > _minDistFromCenterOfDensity)
                // {
                //     
                //     //change velocity based on inertia and center of desnity
                // }

                particle.Position += particle.Velocity;
                particle.Velocity *= _viscocity;
            }
            #endregion
        }
    }
}