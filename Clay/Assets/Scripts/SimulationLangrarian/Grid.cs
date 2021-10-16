using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using Unity.Mathematics;
using UnityEngine;

namespace Fort.LangrarianSim
{
    public class Grid : ImmediateModeShapeDrawer
    {
        [SerializeField] private int3 _dimensions = new int3(8, 8, 8);
        [SerializeField] private float3 _cellSize = new float3(1, 1, 1);
        private Cell[,,] _cells;


        private void Start()
        {
            _cells = new Cell[_dimensions.x, _dimensions.y, _dimensions.z];
            
            //todo spawn parts
            
            //todo move parts based on pressure
        }

        public struct Cell
        {
            public float Density;
            public float3 CenterOfDensity; //normalized
        }
        
        //todo compute shader Langrarian sim
    }
}
