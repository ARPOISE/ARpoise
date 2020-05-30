/*
ArBehaviourUserInterface.cs - MonoBehaviour for Arpoise user interface.

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
    public class ArBehaviourUserInterface : ArBehaviourData
    {
        private const string ConfirmText = "Please confirm.";
        private const string SelectingText = "Please select a layer.";
        private const string LoadingText = "Loading data, please wait";
        private static readonly long _initialSecond = DateTime.Now.Ticks / 10000000L;
        private long _currentSecond = _initialSecond;
        private int _framesPerCurrentSecond = 1;
        private bool _headerButtonActivated = false;
        private ArLayerScrollList _layerScrollList = null;

        protected float InitialCameraAngle = 0;
        protected bool InputPanelEnabled = true;

        public bool HasHitOnObject { get; private set; }

        #region Globals

        public static int FramesPerSecond = 30;
        public GameObject InfoText = null;
        public GameObject MenuButton = null;
        public GameObject HeaderButton = null;
        public GameObject HeaderText = null;
        public GameObject InputPanel = null;
        public GameObject InfoPanel = null;
        public GameObject LayerPanel = null;
        public GameObject PanelHeaderButton = null;
        public Transform ContentPanel;
        public SimpleObjectPool ButtonObjectPool;

        #endregion

        public override bool InfoPanelIsActive()
        {
            return InfoPanel != null && InfoPanel.activeSelf;
        }

        public override bool InputPanelIsActive()
        {
            return InputPanel != null && InputPanel.activeSelf;
        }

        public override bool LayerPanelIsActive()
        {
            return LayerPanel != null && LayerPanel.activeSelf;
        }

        #region Buttons
        public override void HandleInfoPanelClosed()
        {
            //Debug.Log("HandleInfoPanelClosed");

            InfoPanel?.SetActive(false);
            PlayerPrefs.SetString(nameof(InfoPanelIsActive), false.ToString());
        }

        public void HandleInputPanelClosed(float? latitude, float? longitude)
        {
            //Debug.Log("HandleInputPanelClosed lat " + latitude + " lon " + longitude);

            RefreshRequest = new RefreshRequest
            {
                url = ArpoiseDirectoryUrl,
                layerName = ArpoiseDirectoryLayer,
                latitude = latitude,
                longitude = longitude
            };
        }

        private long _lastButtonSecond = 0;

        public override void SetMenuButtonActive(List<ArLayer> layers)
        {
            if (InputPanel != null && !MenuEnabled.HasValue)
            {
                var inputPanel = InputPanel.GetComponent<InputPanel>();
                if (inputPanel != null && inputPanel.IsActivated())
                {
                    MenuEnabled = true;
                }
                else
                {
                    MenuEnabled = !layers.Any(x => !x.showMenuButton);
                }
            }
            MenuButton.SetActive(MenuEnabled.HasValue && MenuEnabled.Value);
        }

        public override void SetHeaderActive(string layerTitle)
        {
            if (!string.IsNullOrWhiteSpace(layerTitle))
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

            var layerItemList = LayerItemList;
            if (MenuEnabled.HasValue && MenuEnabled.Value && layerItemList != null && layerItemList.Any())
            {
                if (_layerScrollList != null)
                {
                    _layerScrollList.RemoveButtons();
                }

                _layerScrollList = new ArLayerScrollList(ContentPanel, ButtonObjectPool);
                _layerScrollList.AddButtons(layerItemList, this);
                InputPanel.SetActive(false);
                HeaderButton.SetActive(false);
                MenuButton.SetActive(false);
                LayerPanel.SetActive(true);
            }
        }

        public void HandlePanelHeaderButtonClick()
        {
            if (ArObjectState == null)
            {
                return;
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
            if (item != null && !string.IsNullOrWhiteSpace(item.layerName) && !string.IsNullOrWhiteSpace(item.url))
            {
                //Debug.Log("HandleLayerButtonClick " + item.itemName);

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
        protected virtual void Start()
        {
            var menuButton = MenuButton.GetComponent<MenuButton>();
            menuButton?.Setup(this);

            var panelHeaderButton = PanelHeaderButton.GetComponent<PanelHeaderButton>();
            panelHeaderButton?.Setup(this);

            var inputPanel = InputPanel.GetComponent<InputPanel>();
            inputPanel.Activate(null);
            FixedDeviceLatitude = inputPanel.GetLatitude();
            FixedDeviceLongitude = inputPanel.GetLongitude();
        }
        #endregion

        #region Update
        protected virtual void Update()
        {
            var menuButtonSetActive = MenuButtonSetActive;
            if (menuButtonSetActive != null)
            {
                MenuButtonSetActive = null;
                menuButtonSetActive.Execute();
            }

            var headerSetActive = HeaderSetActive;
            if (headerSetActive != null)
            {
                HeaderSetActive = null;
                headerSetActive.Execute();
            }

            var menuButtonClick = MenuButtonClick;
            if (menuButtonClick != null)
            {
                MenuButtonClick = null;
                menuButtonClick.Execute();
            }

            if (IsNewLayer)
            {
                IsNewLayer = false;
                InitialHeading = Input.compass.trueHeading;
                HeadingShown = Input.compass.trueHeading;
            }

            // Set any error text onto the canvas
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                SetInfoText(ErrorMessage);
                return;
            }

            if (InfoPanelIsActive())
            {
                SetInfoText(ConfirmText);
                return;
            }

            if (InputPanelIsActive())
            {
                SetInfoText("Please set the values.");
                return;
            }

            if (LayerPanelIsActive())
            {
                SetInfoText(SelectingText);
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
                SetInfoText(LoadingText + progress);
                return;
            }

            if (arObjectState.IsDirty)
            {
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
            HasHitOnObject = arObjectState.HandleAnimations(StartTicks, nowTicks);

            if (_currentSecond == second)
            {
                _framesPerCurrentSecond++;
            }
            else
            {
                if (_currentSecond == second - 1)
                {
                    FramesPerSecond = _framesPerCurrentSecond;
                }
                else
                {
                    FramesPerSecond = 1;
                }
                _framesPerCurrentSecond = 1;
                _currentSecond = second;
            }

            // Set any error text onto the canvas
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                SetInfoText(ErrorMessage);
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
                        position = Vector3.Lerp(arObject.WrapperObject.transform.position, arObject.TargetPosition, .5f / FramesPerSecond);
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
                            localScale = Vector3.Lerp(arObject.WrapperObject.transform.localScale, localScale, 1f / FramesPerSecond);
                        }
                        arObject.WrapperObject.transform.localScale = localScale;
                    }
                }
            }
            SceneAnchor.transform.localEulerAngles = new Vector3(0, DeviceAngle - InitialHeading, 0);

            // Turn the ar objects
            if (CameraIsInitializing)
            {
                InitialHeading = HeadingShown;
                if (ArCamera != null)
                {
                    InitialCameraAngle = ArCamera.transform.eulerAngles.y;
                }
            }

            if (InfoText != null)
            {
                // Set info text
                if (!ShowInfo)
                {
                    SetInfoText(string.Empty);
                    return;
                }

                var firstArObject = arObjectState.ArObjects.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(InformationMessage))
                {
                    // This is for debugging, put the strings used below into the information message of your layer
                    var message = InformationMessage;
                    if (message.Contains("{"))
                    {
                        message = message.Replace("{F}", "" + FramesPerSecond);
                        message = message.Replace("{N}", "" + arObjectState.Count);
                        message = message.Replace("{A}", "" + arObjectState.NumberOfAnimations);
                        message = message.Replace("{T}", "" + TriggerObjects.Values.Count(x => x.isActive));
                        message = message.Replace("{S}", "" + SlamObjects.Values.Count(x => x.isActive));

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
                    SetInfoText(message);
                    return;
                }

                var text =
                    ""
                    //+ "B " + _bleachingValue
                    //+ " CA " + (_locationLatitude).ToString("F6")
                    //+ " A " + (_locationHorizontalAccuracy).ToString("F6")
                    //+ "" + (UsedLatitude).ToString("F6")
                    //+ " CO " + (_locationLongitude).ToString("F6")
                    //+ " " + (UsedLongitude).ToString("F6")
                    //+ " AS " + _areaSize
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
                    //+ " D " + (firstArObject != null ? CalculateDistance(UsedLatitude, UsedLongitude, firstArObject.Latitude, firstArObject.Longitude) : 0).ToString("F1")
                    //+ " F " + _framesPerSecond
                    //+ " C " + _cameraTransform.eulerAngles.y.ToString("F")
                    + " I " + (int)InitialHeading
                    + " D " + (int)DeviceAngle
                    + " Y " + (int)SceneAnchor.transform.eulerAngles.y
                    + " H " + (int)HeadingShown
                    + " C " + (int)currentHeading
                    //+ " IH " + _initialHeading.ToString("F")
                    + " N " + arObjectState.ArObjects.Sum(x => x.GameObjects.Count)
                    //+ " O " + _onFocusAnimations.Count
                    //+ " R " + ray.ToString()
                    //+ " R " + ray.origin.x.ToString("F1") + " " + ray.origin.y.ToString("F1") + " " + ray.origin.z.ToString("F1")
                    //+ " " + ray.direction.x.ToString("F1") + " " + ray.direction.y.ToString("F1") + " " + ray.direction.z.ToString("F1")
                    + (HasHitOnObject ? " h " : string.Empty)
                    ;
                SetInfoText(text);
            }
        }

        public static Vector3 DisplayGoalPosition;
        public static float DisplayAnimationValueForward;
        public static float DisplayAnimationValueRight;
        public static float DisplayPercentage;

        protected void SetInfoText(string text)
        {
            var infoText = InfoText;
            if (infoText != null)
            {
                if (!infoText.activeSelf)
                {
                    infoText.SetActive(true);
                }
                var component = infoText?.GetComponent<Text>();
                if (component != null && component.text != text)
                {
                    component.text = text;
                }
            }
        }
        #endregion
    }
}
