/*
InputPanel.cs - MonoBehaviour for ARpoise input handling.

Copyright (C) 2018, Tamiko Thiel and Peter Graf - All Rights Reserved

ARpoise - Augmented Reality point of interest service environment 

This file is part of ARpoise.

    ARpoise is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ARpoise is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ARpoise.  If not, see <https://www.gnu.org/licenses/>.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
ARpoise, see www.ARpoise.com/

*/

using UnityEngine;
using UnityEngine.UI;

namespace com.arpoise.arpoiseapp
{
    public enum ArpoiseSettings
    {
        ArpoiseSettingsLatitude = 0,
        ArpoiseSettingsLongitude = 1,
        ArpoiseSettingsActivated = 2
    }

    public class InputPanel : MonoBehaviour
    {
        public GameObject GameObject;
        public InputField LatInputField;
        public InputField LonInputField;
        public Toggle ActivationToggle;
        public Button CloseButton;

        protected void Start()
        {
            CloseButton.onClick.AddListener(HandleClick);
        }

        private ArBehaviourUserInterface _behaviour;
        public void Activate(ArBehaviourUserInterface behaviour)
        {
            _behaviour = behaviour;

            LatInputField.text = PlayerPrefs.GetString(nameof(ArpoiseSettings.ArpoiseSettingsLatitude));
            LonInputField.text = PlayerPrefs.GetString(nameof(ArpoiseSettings.ArpoiseSettingsLongitude));
            ActivationToggle.isOn = true.ToString().Equals(PlayerPrefs.GetString(nameof(ArpoiseSettings.ArpoiseSettingsActivated)));
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
            PlayerPrefs.SetString(nameof(ArpoiseSettings.ArpoiseSettingsLatitude), LatInputField.text);
            PlayerPrefs.SetString(nameof(ArpoiseSettings.ArpoiseSettingsLongitude), LonInputField.text);
            PlayerPrefs.SetString(nameof(ArpoiseSettings.ArpoiseSettingsActivated), ActivationToggle.isOn ? true.ToString() : string.Empty);

            //Debug.Log("InputPanel " + false);
            GameObject.SetActive(false);
            if (_behaviour != null)
            {
                _behaviour.HandleInputPanelClosed(GetLatitude(), GetLongitude());
            }
        }
    }
}
