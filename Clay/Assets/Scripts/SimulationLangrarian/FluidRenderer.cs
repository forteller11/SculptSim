using System.Runtime.InteropServices;
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
        [TabGroup("Grid")] public float2 MinMaxDensity = new float2(0, 10);
        [TabGroup("Grid")] public Color GridBorderColor = new Color(0, 0, 0, 1);

        [TabGroup("Particles")] public float ParticleRadius = 0.1f;

        public bool ShouldRenderParticles = true;
        public bool ShouldRenderGrid = true;
        public override void DrawShapes(Camera cam)
        {
            using (Draw.Command(cam))
            {
                Draw.Matrix = Matrix4x4.identity;
                Draw.BlendMode = ShapesBlendMode.Opaque;

                RenderParticles();
                
                RenderGrid();
            }
            
        }

        private void RenderParticles()
        {
            if (!ShouldRenderParticles)
                return;
            
            var particles = _simulation.Particles;
            for (int i = 0; i < particles.Count; i++)
            {
                var part = particles[i];

                Draw.Color = part.Color;
                Draw.Radius = ParticleRadius;
                Draw.Sphere(new float3(part.Position, 0));
            }
        }

        private void RenderGrid()
        {
            if (!ShouldRenderGrid)
                return;
            
            var grid = _simulation.Grid;
            for (int i = 0; i < grid.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < grid.Cells.GetLength(1); j++)
                {
                    var cell = grid.Cells[i, j];
                    var worldPos = grid.WorldPositionFromIndex(new int2(i, j));
                    var rect = new Rect(worldPos + grid._origin, grid._cellSize - GridLineSpaceThickness);

                    float normBetweenMinMaxDensity = Mathf.InverseLerp(MinMaxDensity.x, MinMaxDensity.y, cell.Density);
                    Draw.Color = cell.Color.WithAlpha(normBetweenMinMaxDensity);
                    Draw.Rectangle(rect, GridLineThickness);

                    Draw.Thickness = 10;
                    Draw.Color = GridBorderColor;
                    Draw.RectangleBorder(rect, GridLineThickness);
                }
            }
        }
    }
}