/*
PanelHeaderButton.cs - Script handling clicks on the layer panel header button.

Copyright (C) 2019, Tamiko Thiel and Peter Graf - All Rights Reserved

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
    public class PanelHeaderButton : MonoBehaviour
    {
        public Button Button;
        private bool _started;

        // Use this for initialization
        protected void Start()
        {
            if (!_started)
            {
                _started = true;
                Button.onClick.AddListener(HandleClick);
                //Debug.Log("PanelHeaderButton Start.");
            }
        }

        private ArBehaviourUserInterface _behaviour;
        public void Setup(ArBehaviourUserInterface behaviour)
        {
            Start();
            _behaviour = behaviour;
            //Debug.Log("PanelHeaderButton Setup.");
        }

        public void HandleClick()
        {
            if (_behaviour != null)
            {
                _behaviour.HandlePanelHeaderButtonClick();
                //Debug.Log("PanelHeaderButton HandleClick.");
            }
        }
    }
}
