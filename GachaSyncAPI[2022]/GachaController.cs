using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GachaController : MonoBehaviour
{
    public GameObject pointerObject;
    public GachaItemPanel selectedObject;
    [HideInInspector] public bool isSkip;
    [SerializeField] private CogManager _cogManager;
    [SerializeField] private GachaUIController _gachaUIController;

    private RectTransform _gachaRect;
    private GachaSelectPointer _gachaSelectorPointer;
    private float _maxWheelSpeed = -70f;
    private float _finalSpeed = -25f;
    private float _lerpDuration = 3f;
    private float _valueToLerp;
    private float _dist;
    private float _timer;
    private bool _isSpin;
    private bool _prepareEndSpin;
    private bool _showReawrd;

    private Coroutine lerpCoroutine;

    private void Start()
    {
        _gachaSelectorPointer = pointerObject.GetComponent<GachaSelectPointer>();
        _gachaRect = GetComponent<RectTransform>();

        _valueToLerp = 0f;
        _isSpin = false;
        _prepareEndSpin = false;
        _showReawrd = false;
        isSkip = false;
    }

    private void FixedUpdate()
    {
        if (_isSpin)
        {
            _dist = Vector3.Distance(selectedObject.transform.position, pointerObject.transform.position);
            float pos = GetObjectPosition(pointerObject.transform, selectedObject.transform);

            if (_isSpin || pos < 0f || _valueToLerp < _finalSpeed)
            {
                _gachaRect.anchoredPosition = new Vector2(_gachaRect.anchoredPosition.x + _valueToLerp, _gachaRect.anchoredPosition.y);
                _cogManager.CogSpeed = _valueToLerp / -5;
            }
        }
        if (_prepareEndSpin && !isSkip)
        {
            if (_valueToLerp < _finalSpeed)
            {
                _gachaRect.anchoredPosition = new Vector2(_gachaRect.anchoredPosition.x + _valueToLerp, _gachaRect.anchoredPosition.y);
                _cogManager.CogSpeed = _valueToLerp / -5;
            }
            else if (_valueToLerp >= _finalSpeed)
            {
                _dist = Vector3.Distance(selectedObject.transform.position, pointerObject.transform.position);
                Vector3 _gachaRectPosition = _gachaRect.gameObject.transform.position;
                _gachaRect.gameObject.transform.position = Vector3.Lerp(new Vector3(_gachaRectPosition.x, _gachaRectPosition.y, _gachaRectPosition.z),
                                                                           new Vector3(_gachaRectPosition.x - _dist, _gachaRectPosition.y, _gachaRectPosition.z),
                                                                           Time.deltaTime*0.9f);
                _valueToLerp = _dist / -3;
                _cogManager.CogSpeed = _valueToLerp / -2;
                if (_dist <= 0.1)
                {
                    _cogManager.CogSpeed = 0;   
                    StopCoroutine(lerpCoroutine);
                    _valueToLerp = 0f;
                    if (!_showReawrd)
                    {
                        _showReawrd = true;
                        _gachaUIController.ShowReward();
                    }
                }
            }
        }

    }

    private void Update()
    {
        if (_isSpin)
        {
            _timer = _timer + 1f * Time.deltaTime;
            if (_timer >= 1f) 
            {
                _gachaSelectorPointer.SetEnableOfGachaPointer(true);
                _isSpin = false;
                _prepareEndSpin = true;
                _timer = 0;
            }
        }
        if (isSkip && _dist >= 0.1f)
        {
            _dist = Vector3.Distance(selectedObject.transform.position, pointerObject.transform.position);
            Vector3 _gachaRectPosition = _gachaRect.gameObject.transform.position;
            _gachaRect.gameObject.transform.position = Vector3.Lerp(new Vector3(_gachaRectPosition.x, _gachaRectPosition.y, _gachaRectPosition.z),
                                                                           new Vector3(_gachaRectPosition.x - _dist, _gachaRectPosition.y, _gachaRectPosition.z),
                                                                           1f);
        }
    }

    public void StartSpin()
    {
        selectedObject.tag = "Selected";
        _gachaSelectorPointer.SetEnableOfGachaPointer(false);
        _valueToLerp = 0f;
        _timer = 0;
        if (lerpCoroutine != null) { StopCoroutine(lerpCoroutine); }
        lerpCoroutine = StartCoroutine(Lerp(_valueToLerp, _maxWheelSpeed, 9f));
        _isSpin = true;
        _showReawrd = false;
        _prepareEndSpin = false;
    }

    public void ResetSpin()
    {
        StopCoroutine(lerpCoroutine);
        _valueToLerp = 0f;
        _cogManager.CogSpeed = 0;
        _isSpin = false;
        _prepareEndSpin = false;
    }

    public void StopWheel()
    {
        _valueToLerp = _maxWheelSpeed;
        if (lerpCoroutine != null) { StopCoroutine(lerpCoroutine); }
        lerpCoroutine = StartCoroutine(Lerp(_valueToLerp, _finalSpeed));
        _isSpin = false;
        _prepareEndSpin = true;
    }

    private float GetObjectPosition(Transform baseObject, Transform selectedObject)
    {
        Vector3 objPosition = selectedObject.position - baseObject.position;

        return objPosition.x;
    }

    IEnumerator Lerp(float startValue, float endValue, float boostSpeed = 1f)
    {
        _isSpin = true;

        float timeElapsed = 0;
        while (timeElapsed < _lerpDuration)
        {
            _valueToLerp = Mathf.Lerp(startValue, endValue, (timeElapsed / _lerpDuration) * boostSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        _valueToLerp = endValue;
    }

}
