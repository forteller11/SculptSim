﻿
using System;
using System.Collections.Generic;
using Collision;

using Sirenix.OdinInspector;
using UnityEngine;
using SpatialPartitioning;
using Unity.Collections;
using Unity.Jobs;
using Random = Unity.Mathematics.Random;


namespace ClaySimulation
{
    public class ClaySimManager : MonoBehaviour
    {
        #region members
        [FoldoutGroup("Spawn")] [SerializeField] [AssetsOnly] private Clay _particlePrefab;
        [FoldoutGroup("Spawn")] [SerializeField] private int _spawnOnStart = 10;
        [FoldoutGroup("Spawn")] [SerializeField] private float _radiusToSpawnIn = 5;
        
        [FoldoutGroup("Octree")] [SerializeField] private OctSettings _octSettings;
        [FoldoutGroup("Octree")] [SerializeField] private float _octreeRadiusMultiplier;
        [FoldoutGroup("Octree")] [SerializeField] int _maxParticlesToSimulate = 5;

        [FoldoutGroup("Sim Settings")] [SerializeField] [MinMaxSlider(0,4, ShowFields = true)] Vector2 _minMaxRadius = new Vector2(0.2f,0.3f);
        [FoldoutGroup("Sim Settings")] [SerializeField] float _desiredPercentBetweenMinMax = .5f;
        [FoldoutGroup("Sim Settings")] [SerializeField] [Range(0,1)] private float  _constantMultiplier = .05f;
        [Tooltip("x== 0 means at desired percent, -1 == at min, 1 == at max")] 
        [FoldoutGroup("Sim Settings")] [SerializeField] AnimationCurve _forceMultiplierCurve = new AnimationCurve(new Keyframe(-1, 1), new Keyframe(0, 0), new Keyframe(1, 1));
        
        [FoldoutGroup("Debug")] public bool DrawParticles;
        [FoldoutGroup("Debug")] public bool DrawOctree;
        
        private static readonly int PARTICLES_LENGTH_UNIFORM = Shader.PropertyToID("_ParticlesLength");
        private static readonly int PARTICLES_UNIFORM = Shader.PropertyToID("_Particles");
        
  
        private Material _material;
        private Octree _octree;
        
        private List<Clay> _particles;
        private NativeArray<Vector3> _particlePositions;
        private NativeArray<Vector3> _queryBuffer;
        private NativeArray<Vector3> _toMove;

        #endregion

        private void Awake()
        {
            _material = GetComponent<MeshRenderer>().material;
            
            #region spawn
            _particles = new List<Clay>(_spawnOnStart);
            var random = Random.CreateFromIndex((uint)System.DateTime.Now.Millisecond);
            for (int i = 0; i < _spawnOnStart; i++)
            {
                var newParticle = Instantiate(_particlePrefab);
                var randomOutput = (Vector3) random.NextFloat3() - new Vector3(0.5f, 0.5f, 0.5f);
                var startingPos = randomOutput * _radiusToSpawnIn;
                newParticle.transform.position = startingPos + transform.position;
                
                _particles.Add(newParticle);
            }
            #endregion

            _particlePositions = new NativeArray<Vector3>(_particles.Count, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            _toMove = new NativeArray<Vector3>(_particles.Count, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            _queryBuffer = new NativeArray<Vector3>(_spawnOnStart, Allocator.Persistent);
            _octree = new Octree(_octSettings, _spawnOnStart);
            
            for (int i = 0; i < _particles.Count; i++)
            {
                _particlePositions[i] = _particles[i].transform.position;
            }
            
        }
        
        private void OnDestroy()
        {
            _octree.Dispose();
            _queryBuffer.Dispose();
            _particlePositions.Dispose();
            _toMove.Dispose();
        }

        private void Update()
        {
            ConstructOctree();
            CalculateParticleForces();
            MoveParticlesAndRefreshPositions();
        }

        void ConstructOctree()
        {
            var aabb = new AABB(transform.position, _radiusToSpawnIn * _octreeRadiusMultiplier);
            var octConstructJob = _octree.CreateConstructJob(_particlePositions, aabb);

            var job = octConstructJob.Schedule();
            
            job.Complete();
            
            // _octree.CleanAndPrepareForInsertion(new AABB(transform.position, _radiusToSpawnIn * _octreeRadiusMultiplier));
            //
            // for (int i = 0; i < _particles.Count; i++)
            // {
            //     var p3 =  _particles[i].transform.position;
            //     _octree.Insert(p3);
            // }
  
        }
        
        void CalculateParticleForces()
        {
            var job = new ParticleSimJob()
            {
                Octree = _octree,
                Positions = _particlePositions,
                ConstMult = _constantMultiplier,
                DeltaTime = Time.deltaTime,
                MinRadius = _minMaxRadius.x,
                MaxRadius = _minMaxRadius.y,
                DesiredPercentBetweenMinMax = _desiredPercentBetweenMinMax,
                MaxParticlesToSimulate = _maxParticlesToSimulate,
                ToMove = _toMove,
                Query = _queryBuffer
            };

            
            var jobHandle = job.Schedule(_particles.Count, 1); //todo increase batch count and measure
            
            jobHandle.Complete();
     
        }

        void MoveParticlesAndRefreshPositions()
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                var pos = _particles[i].transform.position;
                var newPos = pos + _toMove[i];
                var rb = _particles[i].RigidBody;
                
                rb.MovePosition(newPos);
                _particlePositions[i] = newPos;
                _toMove[i] = new Vector3(0,0,0);
            }
        }
        
    }
}