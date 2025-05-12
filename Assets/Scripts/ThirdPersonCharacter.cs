using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonCharacter : MonoBehaviour
{
    [Header("<color=#997570>Animator</color>")]
    [SerializeField] private string _airBoolName = "isOnAir"; 
    [SerializeField] private string _jumpTriggerName = "onJump"; 
    [SerializeField] private string _xFloatName = "xAxis"; 
    [SerializeField] private string _zFloatName = "zAxis";

    [Header("<color=#997570>Inputs</color>")]
    [SerializeField] private KeyCode _actionKey = KeyCode.E;
    [SerializeField] private KeyCode _interactKey = KeyCode.F;
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;

    [Header("<color=#997570>Movement</color>")]
    [SerializeField] private float _jumpForce = 7.5f;
    [SerializeField] private float _moveSpeed = 4.0f;

    [Header("<color=#997570>Physics</color>")]
    [SerializeField] private float _groundDistance = 0.25f;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _interactDistance = 2.0f;
    [SerializeField] private float _interactRadius =0.5f;
    [SerializeField] private LayerMask _interactMask;

    [Header("<color=#997570>Shaders</color>")]
    [SerializeField] private string _invisibilityName = "_InvisiblityAmount";

    private bool _isTurning = false, _isInvisible = false;

    private Vector3 _dir = new(), _dirFix = new(), _cameraForwardFix = new(), _cameraRightFix = new(), _groundOffset = new();

    private Animator _animator;
    private Rigidbody _rb;
    private Material[] _characterMats;
    private Renderer[] _characterRenderers;
    private ThirdPersonCamera _camera;
    private Transform _cameraTransform;

    private Ray _groundRay, _interactRay;
    private RaycastHit _interactHit;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();

        _cameraTransform = Camera.main.transform;
        _camera= Camera.main.GetComponentInParent<ThirdPersonCamera>();

        _characterRenderers = GetComponentsInChildren<Renderer>();
        _characterMats = new Material[_characterRenderers.Length];
        for(int i = 0; i < _characterRenderers.Length; i++)
        {
            _characterMats[i] = _characterRenderers[i].material;
        }
    }

    private void Update()
    {
        _dir.x = Input.GetAxis("Horizontal");
        _animator.SetFloat(_xFloatName, _dir.x);
        _dir.z = Input.GetAxis("Vertical");
        _animator.SetFloat(_zFloatName, _dir.z);

        if (Input.GetKeyDown(_interactKey))
        {
            Interaction();
        }

        _animator.SetBool(_airBoolName, IsOnAir());

        if (Input.GetKeyDown(_jumpKey) && !IsOnAir())
        {
            _animator.SetTrigger(_jumpTriggerName);
            Jump();
        }

        if(Input.GetKeyDown(_actionKey) && !_isTurning)
        {
            StartCoroutine(InvisibilityState());
        }
    }

    private void FixedUpdate()
    {
        if(_dir.sqrMagnitude != 0.0f)
        {
            Movement(_dir);
        }
    }

    private void Interaction()
    {
        _interactRay = new Ray(_camera.Target.position, _camera.Target.forward);

        if (Physics.SphereCast(_interactRay, _interactRadius, out _interactHit, _interactDistance, _interactMask))
        {
            if (_interactHit.collider.TryGetComponent(out IInteractable inter))
            {
                inter.OnInteract();
            }
        }
    }

    private bool IsOnAir()
    {
        _groundOffset = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);

        _groundRay = new Ray(_groundOffset, -transform.up);

        return !Physics.Raycast(_groundRay, _groundDistance, _groundMask);
    }

    private void Jump()
    {
        _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
    }

    private void Movement(Vector3 dir)
    {
        _cameraForwardFix = _cameraTransform.forward;
        _cameraRightFix = _cameraTransform.right;

        _cameraForwardFix.y = 0.0f;
        _cameraRightFix.y = 0.0f;

        Rotate(_cameraForwardFix);

        _dirFix = (_cameraRightFix * dir.x + _cameraForwardFix * dir.z).normalized;

        _rb.MovePosition(transform.position + _dirFix * _moveSpeed * Time.fixedDeltaTime);
    }

    private void Rotate(Vector3 dir)
    {
        transform.forward = dir;
    }

    private IEnumerator InvisibilityState()
    {
        _isTurning = true;

        float t = 0.0f;

        while(t < 1.0f)
        {
            t += Time.deltaTime;

            foreach (Material mat in _characterMats)
            {
                if (_isInvisible)
                {
                    mat.SetFloat(_invisibilityName, Mathf.Lerp(0.9f, 0.0f, t));
                }
                else
                {
                    mat.SetFloat(_invisibilityName, Mathf.Lerp(0.0f, 0.9f, t));
                }
            }

            yield return null;
        }

        _isInvisible = !_isInvisible;

        foreach (Material mat in _characterMats)
        {
            if (_isInvisible)
            {
                mat.SetFloat(_invisibilityName, 0.9f);
                for (int i = 0; i < _characterRenderers.Length; i++)
                {
                    _characterRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
                }
            }
            else
            {
                mat.SetFloat(_invisibilityName, 0.0f);
                for (int i = 0; i < _characterRenderers.Length; i++)
                {
                    _characterRenderers[i].shadowCastingMode = ShadowCastingMode.On;
                }
            }
        }        

        _isTurning = false;
    }
}
