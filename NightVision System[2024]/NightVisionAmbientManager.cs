using DG.Tweening;
using MultiplayerARPG;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NightVisionAmbientManager : BaseNightVisionManager
{
    public bool On
    {
        get { return nightVisionEnable; }
        set
        {
            if (enabled == true && nightVisionEnable != value)
            {
                nightVisionEnable = IsOn;

                if (AudioManager.Singleton != null)
                {
                    if (nightVisionEnable == true && _switchOn != null)
                        AudioManager.PlaySfxClipAtPoint(_switchOn, GameInstance.PlayingCharacter.CurrentPosition);
                    else if (nightVisionEnable == false && _switchOff != null)
                        AudioManager.PlaySfxClipAtPoint(_switchOff, GameInstance.PlayingCharacter.CurrentPosition);
                }
                SwitchNightVision(nightVisionEnable);
            }
        }
    }
    [SerializeField]
    private bool nightVisionEnable = false;
    [SerializeField] private AudioClip _switchOn;
    [SerializeField] private AudioClip _switchOff;
    [SerializeField] private Color _ambientColor;
    [SerializeField] private Color _previousambientColor;
    [SerializeField] private Volume _volume;
    private float _vignetteStartIntensity = 0f;
    private float _vignetteEndIntensity = 0.35f;
    private float _exposureStartValue = 5f;
    private float _exposureEndValue = 0f;
    private float _animationDuration = 0.5f;

    private Light[] _lightInScene;
    private Vignette _vignette;
    private ColorAdjustments _colorAdjustments;
    private DayNightController _dayNightController;

    private void Start()
    {
        _volume.weight = 0;
        _dayNightController = FindObjectOfType<DayNightController>();
        if (_volume.profile.TryGet(out _vignette))
        {
            _vignette.intensity.value = _vignetteStartIntensity;
        }
        else
        {
            Debug.LogError("Vignette not found in the Volume!");
        }
        if (_volume.profile.TryGet(out _colorAdjustments))
        {
            _colorAdjustments.postExposure.value = _exposureStartValue;
        }
        else
        {
            Debug.LogError("Vignette not found in the Volume!");
        }
    }

    private void OnDestroy()
    {
        
    }

    private void SwitchNightVision(bool isOn)
    {
        DOTween.KillAll();
        ToggleAnimation(isOn);
        if (RenderSettings.ambientMode == AmbientMode.Flat && isOn)
        {
            _previousambientColor = RenderSettings.ambientLight;
            RenderSettings.ambientLight = _ambientColor;
        }
        else
        {
            if (_dayNightController != null)
            {
                // Transition to current time immediately!
#if !UNITY_SERVER
                _dayNightController.TimeTransition(_animationDuration);
#endif
            }
        }
    }

    private void ToggleAnimation(bool _isOn)
    {
        float targetVignetteIntensity = _isOn ? _vignetteEndIntensity : _vignetteStartIntensity;
        float targetExposureValue = _isOn ? _exposureEndValue : _exposureStartValue;
        float targetWeight = _isOn ? 1 : 0;

        DOTween.Sequence()
            .Append(DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, targetVignetteIntensity, _animationDuration / 2)
                .SetEase(Ease.Linear))
            .Join(DOTween.To(() => _colorAdjustments.postExposure.value, x => _colorAdjustments.postExposure.value = x, targetExposureValue, _animationDuration)
                .SetEase(Ease.Linear))
            .Join(DOTween.To(() => _volume.weight, x => _volume.weight = x, targetWeight, _animationDuration / 2)
                .SetEase(Ease.Linear))
            .OnComplete(() => {
                _isOn = !_isOn;
            });
    }

    private void CheckLightInMap()
    {
        Light[] allLights = FindObjectsOfType<Light>();
        _lightInScene = new Light[allLights.Length];
        for (int i = 0; i < allLights.Length; i++)
        {
            _lightInScene[i] = allLights[i];
        }
    }

    protected override void UpdateIsOn(bool isOn)
    {
        On = isOn;
    }
}
