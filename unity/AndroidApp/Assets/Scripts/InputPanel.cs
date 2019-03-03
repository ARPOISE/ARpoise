using UnityEngine;
using UnityEngine.UI;

namespace com.arpoise.arpoiseapp
{
    public class InputPanel : MonoBehaviour
    {
        public GameObject GameObject;
        public InputField LatInputField;
        public InputField LonInputField;
        public Toggle ActivationToggle;
        public Button CloseButton;

        private void Start()
        {
            CloseButton.onClick.AddListener(HandleClick);
        }

        private ArBehaviour _behaviour;
        public void Activate(ArBehaviour behaviour)
        {
            _behaviour = behaviour;

            LatInputField.text = PlayerPrefs.GetString("ArpoiseSettingsLatitude");
            LonInputField.text = PlayerPrefs.GetString("ArpoiseSettingsLongitude");
            ActivationToggle.isOn = true.ToString().Equals(PlayerPrefs.GetString("ArpoiseSettingsActivated"));
        }

        public float? GetLongitude()
        {
            if (ActivationToggle.isOn)
            {
                float value;
                if (float.TryParse(LonInputField.text, out value))
                {
                    return value;
                }
            }
            return null;
        }

        public float? GetLatitude()
        {
            if (ActivationToggle.isOn)
            {
                float value;
                if (float.TryParse(LatInputField.text, out value))
                {
                    return value;
                }
            }
            return null;
        }

        public bool IsActivated()
        {
            return ActivationToggle.isOn;
        }

        public void HandleClick()
        {
            PlayerPrefs.SetString("ArpoiseSettingsLatitude", LatInputField.text);
            PlayerPrefs.SetString("ArpoiseSettingsLongitude", LonInputField.text);
            PlayerPrefs.SetString("ArpoiseSettingsActivated", ActivationToggle.isOn ? true.ToString() : string.Empty);

            GameObject.SetActive(false);
            if (_behaviour != null)
            {
                _behaviour.HandleInputPanelClosed(GetLatitude(), GetLongitude());
            }
        }
    }
}
