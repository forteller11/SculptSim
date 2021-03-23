using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;
using Random =Unity.Mathematics.Random;


namespace ClaySimulation
{
    public class ClayCollisionManager : MonoBehaviour
    {
        
        [FoldoutGroup("Spawn")] [SerializeField] [AssetsOnly] private Clay _particlePrefab;
        [FoldoutGroup("Spawn")] [SerializeField] private int _spawnOnStart = 10;
        [FoldoutGroup("Spawn")] [SerializeField] private float _radiusToSpawnIn = 5;
        
        [FoldoutGroup("Sim Settings")] [SerializeField] float _minRadius = 0;
        [FoldoutGroup("Sim Settings")] [SerializeField] float _maxRadius = 2;
        [FoldoutGroup("Sim Settings")] [SerializeField] float _desiredPercentBetweenMinMax = .5f;
        [FoldoutGroup("Sim Settings")] [SerializeField] [Range(0,1)] private float  _constantMultiplier = .05f;
        [Tooltip("x== 0 means at desired percent, -1 == at min, 1 == at max")] 
        [FoldoutGroup("Sim Settings")] [SerializeField] AnimationCurve _forceMultiplierCurve = new AnimationCurve(new Keyframe(-1, 1), new Keyframe(0, 0), new Keyframe(1, 1));
        
        [ShowInInspector] private List<Clay> _particles;
        [ShowInInspector] private List<Vector3> _particlesToMove;
        [ShowInInspector] private List<Vector4> _particlePositions;
        [ShowInInspector] private Material _material;
        
        private static readonly int PARTICLES_LENGTH_UNIFORM = Shader.PropertyToID("_ParticlesLength");
        private static readonly int PARTICLES_UNIFORM = Shader.PropertyToID("_Particles");

        private void Start()
        {
  
            //var alreadyTheregameObject.GetComponentsInChildren<Clay>();
            _material = GetComponent<MeshRenderer>().material;
            
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
                _particlePositions.Add(Vector4.zero);
        }

        private void FixedUpdate()
        {
            #region collision and force calc
            for (int i = 0; i < _particles.Count; i++)
            {
                var p1Pos = _particles[i].transform.position;
                
                for (int j = 0; j < _particles.Count; j++)
                {
                    if (i == j) continue;
                    var p2Pos = _particles[j].transform.position;

                    Vector3 p1ToP2 = p2Pos - p1Pos;
                    float p1p2Dist = p1ToP2.magnitude;
                    Vector3 p1ToP2Dir = p1ToP2 / p1p2Dist;
                    

                    if (p1p2Dist < _maxRadius)
                    {
                        //todo is colliding?

                        float percentageBetweenMinMax = Mathf.InverseLerp(_minRadius, _maxRadius, p1p2Dist);
                        float desiredPercentageBetweenMinMax = _desiredPercentBetweenMinMax;
                        float currentToDesiredPercentage = percentageBetweenMinMax - desiredPercentageBetweenMinMax;
                        float desiredDist = Mathf.Lerp(_minRadius, _maxRadius, _desiredPercentBetweenMinMax);
                        
                        float indexInCurve;
                        if (p1p2Dist < desiredDist)
                            indexInCurve = -Mathf.InverseLerp(desiredDist, _minRadius, p1p2Dist); //0, -1
                        else 
                            indexInCurve = Mathf.InverseLerp(desiredDist, _maxRadius, p1p2Dist); //0, 1
                        
                        float scale = currentToDesiredPercentage * _constantMultiplier * _forceMultiplierCurve.Evaluate(indexInCurve);
                        Vector3 posToAddScaled = p1ToP2Dir * scale;
                        
                        _particlesToMove[i] += posToAddScaled;
                    }
                }
                
            }
            #endregion

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
            SendParticlesToShader();
        }

        private void SendParticlesToShader()
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                var pos = _particles[i].RigidBody.position;
                _particlePositions[i] = new Vector4(pos.x, pos.y, pos.z, 0);
            }
            
            _material.SetVectorArray(PARTICLES_UNIFORM, _particlePositions);
            _material.SetInt(PARTICLES_LENGTH_UNIFORM, _particlePositions.Count);
        }
        

        private void OnDrawGizmosSelected()
        {

            if (_particles != null)
            {
                Random ran = Random.CreateFromIndex(0);
                for (int i = 0; i < _particles.Count; i++)
                {
                    var particle = _particles[i];
                    var color = Common.RandomColor(ran);
                    Gizmos.color = color;

                    Gizmos.DrawWireSphere(particle.RigidBody.position, _minRadius);
                    Gizmos.DrawWireSphere(particle.RigidBody.position, _maxRadius);
                    Gizmos.DrawWireSphere(particle.RigidBody.position, Mathf.Lerp(_minRadius, _maxRadius, _desiredPercentBetweenMinMax));
                }
            }
        }
    }
}