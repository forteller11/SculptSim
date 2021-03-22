using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;

namespace ClaySimulation
{
    public class ClayCollisionManager : MonoBehaviour
    {
        [ShowInInspector] private List<Clay> _particles;
        private List<Vector3> _particlesToMove;
        [Required] [SerializeField] float _minRadius;
        [Required] [SerializeField] float _maxRadius;
        [SerializeField] float _desiredPercentBetweenMinMax = .5f;
        [Tooltip("x== 0 means at desired percent, -1 == at min, 1 == at max")] 
        [SerializeField] AnimationCurve _forceMultiplierCurve = new AnimationCurve(new Keyframe(-1, 1), new Keyframe(0, 0), new Keyframe(1, 1));

        [SerializeField] private float _forceMultiplier = 1f;
        private void Start()
        {
            _particles = gameObject.GetComponentsInChildren<Clay>().ToList();
            
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
                        //todo is colliding?

                        float percentageBetweenMinMax = Mathf.InverseLerp(_minRadius, _maxRadius, p1p2Dist);
                        float desiredPercentageBetweenMinMax = _desiredPercentBetweenMinMax;
                        float currentToDesiredPercentage = percentageBetweenMinMax - desiredPercentageBetweenMinMax;

                        float indexInCurve;
                        if (percentageBetweenMinMax < _desiredPercentBetweenMinMax)
                            indexInCurve = -Mathf.InverseLerp(_desiredPercentBetweenMinMax, _minRadius, p1p2Dist); //0, -1
                        else 
                            indexInCurve = Mathf.InverseLerp(_desiredPercentBetweenMinMax, _maxRadius, p1p2Dist); //0, 1

                        float scale = currentToDesiredPercentage * _forceMultiplierCurve.Evaluate(indexInCurve) * _forceMultiplier;
                        Vector3 posToAddScaled = p1ToP2Dir * scale;
                        
                        _particlesToMove[i] += posToAddScaled;
                    }
                }
                
            }
            #endregion

            #region move particles
            for (int i = 0; i < _particles.Count; i++)
            {
                 //var pos = _particles[i].RigidBody.position;
                var newPos = _particlesToMove[i];
                _particles[i].RigidBody.AddForce(newPos);
                _particlesToMove[i] = Vector3.zero;
            }
            #endregion
        }

        private void OnDrawGizmosSelected()
        {
            Unity.Mathematics.Random ran = Unity.Mathematics.Random.CreateFromIndex(0);
            
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