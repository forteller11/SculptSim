using System;
using System.Collections.Generic;
using ClaySimulation.Utils;
using Shapes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Fort.EulerSim
{
    public class Simulation : ImmediateModeShapeDrawer
    {
        [SerializeField] private Grid _grid;
        [SerializeField] private float _viscocity = 0.98f;
        [SerializeField] private float _minDistFromCenterOfDensity = 0.001f;

        [SerializeField] private float2 _minMaxValidDensity = new float2(0.4f, .6f);

        private List<Particle> _particles = new List<Particle>(80);

        public void AddParticle(Particle particle)
        {
            _particles.Add(particle);
        }

        private void Start()
        {
            _grid.Init();
        }

        public void Update()
        {
            _grid.Reset();
            
            #region add particles to grid
            for (int i = 0; i < _particles.Count; i++)
            {
                _grid.AddParticleToSim(_particles[i]);
            }
            #endregion

            #region simulate particles from grid
            for (int i = 0; i < _particles.Count; i++)
            {
                var particle = _particles[i];
                float2 localPos = _grid.WorldPositionToLocalPosition(particle.Position);
                int2 cellIndex = _grid.CellIndexFromWorldPosition(particle.Position);
                Cell cell = _grid._cells[cellIndex.x, cellIndex.y];

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

        public override void DrawShapes(Camera cam)
        {
            using (Draw.Command(cam))
            {
                Draw.Matrix = Matrix4x4.identity;
                Draw.BlendMode = ShapesBlendMode.Opaque;
                
                for (int i = 0; i < _grid._cells.GetLength(0); i++)
                {
                    for (int j = 0; j < _grid._cells.GetLength(1); j++)
                    {
                        var cell = _grid._cells[i, j];
                        var worldPos = _grid.IndexToWorldPosition(new int2(i, j));
                        var rect = new Rect(worldPos + _grid._origin, _grid._cellSize);
                        Debug.Log("Start");
                        Debug.Log(rect);
                        Draw.Thickness = 10;
                        Draw.Color = cell.Color.WithAlpha(1);
                        Draw.Rectangle(rect);
                    }
                }
                
                for (int i = 0; i < _particles.Count; i++)
                {
                    var part = _particles[i];
                
                    Draw.Color = part.Color;
                    Draw.Radius = part.Mass;
                    Draw.Sphere(new float3(part.Position, 0));
                }
            }
            
        }
    }
}