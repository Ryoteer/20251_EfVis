using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SwordSwitch : MonoBehaviour
{
    [Header("<color=orange>Renderer</color>")]
    [SerializeField] private float _transitionTime = 0.5f;

    private Animator _animator;
    private Material _swordMat;
    private VisualEffect _swordVfx;

    private bool _enabledState = false, _isChanging = false;
    private Vector4 _actualColor, _randomColor;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _swordMat = GetComponentInChildren<Renderer>().material;
        _swordVfx = GetComponentInChildren<VisualEffect>();

        _actualColor = _swordMat.GetColor("_AuraColor");
        _swordVfx.SetVector4("GeneralColor", _actualColor);
    }

    public void SetEvent(string eventName)
    {
        _swordVfx.SendEvent(eventName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _enabledState = !_enabledState;

            _animator.SetBool("isEnabled", _enabledState);
            _animator.SetTrigger("onSwitch");
        }
        else if (Input.GetKeyDown(KeyCode.C) && _enabledState && !_isChanging)
        {
            StartCoroutine(ChangeColor());
        }
    }

    private IEnumerator ChangeColor()
    {
        _isChanging = true;

        _randomColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);

        float t = 0.0f;

        while (t < 1)
        {
            t += Time.deltaTime / _transitionTime;

            _swordMat.SetColor("_AuraColor", Vector4.Lerp(_actualColor, _randomColor, t));
            _swordVfx.SetVector4("Color", Vector4.Lerp(_actualColor, _randomColor, t));

            yield return null;
        }

        SetEvent("OnSwordChange");

        _actualColor = _randomColor;

        _isChanging = false;
    }
}
