using System;
using Unity.Mathematics;
using UnityEngine;

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
            _cells[cellIndex.x, cellIndex.y].AddParticle(particle);
            //todo does this work
        }
        
        public int2 CellIndexFromWorldPosition(float2 worldPosition)
        {
            int xIndex = (int) (worldPosition.x % _cellSize.x);
            int yIndex = (int) (worldPosition.y % _cellSize.y);
            return new int2(xIndex, yIndex);
        }

        public float2 IndexToWorldPosition(int2 cellIndex)
        {
            float2 result = (cellIndex * _cellSize) + _origin;
            return result;
        }
        
        public float2 WorldPositionToLocalPosition(float2 worldPosition)
        {
            int2 cellIndex = CellIndexFromWorldPosition(worldPosition);
            float2 worldPositionCell = IndexToWorldPosition(cellIndex);
            float2 localCellPosition = worldPositionCell - worldPosition;

            return localCellPosition;
        }
        
    }
}
