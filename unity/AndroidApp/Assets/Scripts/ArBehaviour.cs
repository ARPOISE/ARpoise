/*
ArBehaviour.cs - MonoBehaviour for Arpoise.

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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if HAS_AR_CORE
#else
#if HAS_AR_KIT
#else
using Vuforia;
#endif
#endif

namespace com.arpoise.arpoiseapp
{
    public class ArBehaviour : ArBehaviourData
    {
        private static readonly string _selectingText = "Please select a layer.";
        private static readonly string _loadingText = "Loading data, please wait";
        private static readonly long _initialSecond = DateTime.Now.Ticks / 10000000L;
        private long _currentSecond = _initialSecond;
        private int _framesPerSecond = 30;
        private int _framesPerCurrentSecond = 1;
        private bool _headerButtonActivated = false;
        private ArLayerScrollList _layerScrollList = null;

        protected float InitialCameraAngle = 0;
        protected bool InputPanelEnabled = true;

        #region Globals

        public GameObject InfoText = null;
        public GameObject HeaderText = null;
        public GameObject LayerPanel = null;
        public GameObject HeaderButton = null;
        public GameObject PanelHeaderButton = null;
        public Transform ContentPanel;
        public SimpleObjectPool ButtonObjectPool;
        #endregion

        #region Buttons
        public void HandleInputPanelClosed(float? latitude, float? longitude)
        {
            Debug.Log("HandleInputPanelClosed lat " + latitude + " lon " + longitude);

            RefreshRequest = new RefreshRequest
            {
                url = ArpoiseDirectoryUrl,
                layerName = ArpoiseDirectoryLayer,
                latitude = latitude,
                longitude = longitude
            };
        }

        private long _lastButtonSecond = 0;

        protected override void SetHeaderActive(string layerTitle)
        {
            if (!IsEmpty(layerTitle))
            {
                HeaderText.GetComponent<Text>().text = layerTitle;
                _headerButtonActivated = true;
                HeaderButton.SetActive(_headerButtonActivated);
            }
            else
            {
                HeaderText.GetComponent<Text>().text = string.Empty;
                _headerButtonActivated = false;
                HeaderButton.SetActive(_headerButtonActivated);
            }
        }

        public override void HandleMenuButtonClick()
        {
            if (InputPanelEnabled)
            {
                var second = DateTime.Now.Ticks / 10000000L;
                if (_lastButtonSecond == second)
                {
                    InputPanel.SetActive(true);
                    var inputPanel = InputPanel.GetComponent<InputPanel>();
                    inputPanel.Activate(this);
                    return;
                }
                _lastButtonSecond = second;
            }

            List<ArItem> itemList = LayerItemList;
            if (MenuEnabled.HasValue && MenuEnabled.Value && itemList != null && itemList.Any())
            {
                if (_layerScrollList != null)
                {
                    _layerScrollList.RemoveButtons();
                }

                _layerScrollList = new ArLayerScrollList(ContentPanel, ButtonObjectPool);
                _layerScrollList.AddButtons(itemList, this);

                InputPanel.SetActive(false);
                HeaderButton.SetActive(false);
                MenuButton.SetActive(false);
                LayerPanel.SetActive(true);
            }
        }

        public void HandlePanelHeaderButtonClick()
        {
            if (MenuEnabled.HasValue && MenuEnabled.Value)
            {
                HeaderButton.SetActive(_headerButtonActivated);
                MenuButton.SetActive(MenuEnabled.HasValue && MenuEnabled.Value);
                LayerPanel.SetActive(false);
                if (_layerScrollList != null)
                {
                    _layerScrollList.RemoveButtons();
                }
            }
            if (InputPanelEnabled)
            {
                var second = DateTime.Now.Ticks / 10000000L;
                if (_lastButtonSecond == second)
                {
                    InputPanel.SetActive(true);
                    InputPanel inputPanel = InputPanel.GetComponent<InputPanel>();
                    inputPanel.Activate(this);
                }
                _lastButtonSecond = second;
            }
        }

        public void HandleLayerButtonClick(ArItem item)
        {
            if (item != null && !IsEmpty(item.layerName) && !IsEmpty(item.url))
            {
                Debug.Log("HandleLayerButtonClick " + item.itemName);

                RefreshRequest = new RefreshRequest
                {
                    url = item.url,
                    layerName = item.layerName,
                    latitude = FixedDeviceLatitude,
                    longitude = FixedDeviceLongitude
                };
            }
            if (MenuEnabled.HasValue && MenuEnabled.Value)
            {
                HeaderButton.SetActive(_headerButtonActivated);
                MenuButton.SetActive(MenuEnabled.HasValue && MenuEnabled.Value);
                LayerPanel.SetActive(false);
                if (_layerScrollList != null)
                {
                    _layerScrollList.RemoveButtons();
                }
            }
        }
        #endregion

        #region Start
        private void Start()
        {
#if UNITY_EDITOR
            Debug.Log("UNITY_EDITOR Start");
#endif
            var inputPanel = InputPanel.GetComponent<InputPanel>();
            inputPanel.Activate(null);
            FixedDeviceLatitude = inputPanel.GetLatitude();
            FixedDeviceLongitude = inputPanel.GetLongitude();

#if HAS_AR_CORE
#else
#if HAS_AR_KIT
#else
            ArCamera.GetComponent<VuforiaBehaviour>().enabled = true;
            VuforiaRuntime.Instance.InitVuforia();
#endif
#endif
            // Start GetPosition() coroutine 
            StartCoroutine("GetPosition");
            // Start GetData() coroutine 
            StartCoroutine("GetData");
        }
        #endregion

        #region Update
        private void Update()
        {
            // Set any error text onto the canvas
            if (!IsEmpty(ErrorMessage) && InfoText != null)
            {
                InfoText.GetComponent<Text>().text = ErrorMessage;
                return;
            }

            if (WaitingForLayerSelection)
            {
                InfoText.GetComponent<Text>().text = _selectingText;
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

            // Set any error text onto the canvas
            if (!IsEmpty(ErrorMessage) && InfoText != null)
            {
                InfoText.GetComponent<Text>().text = ErrorMessage;
                return;
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

            // Place the ar objects
            try
            {
                SceneAnchor.transform.eulerAngles = Vector3.zero;

                var arObjectsToPlace = arObjectState.ArObjectsToPlace;
                if (arObjectsToPlace != null)
                {
                    foreach (var arObject in arObjectsToPlace)
                    {
                        var jump = false;

                        // Linearly interpolate from current position to target position
                        Vector3 position;
                        if (AreaSize > 0 && AreaWidth > 0
                            && (Math.Abs(arObject.WrapperObject.transform.position.x - arObject.TargetPosition.x) > AreaWidth * .75
                            || Math.Abs(arObject.WrapperObject.transform.position.z - arObject.TargetPosition.z) > AreaSize * .75))
                        {
                            // Jump if area handling is active and distance is too big
                            position = new Vector3(arObject.TargetPosition.x, arObject.TargetPosition.y, arObject.TargetPosition.z);
                            jump = true;
                        }
                        else
                        {
                            position = Vector3.Lerp(arObject.WrapperObject.transform.position, arObject.TargetPosition, .5f / _framesPerSecond);
                        }
                        arObject.WrapperObject.transform.position = position;

                        if (AreaSize > 0)
                        {
                            // Scale the objects at the edge of the area
                            var scale = arObject.Scale;
                            if (scale < 0)
                            {
                                scale = 1;
                            }
                            Vector3 localScale;
                            if (jump)
                            {
                                if (scale < 1)
                                {
                                    scale = 0.01f;
                                }
                                localScale = new Vector3(scale, scale, scale);
                            }
                            else
                            {
                                localScale = new Vector3(scale, scale, scale);
                                localScale = Vector3.Lerp(arObject.WrapperObject.transform.localScale, localScale, 1f / _framesPerSecond);
                            }
                            arObject.WrapperObject.transform.localScale = localScale;
                        }
                    }
                }
            }
            finally
            {
                SceneAnchor.transform.eulerAngles = new Vector3(0, DeviceAngle - InitialHeading, 0);
            }

            // Turn the ar objects
            if (CameraIsInitializing)
            {
                InitialHeading = HeadingShown;
                InitialCameraAngle = ArCamera.transform.eulerAngles.y;
                foreach (var arObject in arObjectState.ArObjects)
                {
                    arObject.WrapperObject.transform.eulerAngles = new Vector3(0, DeviceAngle - InitialHeading, 0);
                }
            }

            if (InfoText != null)
            {
                // Set info text
                if (!ShowInfo)
                {
                    InfoText.GetComponent<Text>().text = string.Empty;
                    return;
                }
                if (!IsEmpty(InformationMessage))
                {
                    InfoText.GetComponent<Text>().text = InformationMessage;
                    return;
                }

                var firstArObject = arObjectState.ArObjects.FirstOrDefault();
                InfoText.GetComponent<Text>().text =
                    ""
                    //+ "B " + _bleachingValue
                    //+ "CLT " + (_locationTimestamp).ToString("F3")
                    //+ " CA " + (_locationLatitude).ToString("F6")
                    //+ " A " + (_locationHorizontalAccuracy).ToString("F6")
                    + "LA " + (UsedLatitude).ToString("F6")
                    //+ " CO " + (_locationLongitude).ToString("F6")
                    + " LO " + (UsedLongitude).ToString("F6")
                    //+ " AS " + _areaSize
                    //+ " AV " + AnimationValue.ToString("F3")
                    //+ " F " + DisplayAnimationValueForward.ToString("F1")
                    //+ " R " + DisplayAnimationValueRight.ToString("F1")
                    //+ " % " + DisplayPercentage.ToString("F1")
                    //+ " Z " + DisplayGoalPosition.z.ToString("F1")
                    //+ " X " + DisplayGoalPosition.x.ToString("F1")
                    //+ " Y " + DisplayGoalPosition.y.ToString("F1")
                    //+ " Z " + (LastObject != null ? LastObject.TargetPosition : Vector3.zero).z.ToString("F1")
                    //+ " X " + (LastObject != null ? LastObject.TargetPosition : Vector3.zero).x.ToString("F1")
                    //+ " Y " + (LastObject != null ? LastObject.TargetPosition : Vector3.zero).y.ToString("F1")
                    //+ " OH " + (firstArObject != null ? firstArObject.Latitude : 0).ToString("F6")
                    //+ " OL " + (firstArObject != null ? firstArObject.Longitude : 0).ToString("F6")
                    + " D " + (firstArObject != null ? CalculateDistance(UsedLatitude, UsedLongitude, firstArObject.Latitude, firstArObject.Longitude) : 0).ToString("F1")
                    + " F " + _framesPerSecond
                    //+ " C " + _cameraTransform.eulerAngles.y.ToString("F")
                    //+ " IC " + _initialCameraAngle.ToString("F")
                    //+ " SA " + _sceneAnchor.transform.eulerAngles.y.ToString("F")
                    + " H " + (int)HeadingShown
                    //+ " IH " + _initialHeading.ToString("F")
                    + " N " + arObjectState.ArObjects.Sum(x => x.GameObjects.Count)
                    //+ " O " + _onFocusAnimations.Count
                    //+ " R " + ray.ToString()
                    //+ " R " + ray.origin.x.ToString("F1") + " " + ray.origin.y.ToString("F1") + " " + ray.origin.z.ToString("F1")
                    //+ " " + ray.direction.x.ToString("F1") + " " + ray.direction.y.ToString("F1") + " " + ray.direction.z.ToString("F1")
                    ;
            }
        }

        public static Vector3 DisplayGoalPosition;
        public static float DisplayAnimationValueForward;
        public static float DisplayAnimationValueRight;
        public static float DisplayPercentage;

        #endregion
    }
}
