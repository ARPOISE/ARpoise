/*
ArBehaviourSlam.cs - MonoBehaviour for ARpoise slam handling.

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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if HAS_AR_KIT
using UnityEngine.XR.iOS;
#endif
namespace com.arpoise.arpoiseapp
{
    public class ArBehaviourSlam : ArBehaviourImage
    {
        private readonly List<GameObject> _imageSceneObjects = new List<GameObject>();
        private readonly List<GameObject> _slamSceneObjects = new List<GameObject>();
        public readonly List<TriggerObject> VisualizedSlamObjects = new List<TriggerObject>();

        #region Globals
#if HAS_AR_KIT
        public GameObject AnchorManager;

        public GameObject PointCloudParticleExample;
        public GameObject GeneratePlanes;
        public GameObject HitAnchor;
        public GameObject DetectAPlaneOverLay;

        private bool _myIsSlam = false;
#endif

#if HAS_AR_CORE
        /// <summary>
        /// A prefab for tracking and visualizing detected planes.
        /// </summary>
        public GameObject DetectedPlanePrefab;

        // Slam example game objects
        public GameObject PlaneGenerator;
        public GameObject PlaneDiscovery;
        public GameObject PointCloud;
#endif
        #endregion

        #region Start
        protected override void Start()
        {
            base.Start();

#if HAS_AR_KIT
            if (!_imageSceneObjects.Any())
            {
                _imageSceneObjects.Add(AnchorManager);
            }
            if (!_slamSceneObjects.Any())
            {
                _slamSceneObjects.Add(PointCloudParticleExample);
                _slamSceneObjects.Add(GeneratePlanes);
            }
#endif
#if HAS_AR_CORE
            if (!_slamSceneObjects.Any())
            {
                _slamSceneObjects.Add(DetectedPlanePrefab);
                _slamSceneObjects.Add(PlaneGenerator);
                _slamSceneObjects.Add(PlaneDiscovery);
                _slamSceneObjects.Add(PointCloud);
            }
#endif
        }
        #endregion

        public List<TriggerObject> AvailableSlamObjects
        {
            get
            {
                var result = new List<TriggerObject>();

                foreach (var slamObject in SlamObjects.Where(x => x.poi != null && x.layerWebUrl == LayerWebUrl))
                {
                    var maximumCount = slamObject.poi.MaximumCount;
                    if (maximumCount > 0)
                    {
                        var count = VisualizedSlamObjects.Where(x => x.poi != null && x.poi.id == slamObject.poi.id).Count();
                        if (count >= maximumCount)
                        {
                            continue;
                        }
                    }
                    result.Add(slamObject);
                }
                return result;
            }
        }

        #region Update
        protected override void Update()
        {
            base.Update();
            var slamObjectsAvailable = AvailableSlamObjects.Any();
            foreach (var sceneObject in _imageSceneObjects)
            {
                var active = !IsSlam && HasTriggerImages;
                if (sceneObject != null && sceneObject.activeSelf != active)
                {
                    sceneObject.SetActive(active);
                    //Debug.Log($"{sceneObject.name} {active}");
                }
            }
            foreach (var sceneObject in _slamSceneObjects)
            {
                if (sceneObject != null && sceneObject.activeSelf != slamObjectsAvailable)
                {
#if HAS_AR_CORE
                    sceneObject.SetActive(slamObjectsAvailable);
                    //Debug.Log($"{sceneObject.name} {slamObjectsAvailable}");
#endif
#if HAS_AR_KIT
                    sceneObject.SetActive(slamObjectsAvailable);
                    //Debug.Log($"{sceneObject.name} {slamObjectsAvailable}");

                    if (sceneObject == GeneratePlanes)
                    {
                        var component = GeneratePlanes.GetComponent<UnityARGeneratePlane>();
                        component?.Update();
                    }
                    if (sceneObject == PointCloudParticleExample)
                    {
                        var component = PointCloudParticleExample.GetComponent<PointCloudParticleExample>();
                        component?.Update();
                    }
#endif
                }
            }
#if HAS_AR_KIT
            if (HitAnchor.activeSelf != IsSlam)
            {
                HitAnchor.SetActive(IsSlam);
                //Debug.Log($"HitAnchor {IsSlam}"); 
            }

            if (_myIsSlam != slamObjectsAvailable)
            {
                _myIsSlam = slamObjectsAvailable;

                if (DetectAPlaneOverLay != null && DetectAPlaneOverLay.gameObject.activeSelf != slamObjectsAvailable)
                {
                    DetectAPlaneOverLay.SetActive(slamObjectsAvailable);
                }
            }
#endif
        }
        #endregion
    }
}
