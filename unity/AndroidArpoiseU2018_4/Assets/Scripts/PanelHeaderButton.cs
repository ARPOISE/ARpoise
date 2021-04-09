/*
PanelHeaderButton.cs - Script handling clicks on the ARpoise layer panel header button.

Copyright (C) 2019, Tamiko Thiel and Peter Graf - All Rights Reserved

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
