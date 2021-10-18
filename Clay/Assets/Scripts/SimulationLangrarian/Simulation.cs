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
        [SerializeField] private float _minDistFromCenterOfDensity = 0.001f;

        [SerializeField] private float2 _minMaxValidDensity = new float2(0.4f, .6f);
        
        [SerializeField] private float _accelerationAwayFromPressureMultiplier = 0.01f;
        [SerializeField] private float _drag = 0.98f;
        
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
                
                #region wrap position within grid
                var gridBounds = Grid.GetWorldBounds();
                particle.Position = math.clamp(particle.Position, gridBounds.min, gridBounds.max);
                #endregion
                
                int2 cellIndex = Grid.CellIndexFromWorldPosition(particle.Position);
                try
                {
                    Cell cell = Grid.Cells[cellIndex.x, cellIndex.y];
                    float awayFromPressureMultiplier = cell.Density * _accelerationAwayFromPressureMultiplier;
                    float2 awayFromDensity = (particle.Position - cell.CenterOfDensity) * awayFromPressureMultiplier;
                    particle.Velocity += (awayFromDensity * 1/particle.Mass);
                
                
                    // float distFromCenterOfDenssity = math.distance(float2.zero, awayFromDensity);
                    //
                    // if (distFromCenterOfDesnsity > _minDistFromCenterOfDensity)
                    // {
                    //     
                    //     //change velocity based on inertia and center of desnity
                    // }

                    particle.Position += particle.Velocity;
                    particle.Velocity *= _drag;

                    Particles[i] = particle;
                }
                catch (Exception e)
                {
                    Debug.Log("s");
                }

            }
            #endregion
        }
    }
}