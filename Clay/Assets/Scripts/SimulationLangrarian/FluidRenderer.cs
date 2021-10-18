using ClaySimulation.Utils;
using Shapes;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Fort.EulerSim
{
    public class FluidRenderer : ImmediateModeShapeDrawer
    {
        [Required] [SerializeField] Simulation _simulation;
        [TabGroup("Grid")] public float GridLineThickness = 0.1f;
        [TabGroup("Grid")] public float GridLineSpaceThickness = 0.01f;

        [TabGroup("Particles")] public float ParticleRadius = 0.1f;
        public override void DrawShapes(Camera cam)
        {
            using (Draw.Command(cam))
            {
                Draw.Matrix = Matrix4x4.identity;
                Draw.BlendMode = ShapesBlendMode.Opaque;

                var grid = _simulation.Grid;
                for (int i = 0; i < grid._cells.GetLength(0); i++)
                {
                    for (int j = 0; j < grid._cells.GetLength(1); j++)
                    {
                        var cell = grid._cells[i, j];
                        var worldPos = grid.IndexToWorldPosition(new int2(i, j));
                        var rect = new Rect(worldPos + grid._origin, grid._cellSize - GridLineSpaceThickness);
                        
                        Draw.Thickness = 10;
                        Draw.Color = cell.Color.WithAlpha(1);
                        Draw.RectangleBorder(rect, GridLineThickness);
                    }
                }
                
                var particles = _simulation.Particles;
                for (int i = 0; i < particles.Count; i++)
                {
                    var part = particles[i];
                
                    Draw.Color = part.Color;
                    Draw.Radius = ParticleRadius;
                    Draw.Sphere(new float3(part.Position, 0));
                }
            }
            
        }
    }
}