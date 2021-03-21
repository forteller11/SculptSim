using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ClaySimulation
{


    [RequireComponent(typeof(Rigidbody))]
    public class Clay : MonoBehaviour
    {
        // [Required] [SerializeField] float _maxRadius;
        [Required] [SerializeField] float _minRadius;
        [Required] [SerializeField] float _maxRadius;
        private float DistBetweenMinMax => _maxRadius - _minRadius;
        [SerializeField] float _desiredPercentBetweenMinMax = .5f;

        [SerializeField] [Range(0, 1)] float _asymtoticPos = 0.5f;

        [ShowInInspector] float _lastDesiredPosChange = 0.0f;

        public Rigidbody RigidBody { get; private set; }

        private void Start()
        {
            RigidBody = GetComponent<Rigidbody>();
        }



        private void FixedUpdate()
        {
            var minRadius = _minRadius;
            var maxRadius = _maxRadius;

            Vector3 position = RigidBody.position;
            Vector3 posChange = Vector3.zero;

            float maxCollisionRadius = maxRadius - (maxRadius / 2); //equivalent of point collision
            //todo non alloc
            var overlaps = Physics.OverlapSphere(position, maxCollisionRadius);


            _lastDesiredPosChange = 0;

            for (int i = 0; i < overlaps.Length; i++)
            {
                var otherClay = overlaps[i].GetComponent<Clay>();
                if (otherClay == null ||
                    otherClay == this)
                    continue;

                var toOther = otherClay.RigidBody.position - position;
                var toOtherDist = Vector3.Distance(otherClay.RigidBody.position, position);
                var toOtherDir = toOther.normalized;

                var percentBetween = Mathf.InverseLerp(_minRadius, _maxRadius, toOtherDist);

                var percentDeltaToDesired = _desiredPercentBetweenMinMax - percentBetween;

                var distToDesired = percentDeltaToDesired * DistBetweenMinMax;
                var desiredToAdd = toOtherDir * distToDesired;
                // var desiredPos = position + (toOtherDir * percentDeltaToDesired * DistBetweenMinMax);



                // var interpolatedPosChange = desiredPosChange * _asymtoticPos;

                posChange += desiredToAdd;
                _lastDesiredPosChange += distToDesired;
            }


            var newPos = position + posChange * Time.deltaTime;
            RigidBody.MovePosition(newPos);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, .2f, .6f, .5f);
            Gizmos.DrawWireSphere(transform.position, _minRadius);
            Gizmos.color = new Color(.6f, 0f, .2f, .5f);
            Gizmos.DrawWireSphere(transform.position, _maxRadius);
            Gizmos.color = new Color(.4f, 1f, .2f, .8f);
            Gizmos.DrawWireSphere(transform.position, Mathf.Lerp(_minRadius, _maxRadius, _desiredPercentBetweenMinMax));
        }
    }
}
