/*
InputPanel.cs - MonoBehaviour for Arpoise, input handling.

Copyright (C) 2018, Tamiko Thiel and Peter Graf - All Rights Reserved

ARPOISE - Augmented Reality Point Of Interest Service 

This file is part of Arpoise.

    Arpoise is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Arpoise is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Arpoise.  If not, see <https://www.gnu.org/licenses/>.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
Arpoise, see www.Arpoise.com/

*/

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

        protected void Start()
        {
            CloseButton.onClick.AddListener(HandleClick);
        }

        private ArBehaviourUserInterface _behaviour;
        public void Activate(ArBehaviourUserInterface behaviour)
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

            //Debug.Log("InputPanel " + false);
            GameObject.SetActive(false);
            if (_behaviour != null)
            {
                _behaviour.HandleInputPanelClosed(GetLatitude(), GetLongitude());
            }
        }
    }
}
