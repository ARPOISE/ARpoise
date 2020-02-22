//-----------------------------------------------------------------------
// <copyright file="AugmentedImageExampleController.cs" company="Google">
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
AugmentedImageExampleController.cs - MonoBehaviour for setting image triggers of the Android version of image trigger ARpoise, aka AR-vos.

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
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Controller for AugmentedImage example.
    /// </summary>
    public class AugmentedImageExampleController : ArBehaviourImage
    {
#if HAS_AR_CORE
        /// <summary>
        /// A prefab for visualizing an AugmentedImage.
        /// </summary>
        public AugmentedImageVisualizer AugmentedImageVisualizerPrefab;

        private readonly Dictionary<int, AugmentedImageVisualizer> _visualizers = new Dictionary<int, AugmentedImageVisualizer>();

        private readonly List<AugmentedImage> _tempAugmentedImages = new List<AugmentedImage>();

        protected override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// The Unity Update method.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Check that motion tracking is tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            var fitToScanOverlay = FitToScanOverlay;

            if (!HasTriggerImages)
            {
                if (fitToScanOverlay != null)
                {
                    if (fitToScanOverlay.activeSelf != false)
                    {
                        fitToScanOverlay.SetActive(false);
                    }
                }
                if (_visualizers.Any())
                {
                    foreach (var visualizer in _visualizers.Values)
                    {
                        GameObject.Destroy(visualizer.gameObject);
                    }
                    _visualizers.Clear();
                }
                return;
            }

            // Get updated augmented images for this frame.
            Session.GetTrackables<AugmentedImage>(_tempAugmentedImages, TrackableQueryFilter.Updated);

            // Create visualizers and anchors for updated augmented images that are tracking and do not previously
            // have a visualizer. Remove visualizers for stopped images.
            foreach (var image in _tempAugmentedImages)
            {
                AugmentedImageVisualizer visualizer = null;
                _visualizers.TryGetValue(image.DatabaseIndex, out visualizer);
                if (image.TrackingState == TrackingState.Tracking && visualizer == null)
                {
                    TriggerObject triggerObject = null;
                    if (!TriggerObjects.TryGetValue(image.DatabaseIndex, out triggerObject))
                    {
                        ErrorMessage = "No trigger object for database index " + image.DatabaseIndex;
                    }

                    // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                    Anchor anchor = image.CreateAnchor(image.CenterPose);
                    visualizer = Instantiate(AugmentedImageVisualizerPrefab, anchor.transform);
                    visualizer.Image = image;
                    visualizer.TriggerObject = triggerObject;
                    visualizer.ArBehaviour = this;

                    _visualizers.Add(image.DatabaseIndex, visualizer);
                }
                else if (image.TrackingState == TrackingState.Stopped && visualizer != null)
                {
                    _visualizers.Remove(image.DatabaseIndex);
                    GameObject.Destroy(visualizer.gameObject);
                }
            }

            if (fitToScanOverlay != null)
            {
                // Show the fit-to-scan overlay if there are no images that are Tracking.
                var hasActiveObjects = _visualizers.Values.Any(x => x.Image.TrackingState == TrackingState.Tracking);
                var setActive = !hasActiveObjects && !LayerPanelIsActive();
                if (fitToScanOverlay.activeSelf != setActive)
                {
                    fitToScanOverlay.SetActive(setActive);
                }
            }
        }
#endif
    }
}
