
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using SpatialPartitioning;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using AABB = Collision.AABB;
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

        private Octree _octree;
        
        private List<Clay> _particles;
        private NativeArray<Vector3> _particlePositions;
        private NativeArray<Vector3> _toMove;
        private NativeArray<Vector3> _closestSpheres; //will be of stride _maxParticlesToSimulate
        private NativeArray<int> _closestSpheresCount;
        #endregion

        private void Awake()
        {
            #region spawn
            _particles = new List<Clay>(_spawnOnStart);
            var random = Random.CreateFromIndex((uint)DateTime.Now.Millisecond);
            
            for (int i = 0; i < _spawnOnStart; i++)
            {
                var newParticle = Instantiate(_particlePrefab);
                    
                var randomOutput = (Vector3) random.NextFloat3() - new Vector3(0.5f, 0.5f, 0.5f);
                var startingPos = randomOutput * _radiusToSpawnIn;
                newParticle.transform.position = startingPos + transform.position;
                
                newParticle.ParticlePositions = new Vector4[_maxParticlesToSimulate];
                
                _particles.Add(newParticle);
            }
            #endregion

            _particlePositions = new NativeArray<Vector3>(_particles.Count, Allocator.Persistent);
            _toMove = new NativeArray<Vector3>(_particles.Count, Allocator.Persistent);
            _closestSpheres = new NativeArray<Vector3>(_particles.Count * _maxParticlesToSimulate, Allocator.Persistent);
            _closestSpheresCount = new NativeArray<int>(_particles.Count, Allocator.Persistent);
            
            _octree = new Octree(_octSettings, _particles.Count);
            
            for (int i = 0; i < _particles.Count; i++)
            {
                _particlePositions[i] = _particles[i].transform.position;
            }
            
        }
        
        private void OnDestroy()
        {
            _octree.Dispose();
            _particlePositions.Dispose();
            _closestSpheres.Dispose();
            _closestSpheresCount.Dispose();
            _toMove.Dispose();
        }

        private void Update()
        {
            ConstructOctree();
            CalculateParticleForces();
            MoveParticlesAndRefreshPositions();
            SetParticleMaterials();
        }

        void ConstructOctree()
        {
            var aabb = new AABB(transform.position, _radiusToSpawnIn * _octreeRadiusMultiplier);
            var octConstructJob = _octree.CreateConstructJob(_particlePositions, aabb);

            var job = octConstructJob.Schedule();
            job.Complete();
        }
        
        void CalculateParticleForces()
        {
            var job = new ParticleSimJob()
            {
                //in
                Octree = _octree,
                Positions = _particlePositions,
                ConstMult = _constantMultiplier,
                DeltaTime = Time.deltaTime,
                MinRadius = _minMaxRadius.x,
                MaxRadius = _minMaxRadius.y,
                DesiredPercentBetweenMinMax = _desiredPercentBetweenMinMax,
                MaxParticlesToSimulate = _maxParticlesToSimulate,
                //out
                
                ToMove = _toMove,
                ClosestSpheres = _closestSpheres,
                ClosestSpheresCount = _closestSpheresCount
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

        void SetParticleMaterials()
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                p.ParticleLength = _closestSpheresCount[i];

                for (int j = 0; j < _maxParticlesToSimulate; j++)
                {
                    int index = j + (i * _maxParticlesToSimulate);
                    p.ParticlePositions[j] = _closestSpheres[index];
                }
                
                p.SetMaterial();
            }
        }
        
        
        
    }
}