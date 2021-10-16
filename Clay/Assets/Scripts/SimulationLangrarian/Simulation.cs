using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fort.EulerSim
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private Grid _grid;

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
            //todo reset grid
            
            //todo add parts to grid
            
            //todo sim parts
            
            //todo render
        }
    }
}