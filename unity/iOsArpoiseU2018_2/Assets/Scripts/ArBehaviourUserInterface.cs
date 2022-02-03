/*
ArBehaviourUserInterface.cs - MonoBehaviour for ARpoise user interface.

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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if HAS_AR_CORE
#else
#if HAS_AR_KIT
#else
#if QUEST_ARPOISE
#else
using Vuforia;
#endif
#endif
#endif

namespace com.arpoise.arpoiseapp
{
    public class ArBehaviourUserInterface : ArBehaviourData
    {
        private const string ConfirmText = "Please confirm.";
        private const string SelectingText = "Please select a layer.";
        private const string LoadingText = "Loading data, please wait";
        private long _currentSecond = InitialSecond;
        private int _framesPerCurrentSecond = 1;
        private bool _headerButtonActivated = false;
        private ArLayerScrollList _layerScrollList = null;

        protected float InitialCameraAngle = 0;
        protected bool InputPanelEnabled = true;

        public bool HasHitOnObject { get; private set; }

        #region Globals
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

        public bool MenuButtonIsActive
        {
            get { return MenuButton != null && MenuButton.activeSelf; }
            set
            {
                if (MenuButton != null)
                {
                    if (MenuButton.activeSelf != value)
                    {
                        MenuButton.SetActive(value);
                        //Debug.Log("MenuButton " + MenuButton.activeSelf);
                    }
                }
            }
        }

        public bool HeaderButtonIsActive
        {
            get { return HeaderButton != null && HeaderButton.activeSelf; }
            set
            {
                if (HeaderButton != null)
                {
                    if (HeaderButton.activeSelf != value)
                    {
                        HeaderButton.SetActive(value);
                        //Debug.Log("HeaderButton " + HeaderButton.activeSelf);
                    }
                }
            }
        }

        public bool InputPanelIsActive
        {
            get { return InputPanel != null && InputPanel.activeSelf; }
            set
            {
                if (InputPanel != null)
                {
                    if (InputPanel.activeSelf != value)
                    {
                        InputPanel.SetActive(value);
                        //Debug.Log("InputPanel " + InputPanel.activeSelf);
                    }
                }
            }
        }

        public bool LayerPanelIsActive
        {
            get { return LayerPanel != null && LayerPanel.activeSelf; }
            set
            {
                if (LayerPanel != null)
                {
                    if (LayerPanel.activeSelf != value)
                    {
                        LayerPanel.SetActive(value);
                        //Debug.Log("LayerPanel " + LayerPanel.activeSelf);
                    }
                }
            }
        }

        #region Buttons
        public override void HandleInfoPanelClosed()
        {
            //Debug.Log("HandleInfoPanelClosed");

            if (InfoPanel != null)
            {
                InfoPanel.SetActive(false);
            }
            PlayerPrefs.SetString(nameof(InfoPanelIsActive), false.ToString());
        }

        public void HandleInputPanelClosed(float? latitude, float? longitude)
        {
            //Debug.Log("HandleInputPanelClosed lat " + latitude + " lon " + longitude);

            var refreshRequest = new RefreshRequest
            {
                url = ArpoiseDirectoryUrl,
                layerName = ArpoiseDirectoryLayer,
                latitude = latitude,
                longitude = longitude
            };
            RefreshRequest = refreshRequest;
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
            MenuButtonIsActive = MenuEnabled.HasValue && MenuEnabled.Value;
        }

        public override void SetHeaderActive(string layerTitle)
        {
            if (HeaderText != null)
            {
                if (!string.IsNullOrWhiteSpace(layerTitle))
                {
                    HeaderText.GetComponent<Text>().text = layerTitle;
                    _headerButtonActivated = true;
                    HeaderButtonIsActive = _headerButtonActivated;
                }
                else
                {
                    HeaderText.GetComponent<Text>().text = string.Empty;
                    _headerButtonActivated = false;
                    HeaderButtonIsActive = _headerButtonActivated;
                }
            }
        }

        public override void HandleMenuButtonClick()
        {
            //Debug.Log("ArBehaviourUserInterface.HandleMenuButtonClick");
            if (InputPanelEnabled)
            {
                var second = DateTime.Now.Ticks / 10000000L;
                if (_lastButtonSecond == second)
                {
                    InputPanelIsActive = true;
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
                InputPanelIsActive = false;
                HeaderButtonIsActive = false;
                MenuButtonIsActive = false;
                LayerPanelIsActive = true;
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
                HeaderButtonIsActive = _headerButtonActivated;
                MenuButtonIsActive = MenuEnabled.HasValue && MenuEnabled.Value;
                LayerPanelIsActive = false;
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
                    InputPanelIsActive = true;
                    InputPanel inputPanel = InputPanel.GetComponent<InputPanel>();
                    inputPanel.Activate(this);
                }
                _lastButtonSecond = second;
            }
        }

        public void HandleLayerButtonClick(ArItem item)
        {
            //Debug.Log("HandleLayerButtonClick " + item.itemName);
            if (item != null && !string.IsNullOrWhiteSpace(item.layerName) && !string.IsNullOrWhiteSpace(item.url))
            {
                var layerName = item.layerName;
                var url = item.url;

                if (MenuEnabled.HasValue && MenuEnabled.Value)
                {
                    HeaderButtonIsActive = _headerButtonActivated;
                    MenuButtonIsActive = MenuEnabled.HasValue && MenuEnabled.Value;
                    LayerPanelIsActive = false;
                    if (_layerScrollList != null)
                    {
                        _layerScrollList.RemoveButtons();
                    }
                }

                var refreshRequest = new RefreshRequest
                {
                    url = url,
                    layerName = layerName,
                    latitude = FixedDeviceLatitude,
                    longitude = FixedDeviceLongitude
                };
                RefreshRequest = refreshRequest;
            }
        }
        #endregion

        #region Start
        protected override void Start()
        {
            base.Start();

            if (MenuButton != null)
            {
                var menuButton = MenuButton.GetComponent<MenuButton>();
                if (menuButton != null)
                {
                    menuButton.Setup(this);
                }
            }

            if (PanelHeaderButton != null)
            {
                var panelHeaderButton = PanelHeaderButton.GetComponent<PanelHeaderButton>();
                if (panelHeaderButton != null)
                {
                    panelHeaderButton.Setup(this);
                }
            }

            if (InputPanel != null)
            {
                var inputPanel = InputPanel.GetComponent<InputPanel>();
                inputPanel.Activate(null);
                FixedDeviceLatitude = inputPanel.GetLatitude();
                FixedDeviceLongitude = inputPanel.GetLongitude();
            }
        }
        #endregion

        #region Update
        protected override void Update()
        {
            base.Update();

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

            if (InputPanelIsActive)
            {
                SetInfoText("Please set the values.");
                return;
            }

            if (LayerPanelIsActive)
            {
                SetInfoText(SelectingText);
                return;
            }

            var arObjectState = ArObjectState;
            if (StartTicks == 0 || arObjectState == null)
            {
                string progress = string.Empty;
                for (long s = InitialSecond; s < CurrentSecond; s++)
                {
                    progress += ".";
                }
                SetInfoText(LoadingText + progress);
                return;
            }

            if (_currentSecond == CurrentSecond)
            {
                _framesPerCurrentSecond++;
            }
            else
            {
                if (_currentSecond == CurrentSecond - 1)
                {
                    FramesPerSecond = _framesPerCurrentSecond;
                }
                else
                {
                    FramesPerSecond = 1;
                }
                _framesPerCurrentSecond = 1;
                _currentSecond = CurrentSecond;
            }

            // Update the objects shown
            UpdateArObjects();

            // If we moved away from the current layer
            //if (!CheckDistance())
            //{
            //    InputPanel inputPanel;
            //    if (InputPanel != null && (inputPanel = InputPanel.GetComponent<InputPanel>()) != null)
            //    {
            //        HandleInputPanelClosed(inputPanel.GetLatitude(), inputPanel.GetLongitude());
            //    }
            //    else
            //    {
            //        HandleInputPanelClosed(null, null);
            //    }
            //}

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
            SceneAnchor.transform.localEulerAngles = new Vector3(0, DeviceAngle - InitialHeading, 0);

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
                        message = message.Replace("{t}", string.Empty + (_currentSecond - InitialSecond));

                        message = message.Replace("{F}", string.Empty + FramesPerSecond);
                        message = message.Replace("{N}", string.Empty + arObjectState.Count);
                        message = message.Replace("{O}", string.Empty + arObjectState.CountArObjects());
                        message = message.Replace("{A}", string.Empty + arObjectState.NumberOfAnimations);
                        message = message.Replace("{T}", string.Empty + TriggerObjects.Values.Count(x => x.isActive));
                        message = message.Replace("{S}", string.Empty + SlamObjects.Count(x => x.isActive));

                        message = message.Replace("{I}", string.Empty + (int)InitialHeading);
                        message = message.Replace("{D}", string.Empty + DeviceAngle);
                        message = message.Replace("{Y}", string.Empty + (int)SceneAnchor.transform.eulerAngles.y);
                        message = message.Replace("{H}", string.Empty + (int)HeadingShown);
                        message = message.Replace("{C}", string.Empty + (int)currentHeading);

                        message = message.Replace("{LAT}", UsedLatitude.ToString("F6", CultureInfo.InvariantCulture));
                        message = message.Replace("{LON}", UsedLongitude.ToString("F6", CultureInfo.InvariantCulture));

                        message = message.Replace("{LAT1}", (firstArObject != null ? firstArObject.Latitude : 0).ToString("F6", CultureInfo.InvariantCulture));
                        message = message.Replace("{LON1}", (firstArObject != null ? firstArObject.Longitude : 0).ToString("F6", CultureInfo.InvariantCulture));
                        message = message.Replace("{D1}", (firstArObject != null ? CalculateDistance(UsedLatitude, UsedLongitude, firstArObject.Latitude, firstArObject.Longitude) : 0).ToString("F1", CultureInfo.InvariantCulture));
                        message = message.Replace("{DNS1}", (firstArObject != null ? CalculateDistance(UsedLatitude, UsedLongitude, firstArObject.Latitude, UsedLongitude) : 0).ToString("F1", CultureInfo.InvariantCulture));
                        message = message.Replace("{DEW1}", (firstArObject != null ? CalculateDistance(UsedLatitude, UsedLongitude, UsedLatitude, firstArObject.Longitude) : 0).ToString("F1", CultureInfo.InvariantCulture));

                        message = message.Replace("{DAVF}", DisplayAnimationValueForward.ToString("F1", CultureInfo.InvariantCulture));
                        message = message.Replace("{DAVR}", DisplayAnimationValueRight.ToString("F1", CultureInfo.InvariantCulture));
                        message = message.Replace("{DGPX}", DisplayGoalPosition.x.ToString("F1", CultureInfo.InvariantCulture));
                        message = message.Replace("{DGPY}", DisplayGoalPosition.y.ToString("F1", CultureInfo.InvariantCulture));
                        message = message.Replace("{DGPZ}", DisplayGoalPosition.z.ToString("F1", CultureInfo.InvariantCulture));

                        message = message.Replace("{DSF}", DurationStretchFactor?.ToString("F2", CultureInfo.InvariantCulture));
                    }
                    SetInfoText(message);
                    return;
                }

                var text =
                    string.Empty
                    //+ "B " + _bleachingValue
                    //+ " CA " + (_locationLatitude).ToString("F6", CultureInfo.InvariantCulture)
                    //+ " A " + (_locationHorizontalAccuracy).ToString("F6", CultureInfo.InvariantCulture)
                    //+ string.Empty + (UsedLatitude).ToString("F6", CultureInfo.InvariantCulture)
                    //+ " CO " + (_locationLongitude).ToString("F6", CultureInfo.InvariantCulture)
                    //+ " " + (UsedLongitude).ToString("F6", CultureInfo.InvariantCulture)
                    //+ " AS " + _areaSize
                    //+ " F " + DisplayAnimationValueForward.ToString("F1", CultureInfo.InvariantCulture)
                    //+ " R " + DisplayAnimationValueRight.ToString("F1", CultureInfo.InvariantCulture)
                    //+ " % " + DisplayPercentage.ToString("F1", CultureInfo.InvariantCulture)
                    //+ " Z " + DisplayGoalPosition.z.ToString("F1", CultureInfo.InvariantCulture)
                    //+ " X " + DisplayGoalPosition.x.ToString("F1", CultureInfo.InvariantCulture)
                    //+ " Y " + DisplayGoalPosition.y.ToString("F1", CultureInfo.InvariantCulture)
                    //+ " Z " + (LastObject != null ? LastObject.TargetPosition : Vector3.zero).z.ToString("F1", CultureInfo.InvariantCulture)
                    //+ " X " + (LastObject != null ? LastObject.TargetPosition : Vector3.zero).x.ToString("F1", CultureInfo.InvariantCulture)
                    //+ " Y " + (LastObject != null ? LastObject.TargetPosition : Vector3.zero).y.ToString("F1", CultureInfo.InvariantCulture)
                    //+ " OH " + (firstArObject != null ? firstArObject.Latitude : 0).ToString("F6", CultureInfo.InvariantCulture)
                    //+ " OL " + (firstArObject != null ? firstArObject.Longitude : 0).ToString("F6", CultureInfo.InvariantCulture)
                    //+ " D " + (firstArObject != null ? CalculateDistance(UsedLatitude, UsedLongitude, firstArObject.Latitude, firstArObject.Longitude) : 0).ToString("F1", CultureInfo.InvariantCulture)
                    //+ " F " + _framesPerSecond
                    //+ " C " + _cameraTransform.eulerAngles.y.ToString("F", CultureInfo.InvariantCulture)
                    + " I " + (int)InitialHeading
                    + " D " + (int)DeviceAngle
                    + " Y " + (int)SceneAnchor.transform.eulerAngles.y
                    + " H " + (int)HeadingShown
                    + " C " + (int)currentHeading
                    //+ " IH " + _initialHeading.ToString("F", CultureInfo.InvariantCulture)
                    + " N " + arObjectState.ArObjects.Sum(x => x.GameObjects.Count)
                    //+ " O " + _onFocusAnimations.Count
                    //+ " R " + ray.ToString()
                    //+ " R " + ray.origin.x.ToString("F1", CultureInfo.InvariantCulture) + " " + ray.origin.y.ToString("F1", CultureInfo.InvariantCulture) + " " + ray.origin.z.ToString("F1", CultureInfo.InvariantCulture)
                    //+ " " + ray.direction.x.ToString("F1", CultureInfo.InvariantCulture) + " " + ray.direction.y.ToString("F1", CultureInfo.InvariantCulture) + " " + ray.direction.z.ToString("F1", CultureInfo.InvariantCulture)
                    + (HasHitOnObject ? " h " : string.Empty)
                    ;
                SetInfoText(text);
            }
        }

        public static Vector3 DisplayGoalPosition;
        public static float DisplayAnimationValueForward;
        public static float DisplayAnimationValueRight;
        public static float DisplayPercentage;

        public void SetInfoText(string text)
        {
            var infoText = InfoText;
            if (infoText != null)
            {
                if (!infoText.activeSelf)
                {
                    infoText.SetActive(true);
                }
                var component = infoText.GetComponent<Text>();
                if (component != null && component.text != text)
                {
                    component.text = text;
                }
            }
        }
        #endregion
    }
}
