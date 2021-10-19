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

        [SerializeField] private float _boundaryPressure = 1;
        
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
            
            #region boundary pressure
            var cells = Grid.Cells;
            for (int i = 0; i < cells.GetLength(0); i++)
            {
                
                cells[i, 0].Density = cells[i, 0].Density + _boundaryPressure;
                cells[i, cells.GetLength(1) - 1].Density = cells[i, 0].Density + _boundaryPressure;
            }
            
            for (int i = 0; i < cells.GetLength(1); i++)
            {
                cells[0, i].Density = cells[i, 0].Density + _boundaryPressure;
                cells[cells.GetLength(1) - 1, 0].Density = cells[0, i].Density + _boundaryPressure;
            }
            #endregion

            #region simulate particles from grid
            for (int i = 0; i < Particles.Count; i++)
            {
                var particle = Particles[i];
                
                #region wrap position within grid
                var gridBounds = Grid.GetWorldBounds();
                
                var p = particle.Position;

                while (p.x > gridBounds.xMax)
                    p.x -= gridBounds.width;

                while (p.x < gridBounds.xMin)
                    p.x += gridBounds.width;
                
                while (p.y > gridBounds.yMax)
                    p.y -= gridBounds.height;

                while (p.y < gridBounds.yMin)
                    p.y += gridBounds.height;

                p = math.clamp(p, gridBounds.min, gridBounds.max);
                particle.Position = p;
                #endregion
                
                int2 cellIndex = Grid.CellIndexFromWorldPosition(particle.Position);
                try
                {
                    Cell cell = Grid.Cells[cellIndex.x, cellIndex.y];
                    float awayFromPressureMultiplier = cell.Density * _accelerationAwayFromPressureMultiplier;
                    float2 awayFromDensity = (particle.Position - cell.CenterOfDensity) * awayFromPressureMultiplier;
                    particle.Velocity += (awayFromDensity * 1/particle.Mass);

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