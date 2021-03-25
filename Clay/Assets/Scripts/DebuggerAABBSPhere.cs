// using System;
// using System.Collections;
// using System.Collections.Generic;
// using DefaultNamespace;
// using UnityEngine;
//
// public class DebuggerAABBSPhere : MonoBehaviour
// {
//     [SerializeField] SphereCollider _sphereCollider;
//     [SerializeField] BoxCollider _boxCollider;
//
//     private void Update()
//     {
//         
//     }
//
//     private void OnDrawGizmosSelected()
//     {
//         if (_sphereCollider == null || _boxCollider == null)
//             return;
//         
//         if (Common.SphereAABBOverlap(_sphereCollider.transform.position, _sphereCollider.radius, _boxCollider.transform.position, _boxCollider.size.x / 2f)){
//             Gizmos.color = Color.green;
//         }
//        else 
//             Gizmos.color = Color.red;
//         
//         Gizmos.DrawSphere(_sphereCollider.transform.position, _sphereCollider.radius);
// }
// }
