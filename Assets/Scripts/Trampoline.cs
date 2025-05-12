using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float _trampolineForce = 10.0f;

    private Animation _animation;

    private void Start()
    {
        _animation = GetComponentInParent<Animation>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.TryGetComponent(out Rigidbody rb))
        {
            rb.AddForce(rb.gameObject.transform.up * _trampolineForce, ForceMode.Force);
            _animation.Play();
        }
    }
}
