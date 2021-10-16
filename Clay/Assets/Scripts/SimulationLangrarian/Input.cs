using System;
using ClaySimulation.Utils.ExtensionMethods;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

namespace Fort.EulerSim
{
    public class Input : MonoBehaviour
    {
        [SerializeField] [Required] Simulation _simulation;
        
        private Random _random = Random.CreateFromIndex(0);

        private void Start()
        {
            throw new NotImplementedException();
        }

        private void Update()
        {
            if (Mouse.current.leftButton.isPressed)
            {
                var mousePos = Mouse.current.position.ReadValue();
                Camera.current.ScreenToWorldPoint(mousePos);

                var particle = new Particle()
                {
                    Position = mousePos,
                    Color = _random.NextColor(),
                    Mass = 1,
                    Velocity = _random.NextFloat2Direction()
                };
                
                _simulation.AddParticle(particle);
            }
        }
        
    }

}