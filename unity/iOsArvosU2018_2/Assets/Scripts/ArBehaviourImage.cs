/*
ArBehaviourImage.cs - MonoBehaviour for Arpoise, image handling.

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

#if HAS_AR_CORE
using GoogleARCore;
#else
#endif

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace com.arpoise.arpoiseapp
{
    public class ArBehaviourImage : ArBehaviourUserInterface
    {
        #region Globals
#if HAS_AR_CORE
        public GameObject ArCoreDevice;
#endif
        public GameObject FitToScanOverlay;

#endregion

#region Start
        protected override void Start()
        {
            base.Start();

#if HAS_AR_CORE
            if (ArCoreDevice == null)
            {
                ErrorMessage = "ArCoreDevice needs to be set.";
                return;
            }

            var aRCoreSession = ArCoreDevice.GetComponent<ARCoreSession>();
            if (aRCoreSession == null)
            {
                ErrorMessage = "ARCoreSession needs to be set on ArCoreDevice.";
                return;
            }

            var aRCoreSessionConfig = aRCoreSession.SessionConfig;
            if (aRCoreSessionConfig == null)
            {
                ErrorMessage = "ARCoreSessionConfig needs to be set on ARCoreSession.";
                return;
            }

            AugmentedImageDatabase = aRCoreSessionConfig.AugmentedImageDatabase;
            if (AugmentedImageDatabase == null)
            {
                ErrorMessage = "AugmentedImageDatabase needs to be set on ARCoreSessionConfig.";
                return;
            }
#endif

#if HAS_AR_KIT
            if (InfoPanel != null)
            {
                var showInfoPanel = PlayerPrefs.GetString(nameof(InfoPanelIsActive));
                if (!false.ToString().Equals(showInfoPanel))
                {
                    var infoPanel = InfoPanel.GetComponent<InfoPanel>();
                    if (infoPanel != null)
                    {
                        infoPanel.Setup(this);
                        InfoPanel.SetActive(true);
                    }
                }
            }
#endif
            // Start GetPosition() coroutine 
            StartCoroutine("GetPosition");
            // Start GetData() coroutine 
            StartCoroutine("GetData");
        }
#endregion

#region Update
        protected override void Update()
        {
            base.Update();
        }
#endregion
    }
}
