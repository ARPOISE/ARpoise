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
        private long _currentSecond = _initialSecond;
        private int _framesPerSecond = 30;
        private int _framesPerCurrentSecond = 1;

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
            SceneAnchor.transform.eulerAngles = new Vector3(0, DeviceAngle - InitialHeading, 0);
            arObjectState.HandleAnimations(StartTicks, nowTicks);

            if (_currentSecond == second)
            {
                _framesPerCurrentSecond++;
            }
            else
            {
                if (_currentSecond == second - 1)
                {
                    _framesPerSecond = _framesPerCurrentSecond;
                }
                else
                {
                    _framesPerSecond = 1;
                }
                _framesPerCurrentSecond = 1;
                _currentSecond = second;
            }

            // Calculate heading
            var currentHeading = CurrentHeading;
            if (Math.Abs(currentHeading - HeadingShown) > 180)
            {
                if (currentHeading < HeadingShown)
                {
                    currentHeading += 360;
                }
                else
                {
                    HeadingShown += 360;
                }
            }
            HeadingShown += (currentHeading - HeadingShown) / 10;
            while (HeadingShown > 360)
            {
                HeadingShown -= 360;
            }

            // Set any error text onto the canvas
            if (!IsEmpty(ErrorMessage) && InfoText != null)
            {
                InfoText.GetComponent<Text>().text = ErrorMessage;
                return;
            }

            if (InfoText != null)
            {
                // Set info text
                if (!ShowInfo)
                {
                    InfoText.GetComponent<Text>().text = string.Empty;
                    return;
                }
                var firstArObject = arObjectState.ArObjects.FirstOrDefault();
                var firstGameObject = firstArObject?.GameObjects.FirstOrDefault();

                if (!IsEmpty(InformationMessage))
                {
                    // This is for debugging, put the strings used below into the information message of your layer
                    var message = InformationMessage;
                    if (message.Contains("{"))
                    {
                        message = message.Replace("{F}", "" + _framesPerSecond);
                        message = message.Replace("{N}", "" + arObjectState.Count);
                        message = message.Replace("{A}", "" + arObjectState.NumberOfAnimations);
                        message = message.Replace("{T}", "" + TriggerObjects.Count);

                        message = message.Replace("{I}", "" + (int)InitialHeading);
                        message = message.Replace("{D}", "" + DeviceAngle);
                        message = message.Replace("{Y}", "" + (int)SceneAnchor.transform.eulerAngles.y);
                        message = message.Replace("{H}", "" + (int)HeadingShown);
                        message = message.Replace("{C}", "" + (int)currentHeading);

                        message = message.Replace("{LAT}", UsedLatitude.ToString("F6"));
                        message = message.Replace("{LON}", UsedLongitude.ToString("F6"));

                        message = message.Replace("{LAT1}", (firstArObject != null ? firstArObject.Latitude : 0).ToString("F6"));
                        message = message.Replace("{LON1}", (firstArObject != null ? firstArObject.Longitude : 0).ToString("F6"));
                        message = message.Replace("{D1}", (firstArObject != null ? CalculateDistance(UsedLatitude, UsedLongitude, firstArObject.Latitude, firstArObject.Longitude) : 0).ToString("F1"));
                        message = message.Replace("{DNS1}", (firstArObject != null ? CalculateDistance(UsedLatitude, UsedLongitude, firstArObject.Latitude, UsedLongitude) : 0).ToString("F1"));
                        message = message.Replace("{DEW1}", (firstArObject != null ? CalculateDistance(UsedLatitude, UsedLongitude, UsedLatitude, firstArObject.Longitude) : 0).ToString("F1"));
                    }
                    InfoText.GetComponent<Text>().text = message;
                    return;
                }

                InfoText.GetComponent<Text>().text =
                    ""
                    + "" + (UsedLatitude).ToString("F6")
                    + " " + (UsedLongitude).ToString("F6")
                    + "F " + _framesPerSecond
                    + " N " + arObjectState.Count
                    + " T " + TriggerObjects.Count
                    + " A " + arObjectState.NumberOfAnimations
                    //+ " Z " + (firstGameObject != null ? firstGameObject.transform.position : Vector3.zero).z.ToString("F1")
                    //+ " X " + (firstGameObject != null ? firstGameObject.transform.position : Vector3.zero).x.ToString("F1")
                    //+ " Y " + (firstGameObject != null ? firstGameObject.transform.position : Vector3.zero).y.ToString("F1")
                    ;
            }
        }
        #endregion
    }
}
