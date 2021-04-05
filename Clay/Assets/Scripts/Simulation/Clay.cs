using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

namespace ClaySimulation
{
    [RequireComponent(typeof(Rigidbody))]
    public class Clay : MonoBehaviour
    {
        [Required] public Rigidbody RigidBody;
    }
}
