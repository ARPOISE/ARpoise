/*
MenuButton.cs - Script handling clicks on the info panel.

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

/*
Welcome to AR-vos augmented reality app!

The AR-vos menu will present you with one or more different AR layers, depending on your location.

If you select a geolocative layer, you will be immediately surrounded by the augments. Look all around you, as they may be above, below or behind you!

If you select an image trigger layer, you then point the scan screen at a designated AR-vos trigger image to call up the augments.

For example AR-vos trigger images, see www.ar-vos.com/trigger
*/
using UnityEngine;
using UnityEngine.UI;

namespace com.arpoise.arpoiseapp
{
    public class InfoPanel : MonoBehaviour
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
                //Debug.Log("InfoPanel Start.");
            }
        }

        private ArBehaviourUserInterface _behaviour;
        public void Setup(ArBehaviourUserInterface behaviour)
        {
            Start();
            _behaviour = behaviour;
            //Debug.Log("InfoPanel Setup.");
        }

        public void HandleClick()
        {
            if (_behaviour != null)
            {
                _behaviour.HandleInfoPanelClosed();
                //Debug.Log("InfoPanel HandleClick.");
            }
        }
    }
}
