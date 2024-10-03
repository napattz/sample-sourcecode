using UnityEngine;


public abstract class BaseNightVisionManager : MonoBehaviour
{
    public static BaseNightVisionManager Singleton { get; private set; }

    private bool _isOn;
    public bool IsOn
    {
        get
        {
            return _isOn;
        }
        set
        {
            if (_isOn == value)
                return;
            _isOn = value;
            UpdateIsOn(value);
        }
    }

    protected virtual void Awake()
    {
        Singleton = this;
    }

    protected abstract void UpdateIsOn(bool isOn);
}

