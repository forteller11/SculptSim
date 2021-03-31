
using System;
using System.Collections.Generic;
using Collision;
using Sirenix.OdinInspector;
using UnityEngine;
using SpatialPartitioning;
using Unity.Collections;
using Random = Unity.Mathematics.Random;


namespace ClaySimulation
{
    public class ClayCollisionManager : MonoBehaviour
    {
        
        [FoldoutGroup("Spawn")] [SerializeField] [AssetsOnly] private Clay _particlePrefab;
        [FoldoutGroup("Spawn")] [SerializeField] private int _spawnOnStart = 10;
        [FoldoutGroup("Spawn")] [SerializeField] private float _radiusToSpawnIn = 5;
        
        [FoldoutGroup("Octree")] [SerializeField] private OctSettings _octSettings;
        [FoldoutGroup("Octree")] [SerializeField] private float _octreeRadiusMultiplier;

        [FoldoutGroup("Sim Settings")] [SerializeField] float _minRadius = 0;
        [FoldoutGroup("Sim Settings")] [SerializeField] float _maxRadius = 2;
        [FoldoutGroup("Sim Settings")] [SerializeField] float _desiredPercentBetweenMinMax = .5f;
        [FoldoutGroup("Sim Settings")] [SerializeField] [Range(0,1)] private float  _constantMultiplier = .05f;
        [Tooltip("x== 0 means at desired percent, -1 == at min, 1 == at max")] 
        [FoldoutGroup("Sim Settings")] [SerializeField] AnimationCurve _forceMultiplierCurve = new AnimationCurve(new Keyframe(-1, 1), new Keyframe(0, 0), new Keyframe(1, 1));

        [FoldoutGroup("Debug")] public bool DrawParticles;
        [FoldoutGroup("Debug")] public bool DrawOctree;
        
        [ShowInInspector] private List<Clay> _particles;
        [ShowInInspector] private List<Vector3> _particlesToMove;
        [ShowInInspector] private List<Vector4> _particlePositions;
        [ShowInInspector] private NativeArray<Vector3> _queryResults;
        [ShowInInspector] private Material _material;
        
        private static readonly int PARTICLES_LENGTH_UNIFORM = Shader.PropertyToID("_ParticlesLength");
        private static readonly int PARTICLES_UNIFORM = Shader.PropertyToID("_Particles");
        [SerializeField] private Octree Octree;

        private void Awake()
        {
            _material = GetComponent<MeshRenderer>().material;
            
            #region particles
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
            
            _particlesToMove = new List<Vector3>(_particles.Count);
            for (int i = 0; i < _particles.Count; i++)
                _particlesToMove.Add(Vector3.zero);
            
            _particlePositions = new List<Vector4>(_particles.Count);
            for (int i = 0; i < _particles.Count; i++)
                _particlePositions.Add(new Vector4(0,0,0, 0));
            #endregion
            
            #region octree
            Octree = new Octree(_octSettings, _spawnOnStart);
            _queryResults = new NativeArray<Vector3>(_spawnOnStart, Allocator.Persistent);
            #endregion
        }

        private void FixedUpdate()
        {
            ConstructOctree();
            CalculateParticleForces();
            ApplyParticleForces();
        }

        void ConstructOctree()
        {
            Octree.CleanAndPrepareForInsertion(new AABB(transform.position, _radiusToSpawnIn * _octreeRadiusMultiplier));
            
            for (int i = 0; i < _particles.Count; i++)
            {
                var p3 =  _particles[i].transform.position;
                Octree.Insert(p3);
            }
        }
        
        void CalculateParticleForces()
        {
            #region collision and force calc
            for (int i = 0; i < _particles.Count; i++)
            {
                var p1Pos = _particles[i].transform.position;

                var querySphere = new Sphere(p1Pos, _maxRadius);
                
                int queryResultCount = Octree.QueryNonAlloc(querySphere, _queryResults);
                    
                for (int j = 0; j < queryResultCount; j++)
                {
                    var p2Pos = _queryResults[j];

                    if (p2Pos == p1Pos) continue; //if the same particle
                    
                    Vector3 p1ToP2 = p2Pos - p1Pos;
                    float p1P2Dist = p1ToP2.magnitude;
                    Vector3 p1ToP2Dir = p1ToP2 / p1P2Dist;
                    

                    if (p1P2Dist < _maxRadius)
                    {
                        float percentageBetweenMinMax = Mathf.InverseLerp(_minRadius, _maxRadius, p1P2Dist);
                        float desiredPercentageBetweenMinMax = _desiredPercentBetweenMinMax;
                        float currentToDesiredPercentage = percentageBetweenMinMax - desiredPercentageBetweenMinMax;
                        float desiredDist = Mathf.Lerp(_minRadius, _maxRadius, _desiredPercentBetweenMinMax);
                        
                        float indexInCurve;
                        if (p1P2Dist < desiredDist)
                            indexInCurve = -Mathf.InverseLerp(desiredDist, _minRadius, p1P2Dist); //0, -1
                        else 
                            indexInCurve = Mathf.InverseLerp(desiredDist, _maxRadius, p1P2Dist); //0, 1
                        
                        float scale = currentToDesiredPercentage * _constantMultiplier * _forceMultiplierCurve.Evaluate(indexInCurve);
                        Vector3 posToAddScaled = p1ToP2Dir * scale;
                        
                        _particlesToMove[i] += posToAddScaled;
                    }
                }
                
            }
            #endregion
        }

        void ApplyParticleForces()
        {
            #region move particles
            for (int i = 0; i < _particles.Count; i++)
            {
                var pos = _particles[i].transform.position;
                var newPos = pos + _particlesToMove[i];

                _particles[i].RigidBody.MovePosition(newPos);
                _particlesToMove[i] = Vector3.zero;
            }
            #endregion
        }
        
        

        private void Update()
        {
            // SendParticlesToShader();
        }

        private void SendParticlesToShader()
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i].RigidBody.position;
                _particlePositions[i] = new Vector4(p.x, p.y, p.z, 0);
            }
            
            _material.SetVectorArray(PARTICLES_UNIFORM, _particlePositions);
            _material.SetInt(PARTICLES_LENGTH_UNIFORM, _particlePositions.Count);
        }

        private void OnDestroy()
        {
            Octree.Dispose();
            _queryResults.Dispose();
        }

        private void OnDrawGizmosSelected()
        {
            if (_particles != null && DrawParticles)
            {
                Random ran = Random.CreateFromIndex(0);
                for (int i = 0; i < _particles.Count; i++)
                {
                    var particle = _particles[i];
                    var color = Common.RandomColor(ref ran);
                    Gizmos.color = color;

                    Gizmos.DrawSphere(particle.RigidBody.position, _minRadius);
                    Gizmos.DrawWireSphere(particle.RigidBody.position, _maxRadius);
                    Gizmos.DrawWireSphere(particle.RigidBody.position, Mathf.Lerp(_minRadius, _maxRadius, _desiredPercentBetweenMinMax));
                }
            }

            if (Octree != null && DrawOctree)
            {
                Random ran = Random.CreateFromIndex(3759);
                for (int i = 0; i < Octree.Nodes.Length; i++)
                {
                    var nodes = Octree.Nodes;
                    var width = nodes[i].AABB.HalfWidth * 2;
                    
                    var color = Common.RandomColor(ref ran);
                    Gizmos.color = color;
                    
                    // Gizmos.DrawWireCube(nodes[i].AABB.Center, new Vector3(width, width, width));
                    float offset = 0.005f;
                    Gizmos.DrawWireCube(nodes[i].AABB.Center, new Vector3(width, width, width) - new Vector3(offset,offset,offset));

                    Octree.GetValuesAsArray(nodes[i], out var nodeValues);
                    for (int j = 0; j < nodeValues.Length; j++)
                        Gizmos.DrawSphere(nodeValues[j].Position, 0.05f);
                    nodeValues.Dispose();

                }
                
            }
        }
    }
}