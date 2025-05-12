using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("<color=#997570>Components</color>")]
    [SerializeField] private Transform _target;

    public Transform Target
    {
        get { return _target; }
    }

    [Header("<color=#997570>Cursor</color>")]
    [SerializeField] private CursorLockMode _lockMode = CursorLockMode.Locked;
    [SerializeField] private bool _isCursorVisible = false;

    [Header("<color=#997570>Physics</color>")]
    [Range(0.05f, 1.0f)][SerializeField] private float _detectionRadius = 0.1f;
    [SerializeField] private float _hitOffset = 0.25f;

    [Header("<color=#997570>Settings</color>")]
    [Range(10.0f, 1000.0f)][SerializeField] private float _mouseSensitivity = 500.0f;
    [Range(0.125f, 1.0f)][SerializeField] private float _minDistance = 0.25f;
    [Range(1.0f, 10.0f)][SerializeField] private float _maxDistance = 5.0f;
    [Range(-90.0f, 0.0f)][SerializeField] private float _minRotation = -85.0f;
    [Range(0.0f, 90.0f)][SerializeField] private float _maxRotation = 75.0f;

    private bool _isBlocked = false;
    private float _mouseX = 0.0f, _mouseY = 0.0f;
    private Vector3 _dir = new(), _dirTest = new(), _camPos = new();

    private Camera _camera;

    private Ray _cameraRay;
    private RaycastHit _cameraHit;

    private void Start()
    {
        _camera = Camera.main;

        Cursor.lockState = _lockMode;
        Cursor.visible = _isCursorVisible;

        transform.forward = _target.forward;

        _mouseX = transform.eulerAngles.y;
        _mouseY = transform.eulerAngles.x;
    }

    private void FixedUpdate()
    {
        _cameraRay = new Ray(transform.position, _dir);

        _isBlocked = Physics.SphereCast(_cameraRay, _detectionRadius, out _cameraHit, _maxDistance);
    }

    private void LateUpdate()
    {
        UpdateCamRot(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        UpdateSpringArm();
    }

    private void UpdateCamRot(float x, float y)
    {
        transform.position = _target.position;

        if (x == 0.0f && y == 0.0f) return;

        if (x != 0.0f)
        {
            _mouseX += x * _mouseSensitivity * Time.deltaTime;

            if (_mouseX > 360.0f || _mouseX < -360.0f)
            {
                _mouseX -= 360.0f * Mathf.Sign(_mouseX);
            }
        }

        if (y != 0.0f)
        {
            _mouseY += y * _mouseSensitivity * Time.deltaTime;

            _mouseY = Mathf.Clamp(_mouseY, _minRotation, _maxRotation);
        }

        transform.rotation = Quaternion.Euler(-_mouseY, _mouseX, 0.0f);
    }

    private void UpdateSpringArm()
    {
        _dir = -transform.forward;

        if (_isBlocked)
        {
            _dirTest = (_cameraHit.point - transform.position) + (_cameraHit.normal * _hitOffset);

            if (_dirTest.sqrMagnitude <= _minDistance * _minDistance)
            {
                _camPos = transform.position + _dir * _minDistance;
            }
            else
            {
                _camPos = transform.position + _dirTest;
            }
        }
        else
        {
            _camPos = transform.position + _dir * _maxDistance;
        }

        _camera.transform.position = _camPos;
        _camera.transform.LookAt(transform.position);
    }
}
