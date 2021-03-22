using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Mathematics;
using Random =Unity.Mathematics.Random;


namespace ClaySimulation
{
    public class ClayCollisionManager : MonoBehaviour
    {
        [ShowInInspector] private List<Clay> _particles;
        [FoldoutGroup("Spawn")] [SerializeField] [AssetsOnly] private Clay _particlePrefab;
        [FoldoutGroup("Spawn")] [SerializeField] private int _spawnOnStart = 10;
        [FoldoutGroup("Spawn")] [SerializeField] private float _radiusToSpawnIn = 5;
        
        [FoldoutGroup("Sim Settings")] [SerializeField] float _minRadius = 0;
        [FoldoutGroup("Sim Settings")] [SerializeField] float _maxRadius = 2;
        [FoldoutGroup("Sim Settings")] [SerializeField] float _desiredPercentBetweenMinMax = .5f;
        [FoldoutGroup("Sim Settings")] [SerializeField] [Range(0,1)] private float  _constantMultiplier = .5f;
        
        private List<Vector3> _particlesToMove;
        private void Start()
        {
            //var alreadyTheregameObject.GetComponentsInChildren<Clay>();

            _particles = new List<Clay>(_spawnOnStart);
            var random = Random.CreateFromIndex((uint)System.DateTime.Now.Millisecond);
            for (int i = 0; i < _spawnOnStart; i++)
            {
                var newParticle = Instantiate(_particlePrefab, transform);
                var randomOutput = (Vector3) random.NextFloat3() - new Vector3(0.5f, 0.5f, 0.5f);
                Debug.Log(randomOutput);
                var startingPos = randomOutput * _radiusToSpawnIn;
                newParticle.transform.position = startingPos + transform.position;
                
                _particles.Add(newParticle);
            }
            
            
            _particlesToMove = new List<Vector3>(_particles.Count);
            for (int i = 0; i < _particles.Count; i++)
                _particlesToMove.Add(Vector3.zero);
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

                        float percentageBetweenMinMax = Mathf.InverseLerp(_minRadius, _maxRadius, p1p2Dist);
                        float desiredDist = Mathf.Lerp(_minRadius, _maxRadius, _desiredPercentBetweenMinMax);

                        Vector3 desiredPosRelative = p1ToP2Dir * desiredDist;
                        
                        Vector3 currentPosRelative = p1ToP2;
                        Vector3 toDesiredPosRelative = desiredPosRelative;
                        
                        float currentToDesiredPercentage = -percentageBetweenMinMax + _desiredPercentBetweenMinMax;

                        float dynamicMultiplier;
                        if (p1p2Dist < desiredDist)
                        {
                            float percentageDesiredToMin = Mathf.InverseLerp(desiredDist, _minRadius, percentageBetweenMinMax);
                            dynamicMultiplier = Common.EaseOutQuart(percentageDesiredToMin);
                        }
                        else
                        {
                            float percentageDesiredToMax = Mathf.InverseLerp(desiredDist, _maxRadius, percentageBetweenMinMax);
                            dynamicMultiplier = Common.EaseInQuart(percentageDesiredToMax);
                        }


                        // float scale = currentToDesiredPercentage  * _constantMultiplier;
                        
                        Vector3 posToAddScaled = toDesiredPosRelative * _constantMultiplier;
                        
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