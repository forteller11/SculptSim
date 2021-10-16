using System;
using Unity.Mathematics;
using UnityEngine;

namespace Fort.EulerSim
{
    [Serializable]
    public class Grid
    {
        [SerializeField] private float3 _origin = float3.zero;
        [SerializeField] private int3 _dimensions = new int3(8, 8, 8);
        [SerializeField] private float3 _cellSize = new float3(1, 1, 1);
        
        private Cell[,] _cells;
        
        public void Init()
        {
            _cells = new Cell[_dimensions.x, _dimensions.y];
            
            //todo spawn parts
            
            //todo move parts based on pressure
        }

        public int2 CellIndexFromWorldPosition(float2 worldPosition)
        {
            int xIndex = (int) (worldPosition.x % _cellSize.x);
            int yIndex = (int) (worldPosition.y % _cellSize.y);
            return new int2(xIndex, yIndex);
        }

        public void Reset()
        {
            
        }

        
        //todo compute shader Langrarian sim
    }
}
