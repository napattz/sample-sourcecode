using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class GyroscopEnabledToggle : MonoBehaviour
    {
        public const string SAVE_KEY = "GYROSCOPE_ENABLE";
        public bool setting = true;
        public static bool IsOn = false;
        public Toggle toggle;
        public Button button;
        public bool applyImmediately = true;
        public bool ApplyImmediately { get { return applyImmediately; } set { applyImmediately = value; } }

        private bool _isOn;

        private void Start()
        {
            if (toggle != null)
            {
                toggle.SetIsOnWithoutNotify(GetGyroscopeValue() == setting);
                toggle.onValueChanged.AddListener(OnToggle);
            }
            if (button != null)
                button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(OnToggle);
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }

        public void OnToggle(bool isOn)
        {
            _isOn = isOn;
            if (isOn)
                OnClick();
        }

        public void OnClick()
        {
            if (ApplyImmediately)
            {
                _isOn = true;
                Apply();
            }
        }

        public void Apply()
        {
            if (_isOn)
            {
                SetGyroscopeValue(setting);
                PlayerPrefs.SetInt(SAVE_KEY, setting ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static void Load()
        {
            SetGyroscopeValue(PlayerPrefs.GetInt(SAVE_KEY, GetGyroscopeValue() ? 1 : 0) > 0);
        }

        public static bool GetGyroscopeValue()
        {
            return IsOn;
        }

        public static void SetGyroscopeValue(bool isOn)
        {
            IsOn = Input.gyro.enabled = isOn;
        }
    }
}
