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
    public class ArBehaviourImage : ArBehaviourData
    {
        #region Globals

        public GameObject ArCoreDevice = null;
        public GameObject InfoText = null;

        #endregion

        private static readonly string _loadingText = "Loading data, please wait";
        private static readonly long _initialSecond = DateTime.Now.Ticks / 10000000L;

        #region Start
        protected virtual void Start()
        {
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
            // Start GetPosition() coroutine 
            StartCoroutine("GetPosition");
            // Start GetData() coroutine 
            StartCoroutine("GetData");
        }
        #endregion
        #region Update
        protected virtual void Update()
        {
            // Set any error text onto the canvas
            if (!IsEmpty(ErrorMessage) && InfoText != null)
            {
                InfoText.GetComponent<Text>().text = ErrorMessage;
                return;
            }

            long nowTicks = DateTime.Now.Ticks;
            var second = nowTicks / 10000000L;

            var arObjectState = ArObjectState;
            if (StartTicks == 0 || arObjectState == null)
            {
                string progress = string.Empty;
                for (long s = _initialSecond; s < second; s++)
                {
                    progress += ".";
                }
                InfoText.GetComponent<Text>().text = _loadingText + progress;
                return;
            }

            if (arObjectState.IsDirty)
            {
                try
                {
                    lock (arObjectState)
                    {
                        SceneAnchor.transform.eulerAngles = Vector3.zero;

                        if (arObjectState.ArObjectsToDelete.Any())
                        {
                            arObjectState.DestroyArObjects();
                        }
                        if (arObjectState.ArPois.Any())
                        {
                            CreateArObjects(arObjectState, null, SceneAnchor.transform, arObjectState.ArPois);
                            arObjectState.ArPois.Clear();
                        }
                        arObjectState.SetArObjectsToPlace();
                        arObjectState.IsDirty = false;
                    }
                }
                finally
                {
                    SceneAnchor.transform.eulerAngles = new Vector3(0, DeviceAngle - InitialHeading, 0);
                }
            }

            arObjectState.HandleAnimations(StartTicks, nowTicks);

            // Set any error text onto the canvas
            if (InfoText != null)
            {
                if (!IsEmpty(ErrorMessage))
                {
                    InfoText.GetComponent<Text>().text = ErrorMessage;
                    return;
                }

                InfoText.GetComponent<Text>().text =
                    ""
                    + "LA " + (UsedLatitude).ToString("F6")
                    + " LO " + (UsedLongitude).ToString("F6")
                    + " T " + TriggerObjects.Count
                    + " N " + arObjectState.Count
                    + " A " + arObjectState.NumberOfAnimations
                    ;
            }
        }
        #endregion
    }
}
