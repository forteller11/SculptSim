using System;
using ClaySimulation.Utils.ExtensionMethods;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

namespace Fort.EulerSim
{
    public class Input : MonoBehaviour
    {
        [SerializeField] [Required] Simulation _simulation;
        [SerializeField] [Required] Camera _camera;
        
        private Random _random = Random.CreateFromIndex(0);

        private void Update()
        {
            if (!Keyboard.current.spaceKey.isPressed) 
                return;
            
            var mousePos = Mouse.current.position.ReadValue();
            var worldPoint = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _camera.nearClipPlane));

            var bounds = _simulation.Grid.GetWorldBounds();
            if (!bounds.Contains(worldPoint))
                return;
                
            Debug.Log($"Particle Position: {worldPoint}");
            var particle = new Particle()
            {  
                Position = new float2(worldPoint.x, worldPoint.y),
                Color = _random.NextColor(), 
                Mass = 1,
                Velocity = _random.NextFloat2Direction()
            };

            _simulation.AddParticle(particle);
        }
        
    }

}