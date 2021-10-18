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

        public Cell[,] _cells;
        
        public void Init()
        {
            _cells = new Cell[_dimensions.x, _dimensions.y];
            
            Reset();
        }

        public void Reset()
        {
            int xLength = _cells.GetLength(0);
            int yLength = _cells.GetLength(1);
            for (int i = 0; i < xLength; i++)
            {
                for (int j = 0; j < yLength; j++)
                {
                    var cell = _cells[i, j];
                    
                    cell.Reset();
                    _cells[i, j] = cell;
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
            _cells[cellIndex.x, cellIndex.y].AddParticle(particle);
        }
        
        public int2 CellIndexFromWorldPosition(float2 worldPosition)
        {
            float2 localGridPos = worldPosition - _origin;
            int2 index = (int2) (localGridPos / _cellSize);
            return index;
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
