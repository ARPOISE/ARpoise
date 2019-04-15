//-----------------------------------------------------------------------
// <copyright file="AugmentedImageVisualizer.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

/*
AugmentedImageVisualizer.cs - MonoBehaviour for handling detected image triggers of the Android image trigger application ARvos.

This file is part of Arpoise.

This file is derived from image trigger example of the Google ARCore SDK for Unity

https://github.com/google-ar/arcore-unity-sdk

The license of the original file is shown above.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
Arpoise, see www.Arpoise.com/

*/

namespace GoogleARCore.Examples.AugmentedImage
{
    using com.arpoise.arpoiseapp;
#if HAS_AR_CORE
    using GoogleARCore;
#endif
    using UnityEngine;

    /// <summary>
    /// Uses 4 frame corner objects to visualize an AugmentedImage.
    /// </summary>
    public class AugmentedImageVisualizer : MonoBehaviour
    {
#if HAS_AR_CORE
        /// <summary>
        /// The AugmentedImage to visualize.
        /// </summary>
        public AugmentedImage Image;

        /// <summary>
        /// A model for the lower left corner of the frame to place when an image is detected.
        /// </summary>
        public GameObject FrameLowerLeft;

        /// <summary>
        /// A model for the lower right corner of the frame to place when an image is detected.
        /// </summary>
        public GameObject FrameLowerRight;

        /// <summary>
        /// A model for the upper left corner of the frame to place when an image is detected.
        /// </summary>
        public GameObject FrameUpperLeft;

        /// <summary>
        /// A model for the upper right corner of the frame to place when an image is detected.
        /// </summary>
        public GameObject FrameUpperRight;

        public TriggerObject TriggerObject { get; set; }
        public ArBehaviourImage ArBehaviour { get; set; }

        private GameObject _gameObject = null;
        private bool _gameObjectCreated = false;

        public void Start()
        {
            FrameLowerLeft.SetActive(false);
            FrameLowerRight.SetActive(false);
            FrameUpperLeft.SetActive(false);
            FrameUpperRight.SetActive(false);
        }

        private bool _first = true;

        public void Update()
        {
            if (Image == null || Image.TrackingState != TrackingState.Tracking)
            {
                if (_gameObject != null)
                {
                    _gameObject.SetActive(false);
                    _first = true;
                }
                return;
            }

            var arObjectState = ArBehaviour.ArObjectState;
            if (arObjectState != null && TriggerObject != null && !_gameObjectCreated)
            {
                _gameObjectCreated = true;
                //_gameObject = Instantiate(TriggerObject.gameObject);

                lock (arObjectState)
                {
                    transform.position = Image.CenterPose.position;
                    transform.rotation = Image.CenterPose.rotation;

                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        TriggerObject.gameObject,
                        null,
                        transform,
                        TriggerObject.poi,
                        TriggerObject.poi.id,
                        out _gameObject
                        );
                    if (!ArBehaviourPosition.IsEmpty(result))
                    {
                        ArBehaviour.ErrorMessage = result;
                        return;
                    }
                }
            }

            if (_gameObject != null)
            {
                _gameObject.SetActive(true);
                if (_first)
                {
                    _first = false;
                    _gameObject.transform.position = Image.CenterPose.position;
                    _gameObject.transform.rotation = Image.CenterPose.rotation;
                }
                else
                {
                    _gameObject.transform.position = Vector3.Lerp(_gameObject.transform.position, Image.CenterPose.position, 0.02f);
                    _gameObject.transform.rotation = Quaternion.Lerp(_gameObject.transform.rotation, Image.CenterPose.rotation, 0.02f);
                }
            }
        }
#endif
    }
}
