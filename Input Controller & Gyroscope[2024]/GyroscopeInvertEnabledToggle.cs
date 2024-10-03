using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class GyroscopeInvertEnabledToggle : MonoBehaviour
    {
        public const string SAVE_KEY = "GYROSCOPE_INVERT_ENABLE";
        public bool setting = true;
        public static bool IsInvert = false;
        public Toggle toggle;
        public Button button;
        public bool applyImmediately = true;
        public bool ApplyImmediately { get { return applyImmediately; } set { applyImmediately = value; } }

        private bool _isOn;

        private void Start()
        {
            if (toggle != null)
            {
                toggle.SetIsOnWithoutNotify(GetGyroscopeInvertValue() == setting);
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
                SetGyroscopeInvertValue(setting);
                PlayerPrefs.SetInt(SAVE_KEY, setting ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static void Load()
        {
            SetGyroscopeInvertValue(PlayerPrefs.GetInt(SAVE_KEY, GetGyroscopeInvertValue() ? 1 : 0) > 0);
        }

        public static bool GetGyroscopeInvertValue()
        {
            return IsInvert;
        }

        public static void SetGyroscopeInvertValue(bool isInvert)
        {
            IsInvert = isInvert;
        }
    }
}
