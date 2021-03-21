using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MoveObject : MonoBehaviour
{
    [SerializeField] float _moveMultiplier = 10f;

    [SerializeField] InputAction _xAxisInput;
    [SerializeField] InputAction _yAxisInput;
    [SerializeField] InputAction _zAxisInput;

    [SerializeField] [Required] private Camera _camera;
    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _xAxisInput.Enable();
        _yAxisInput.Enable();
        _zAxisInput.Enable();
    }

    private void Update()
    {

        #region move

        Vector3 moveInput = new Vector3(
            -_xAxisInput.ReadValue<float>(),
            _yAxisInput.ReadValue<float>(),
            _zAxisInput.ReadValue<float>()
        );
        
        Vector3 moveNorm = moveInput.normalized;
        Vector3 moveRelativeNorm = _camera.transform.rotation * moveNorm;
        Vector3 inputRelativeScaled = moveRelativeNorm * (_moveMultiplier * Time.deltaTime);
        
        _rigidbody.AddForce(inputRelativeScaled);
        #endregion
    }

}
