using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using MathUtils = ClaySimulation.Utils.MathUtils;

namespace Fort.EulerSim
{
    [Serializable]
    public class Grid
    {
        [SerializeField] public float2 _origin = float2.zero;
        [SerializeField] public int2 _dimensions = new int2(8, 8);
        [SerializeField] public float2 _cellSize = new float2(1, 1);

        public Cell[,] Cells;
        
        public void Init()
        {
            Cells = new Cell[_dimensions.x, _dimensions.y];
            
            Reset();
        }

        public void Reset()
        {
            int xLength = Cells.GetLength(0);
            int yLength = Cells.GetLength(1);
            for (int i = 0; i < xLength; i++)
            {
                for (int j = 0; j < yLength; j++)
                {
                    var cell = Cells[i, j];
                    
                    cell.Reset();
                    Cells[i, j] = cell;
                }
            }
        }

        public void AddParticleToSim(in Particle particle)
        {
            int2 cellIndex = CellIndexFromWorldPosition(particle.Position);
            if (cellIndex.x >= _dimensions.x || 
                cellIndex.y >= _dimensions.y ||
                cellIndex.x < 0 ||
                cellIndex.y < 0)
            {
                return;
            }
            Cells[cellIndex.x, cellIndex.y].AddParticle(particle);
        }
        
        public int2 CellIndexFromWorldPosition(float2 worldPosition)
        {
            float2 localGridPos = worldPosition - _origin;
            int2 index = (int2) (localGridPos / _cellSize);
            return index;
        }
        
        public int2 CellIndexFromWorldClamped(float2 worldPosition)
        {
            int2 index = CellIndexFromWorldPosition(worldPosition);
            int2 indexClamped = math.clamp(index, int2.zero, _dimensions - new int2(1,1));
            return indexClamped;
        }

        public float2 WorldPositionFromIndex(int2 index)
        {
            return (index * _cellSize) + _origin;
        }

        public float2 WorldToLocalCellPosition(float2 worldPosition)
        {
            float2 localCell = worldPosition % _cellSize;
            return localCell;
        }

        public Rect GetWorldBounds()
        {
            return new Rect(_origin, (Vector2)(_cellSize * _dimensions));
        }
        
    }
}
